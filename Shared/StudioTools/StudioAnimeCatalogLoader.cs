using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using Studio;
using UnityEngine;

namespace MiunaKKHelper.StudioTools
{
    /// <summary>
    /// 从 studio/info 与 studio/anime AB 加载 CharaStudio 动作表（Anime_* / HAnime_* Excel）。
    /// 不依赖 isLoadList 或 LoadExcelDataCoroutine 迭代器反编译。
    /// </summary>
    public static class StudioAnimeCatalogLoader
    {
        const string StudioInfoBundle = "studio/info/00.unity3d";
        const string StudioAnimeBundle = "studio/anime/00.unity3d";
        const string AssetAnimeGroup = "AnimeGroup_";
        const string RegexAnimeCategory = @"AnimeCategory_(\d*)_(\d*)";
        const string RegexAnime = @"Anime_(\d*)_(\d*)_(\d*)";
        const string RegexHAnime = @"HAnime_(\d*)_(\d*)_(\d*)";

        public static bool TryLoadSync()
        {
            Info info = Singleton<Info>.Instance;
            if (info == null)
            {
                MiunaHelperHost.Logger.LogWarning("[AnimExport] Singleton<Info> 不可用（需处于 CharaStudio）");
                return false;
            }

            if (info.dicAnimeLoadInfo != null && info.dicAnimeLoadInfo.Count > 0)
                return true;

            try
            {
                EnsureFileCheck(info);
                LoadViaReflection(info);
                int count = CountAnimeEntries(info.dicAnimeLoadInfo);
                if (count > 0)
                {
                    MiunaHelperHost.Logger.LogInfo("[AnimExport] studio/anime 同步加载 " + count + " 条");
                    return true;
                }

                MiunaHelperHost.Logger.LogWarning("[AnimExport] studio/anime 同步加载为 0，将尝试官方 LoadExcelDataCoroutine");
                return false;
            }
            catch (Exception ex)
            {
                MiunaHelperHost.Logger.LogError("[AnimExport] studio/anime 同步加载失败: " + ex.Message);
                return false;
            }
        }

        public static IEnumerator EnsureLoadedCoroutine(MonoBehaviour host)
        {
            if (host == null)
                yield break;

            Info info = Singleton<Info>.Instance;
            if (info == null)
                yield break;

            if (info.dicAnimeLoadInfo != null && info.dicAnimeLoadInfo.Count > 0)
                yield break;

            if (TryLoadSync())
                yield break;

            MiunaHelperHost.Logger.LogInfo("[AnimExport] 运行 Info.LoadExcelDataCoroutine …");
            yield return info.LoadExcelDataCoroutine();

            const float timeout = 90f;
            float elapsed = 0f;
            while (elapsed < timeout)
            {
                if (info.dicAnimeLoadInfo != null && info.dicAnimeLoadInfo.Count > 0)
                {
                    MiunaHelperHost.Logger.LogInfo("[AnimExport] Coroutine 加载完成: " + CountAnimeEntries(info.dicAnimeLoadInfo) + " 条");
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            MiunaHelperHost.Logger.LogWarning("[AnimExport] LoadExcelDataCoroutine 超时，studio 表仍为空");
        }

        static void LoadViaReflection(Info info)
        {
            MethodInfo loadExcel = AccessTools.Method(typeof(Info), "LoadExcelData");
            MethodInfo loadGroup = AccessTools.Method(typeof(Info), "LoadAnimeGroupInfo");
            MethodInfo loadCategoryTree = AccessTools.Method(typeof(Info), "LoadAnimeCategoryInfo", new[] { typeof(string), typeof(string), typeof(System.Collections.Generic.Dictionary<int, Info.GroupInfo>) });
            MethodInfo findAll = AccessTools.Method(typeof(Info), "FindAllAssetName");
            MethodInfo loadAnime = AccessTools.Method(typeof(Info), "LoadAnimeLoadInfo");
            MethodInfo loadHAnime = AccessTools.Method(typeof(Info), "LoadHAnimeLoadInfo");

            if (loadExcel == null || findAll == null || loadAnime == null || loadHAnime == null)
                throw new InvalidOperationException("Info 反射方法缺失");

            if (loadGroup != null)
            {
                object groupEd = loadExcel.Invoke(info, new object[] { StudioInfoBundle, AssetAnimeGroup });
                loadGroup.Invoke(info, new[] { groupEd, info.dicAGroupCategory });
            }

            if (loadCategoryTree != null)
                loadCategoryTree.Invoke(info, new object[] { StudioInfoBundle, RegexAnimeCategory, info.dicAGroupCategory });

            LoadAnimeExcelFiles(info, findAll, loadExcel, loadAnime, StudioAnimeBundle, RegexAnime);
            LoadAnimeExcelFiles(info, findAll, loadExcel, loadHAnime, StudioAnimeBundle, RegexHAnime);
        }

        static void LoadAnimeExcelFiles(Info info, MethodInfo findAll, MethodInfo loadExcel, MethodInfo loader, string bundlePath, string regex)
        {
            var target = info.dicAnimeLoadInfo;
            string[] assetNames = findAll.Invoke(info, new object[] { bundlePath, regex }) as string[];
            if (assetNames == null || assetNames.Length == 0)
            {
                MiunaHelperHost.Logger.LogWarning("[AnimExport] 未找到 Excel 资产: " + bundlePath + " /" + regex);
                return;
            }

            int files = 0;
            foreach (string assetName in assetNames)
            {
                object ed = loadExcel.Invoke(info, new object[] { bundlePath, assetName });
                if (ed == null)
                    continue;
                loader.Invoke(info, new object[] { ed, target });
                files++;
            }

            MiunaHelperHost.Logger.LogInfo("[AnimExport] " + bundlePath + " [" + regex + "] 读取 " + files + " 个 Excel");
        }

        static void EnsureFileCheck(Info info)
        {
            FieldInfo field = AccessTools.Field(typeof(Info), "fileCheck");
            if (field == null)
                return;
            if (field.GetValue(info) != null)
                return;

            Type fileCheckType = AccessTools.Inner(typeof(Info), "FileCheck");
            if (fileCheckType == null)
                return;

            field.SetValue(info, Activator.CreateInstance(fileCheckType));
        }

        static int CountAnimeEntries(System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, Info.AnimeLoadInfo>>> dic)
        {
            if (dic == null) return 0;
            int count = 0;
            foreach (var g in dic.Values)
            {
                if (g == null) continue;
                foreach (var c in g.Values)
                {
                    if (c == null) continue;
                    count += c.Count;
                }
            }
            return count;
        }
    }
}
