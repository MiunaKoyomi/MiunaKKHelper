using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using KKAPI;
using Studio;
using UnityEngine;

namespace MiunaKKHelper.StudioTools
{
    /// <summary>
    /// 导出 Studio 动作 displayName 与 Controller/AB 映射。
    /// CharaStudio 使用 Info.dicAnimeLoadInfo；h/list 表通过反射调用 HSceneProc.CreateAllAnimationList。
    /// </summary>
    public static class AnimationListExporter
    {
        const string ExportDir = "UserData/MiunaExports";
        const string ExportFileName = "AnimationCatalog.json";

        static List<AnimationEntry> _cached = new List<AnimationEntry>();
        static bool _hasCache;

        public static bool HasCache => _hasCache && _cached.Count > 0;

        public static int CachedCount => _cached.Count;

        /// <summary>导出前主动抓取：Studio 表 + h/list 全量。</summary>
        public static void TryCaptureCurrent()
        {
            _cached.Clear();
            _hasCache = false;

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                StudioAnimeCatalogLoader.TryLoadSync();
                CaptureFromStudioInfo(append: true);
            }

            CaptureFromHListViaReflection(append: true);

            _hasCache = _cached.Count > 0;
            if (_hasCache)
            {
                int studioInCache = 0;
                int hlistInCache = 0;
                foreach (AnimationEntry e in _cached)
                {
                    if (e.source == "studio") studioInCache++;
                    else if (e.source == "hlist") hlistInCache++;
                }
                MiunaHelperHost.Logger.LogInfo("[AnimExport] 已缓存 " + _cached.Count + " 条 (studio=" + studioInCache + ", hlist=" + hlistInCache + ")");
            }
            else
                MiunaHelperHost.Logger.LogWarning("[AnimExport] 未能加载任何动作数据。Studio 需等加载完成；h/list 需游戏 abdata 可读。");
        }

        /// <summary>协程版：等待 studio/anime 加载后再抓取。</summary>
        public static IEnumerator TryCaptureCurrentCoroutine(MonoBehaviour host)
        {
            _cached.Clear();
            _hasCache = false;

            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio && host != null)
                yield return StudioAnimeCatalogLoader.EnsureLoadedCoroutine(host);

            TryCaptureCurrent();
        }

        public static void CaptureFromHSceneProc(HSceneProc proc)
        {
            CaptureFromHSceneProc(proc, "hlist", append: false);
        }

        static int CaptureFromHSceneProc(HSceneProc proc, string source, bool append)
        {
            if (!append)
            {
                _cached.Clear();
                _hasCache = false;
            }

            if (proc == null) return 0;

            FieldInfo field = typeof(HSceneProc).GetField("lstAnimInfo",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                MiunaHelperHost.Logger.LogError("[AnimExport] 未找到 HSceneProc.lstAnimInfo");
                return 0;
            }

            var groups = field.GetValue(proc) as List<HSceneProc.AnimationListInfo>[];
            if (groups == null) return 0;

            int added = 0;
            for (int modeIndex = 0; modeIndex < groups.Length; modeIndex++)
            {
                List<HSceneProc.AnimationListInfo> group = groups[modeIndex];
                if (group == null) continue;

                string modeName = ModeIndexToName(modeIndex);
                foreach (HSceneProc.AnimationListInfo info in group)
                {
                    if (info == null) continue;
                    _cached.Add(AnimationEntry.FromListInfo(info, modeIndex, modeName, source));
                    added++;
                }
            }

            if (!append)
                _hasCache = added > 0;

            return added;
        }

        static int CaptureFromStudioInfo(bool append)
        {
            if (!append)
            {
                _cached.Clear();
                _hasCache = false;
            }

            Info info = Singleton<Info>.Instance;
            if (info == null)
            {
                MiunaHelperHost.Logger.LogWarning("[AnimExport] Singleton<Info> 不可用");
                return 0;
            }

            if (info.dicAnimeLoadInfo == null || info.dicAnimeLoadInfo.Count == 0)
            {
                if (!info.isLoadList)
                    MiunaHelperHost.Logger.LogWarning("[AnimExport] dicAnimeLoadInfo 为空（isLoadList=false），已尝试 studio/anime 直读");
                return 0;
            }

            int added = 0;
            foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, Info.AnimeLoadInfo>>> groupPair in info.dicAnimeLoadInfo)
            {
                int group = groupPair.Key;
                if (groupPair.Value == null) continue;

                foreach (KeyValuePair<int, Dictionary<int, Info.AnimeLoadInfo>> categoryPair in groupPair.Value)
                {
                    int category = categoryPair.Key;
                    if (categoryPair.Value == null) continue;

                    foreach (KeyValuePair<int, Info.AnimeLoadInfo> animPair in categoryPair.Value)
                    {
                        if (animPair.Value == null) continue;
                        _cached.Add(AnimationEntry.FromStudioInfo(info, animPair.Value, group, category, animPair.Key));
                        added++;
                    }
                }
            }

            if (!append)
                _hasCache = added > 0;

            return added;
        }

        static int CaptureFromHListViaReflection(bool append)
        {
            GameObject go = null;
            try
            {
                go = new GameObject(AnimationListExporter_CreateAllAnimationList_Patch.TempExportObjectName);
                go.hideFlags = HideFlags.HideAndDontSave;
                HSceneProc proc = go.AddComponent<HSceneProc>();

                MethodInfo method = AccessTools.Method(typeof(HSceneProc), "CreateAllAnimationList");
                if (method == null)
                {
                    MiunaHelperHost.Logger.LogError("[AnimExport] 未找到 HSceneProc.CreateAllAnimationList");
                    return 0;
                }

                method.Invoke(proc, null);
                return CaptureFromHSceneProc(proc, "hlist", append);
            }
            catch (Exception ex)
            {
                MiunaHelperHost.Logger.LogError("[AnimExport] h/list 反射加载失败: " + ex.Message);
                return 0;
            }
            finally
            {
                if (go != null)
                    UnityEngine.Object.Destroy(go);
            }
        }

        public static string ExportToUserData()
        {
            if (!_hasCache || _cached.Count == 0)
            {
                MiunaHelperHost.Logger.LogWarning("[AnimExport] 缓存为空，正在尝试重新加载…");
                TryCaptureCurrent();
            }

            if (!_hasCache || _cached.Count == 0)
                return null;

            string dir = Path.Combine(Paths.GameRootPath, ExportDir);
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, ExportFileName);

            string json = BuildJson(_cached);
            File.WriteAllText(path, json, Encoding.UTF8);
            MiunaHelperHost.Logger.LogInfo("[AnimExport] 已导出 " + _cached.Count + " 条 → " + path);
            return path;
        }

        static string ModeIndexToName(int modeIndex)
        {
            try
            {
                if (Enum.IsDefined(typeof(HFlag.EMode), modeIndex))
                    return Enum.GetName(typeof(HFlag.EMode), modeIndex);
            }
            catch
            {
                // ignored
            }

            return "mode_" + modeIndex.ToString("00");
        }

        static string BuildJson(List<AnimationEntry> entries)
        {
            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"exportedAt\": \"").Append(Escape(DateTime.Now.ToString("o"))).Append("\",\n");
            sb.Append("  \"count\": ").Append(entries.Count).Append(",\n");
            sb.Append("  \"animations\": [\n");

            for (int i = 0; i < entries.Count; i++)
            {
                AnimationEntry e = entries[i];
                sb.Append("    {\n");
                sb.Append("      \"source\": \"").Append(Escape(e.source)).Append("\",\n");
                sb.Append("      \"displayName\": \"").Append(Escape(e.displayName)).Append("\",\n");
                sb.Append("      \"groupName\": \"").Append(Escape(e.groupName)).Append("\",\n");
                sb.Append("      \"categoryName\": \"").Append(Escape(e.categoryName)).Append("\",\n");
                sb.Append("      \"studioPath\": \"").Append(Escape(e.studioPath)).Append("\",\n");
                sb.Append("      \"studioId\": ").Append(e.studioId).Append(",\n");
                sb.Append("      \"modeIndex\": ").Append(e.modeIndex).Append(",\n");
                sb.Append("      \"mode\": \"").Append(Escape(e.mode)).Append("\",\n");
                sb.Append("      \"group\": ").Append(e.group).Append(",\n");
                sb.Append("      \"category\": ").Append(e.category).Append(",\n");
                sb.Append("      \"no\": ").Append(e.no).Append(",\n");
                sb.Append("      \"isHAnime\": ").Append(e.isHAnime ? "true" : "false").Append(",\n");
                sb.Append("      \"clip\": \"").Append(Escape(e.clip)).Append("\",\n");
                sb.Append("      \"controllerFemale\": \"").Append(Escape(e.controllerFemale)).Append("\",\n");
                sb.Append("      \"controllerMale\": \"").Append(Escape(e.controllerMale)).Append("\",\n");
                sb.Append("      \"controllerFemale1\": \"").Append(Escape(e.controllerFemale1)).Append("\",\n");
                sb.Append("      \"abFemale\": \"").Append(Escape(e.abFemale)).Append("\",\n");
                sb.Append("      \"abMale\": \"").Append(Escape(e.abMale)).Append("\",\n");
                sb.Append("      \"abFemale1\": \"").Append(Escape(e.abFemale1)).Append("\"\n");
                sb.Append("    }");
                if (i < entries.Count - 1) sb.Append(",");
                sb.Append("\n");
            }

            sb.Append("  ]\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        static string SafePathFile(HSceneProc.PathName path)
        {
            return path != null && path.file != null ? path.file : string.Empty;
        }

        static string SafePathAsset(HSceneProc.PathName path)
        {
            return path != null && path.assetpath != null ? path.assetpath : string.Empty;
        }

        sealed class AnimationEntry
        {
            public string source;
            public string displayName;
            public string groupName;
            public string categoryName;
            public string studioPath;
            public int studioId;
            public int modeIndex;
            public string mode;
            public int group;
            public int category;
            public int no;
            public bool isHAnime;
            public string clip;
            public string controllerFemale;
            public string controllerMale;
            public string controllerFemale1;
            public string abFemale;
            public string abMale;
            public string abFemale1;

            public static AnimationEntry FromListInfo(HSceneProc.AnimationListInfo info, int modeIndex, string modeName, string source)
            {
                string displayName = info.nameAnimation ?? string.Empty;
                StudioAnimeCategoryResolver.ResolveHList(modeName, displayName, out string groupName, out string categoryName, out string studioPath);

                return new AnimationEntry
                {
                    source = source ?? "hlist",
                    displayName = displayName,
                    groupName = groupName,
                    categoryName = categoryName,
                    studioPath = studioPath,
                    studioId = info.id,
                    modeIndex = modeIndex,
                    mode = modeName ?? string.Empty,
                    group = -1,
                    category = -1,
                    no = -1,
                    isHAnime = true,
                    clip = string.Empty,
                    controllerFemale = SafePathFile(info.paramFemale != null ? info.paramFemale.path : null),
                    controllerMale = SafePathFile(info.paramMale != null ? info.paramMale.path : null),
                    controllerFemale1 = SafePathFile(info.paramFemale1 != null ? info.paramFemale1.path : null),
                    abFemale = SafePathAsset(info.paramFemale != null ? info.paramFemale.path : null),
                    abMale = SafePathAsset(info.paramMale != null ? info.paramMale.path : null),
                    abFemale1 = SafePathAsset(info.paramFemale1 != null ? info.paramFemale1.path : null)
                };
            }

            public static AnimationEntry FromStudioInfo(Info info, Info.AnimeLoadInfo loadInfo, int group, int category, int no)
            {
                string displayName = loadInfo.name ?? string.Empty;
                StudioAnimeCategoryResolver.ResolveStudio(info, group, category, displayName, out string groupName, out string categoryName, out string studioPath);

                var entry = new AnimationEntry
                {
                    source = "studio",
                    displayName = displayName,
                    groupName = groupName,
                    categoryName = categoryName,
                    studioPath = studioPath,
                    studioId = -1,
                    modeIndex = -1,
                    mode = string.Empty,
                    group = group,
                    category = category,
                    no = no,
                    isHAnime = loadInfo is Info.HAnimeLoadInfo,
                    clip = loadInfo.clip ?? string.Empty,
                    controllerFemale = loadInfo.fileName ?? string.Empty,
                    controllerMale = string.Empty,
                    controllerFemale1 = string.Empty,
                    abFemale = loadInfo.bundlePath ?? string.Empty,
                    abMale = string.Empty,
                    abFemale1 = string.Empty
                };

                Info.HAnimeLoadInfo hInfo = loadInfo as Info.HAnimeLoadInfo;
#if KKS
                if (hInfo != null && hInfo.overrideFile != null && hInfo.overrideFile.Check)
#else
                if (hInfo != null && hInfo.overrideFile != null && hInfo.overrideFile.check)
#endif
                {
                    entry.controllerMale = hInfo.overrideFile.fileName ?? string.Empty;
                    entry.abMale = hInfo.overrideFile.bundlePath ?? string.Empty;
                }

                return entry;
            }
        }
    }

    [HarmonyPatch(typeof(HSceneProc), "CreateAllAnimationList")]
    static class AnimationListExporter_CreateAllAnimationList_Patch
    {
        internal const string TempExportObjectName = "MiunaKK_AnimExportTemp";

        static void Postfix(HSceneProc __instance)
        {
            // 导出用临时 HSceneProc 会触发此方法；若不清除则 Postfix 会 wipe 已缓存的 studio 条目
            if (__instance == null || __instance.gameObject == null)
                return;
            if (__instance.gameObject.name == TempExportObjectName)
                return;

            AnimationListExporter.CaptureFromHSceneProc(__instance);
        }
    }
}
