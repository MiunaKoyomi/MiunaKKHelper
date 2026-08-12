using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx.Logging;
using KKAPI;
using KKAPI.Studio;
using Studio;
using UnityEngine;

namespace MiunaKKHelper.StudioTools;

/// <summary>
/// 把 Studio 场景里的角色提取成独立角色卡（png）。
/// 对齐 DeathWeasel Character Export：chaFile.SaveCharaFile，写入 UserData/chara/{male|female}。
/// </summary>
public static class StudioCharacterCardExport
{
    public static string LastExportDirectory { get; private set; }
    public static string LastResult { get; private set; }

    public static int CountSelectedCharacters()
    {
        int n = 0;
        foreach (OCIChar unused in EnumerateSelectedCharacters())
            n++;
        return n;
    }

    public static int CountSceneCharacters()
    {
        int n = 0;
        foreach (OCIChar unused in EnumerateSceneCharacters())
            n++;
        return n;
    }

    public static int ExportSelected()
    {
        var list = new List<OCIChar>();
        foreach (OCIChar oci in EnumerateSelectedCharacters())
            list.Add(oci);

        if (list.Count == 0)
        {
            LastResult = "未选中角色（Workspace 里点角色）";
            MiunaHelperHost.Logger.LogWarning("[CardExport] " + LastResult);
            return 0;
        }

        return ExportList(list, "selected");
    }

    public static int ExportAllInScene()
    {
        var list = new List<OCIChar>();
        foreach (OCIChar oci in EnumerateSceneCharacters())
            list.Add(oci);

        if (list.Count == 0)
        {
            LastResult = "场景里没有角色";
            MiunaHelperHost.Logger.LogWarning("[CardExport] " + LastResult);
            return 0;
        }

        return ExportList(list, "scene");
    }

    public static void OpenLastExportFolder()
    {
        if (string.IsNullOrEmpty(LastExportDirectory) || !Directory.Exists(LastExportDirectory))
        {
            MiunaHelperHost.Logger.LogWarning("[CardExport] 还没有导出过，或目录不存在");
            return;
        }

        Application.OpenURL(new Uri(LastExportDirectory).AbsoluteUri);
    }

    static int ExportList(List<OCIChar> list, string tag)
    {
        int ok = 0;
        var paths = new List<string>();
        string stamp = DateTime.Now.ToString("yyyyMMddHHmmss");

        for (int i = 0; i < list.Count; i++)
        {
            OCIChar oci = list[i];
            ChaControl cha = oci != null ? oci.charInfo : null;
            if (cha == null || cha.chaFile == null)
                continue;

            try
            {
                string path = BuildExportPath(cha, stamp, i);
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                bool saved = cha.chaFile.SaveCharaFile(path, cha.chaFile.parameter.sex, false);
                if (!saved || !File.Exists(path))
                {
                    MiunaHelperHost.Logger.LogError("[CardExport] 保存失败: " + path);
                    continue;
                }

                ok++;
                paths.Add(path);
                LastExportDirectory = dir;
                MiunaHelperHost.Logger.LogInfo("[CardExport] " + path);
            }
            catch (Exception ex)
            {
                MiunaHelperHost.Logger.LogError("[CardExport] " + ex);
            }
        }

        LastResult = ok == 0
            ? "导出失败 (" + tag + ")"
            : "已导出 " + ok + " 张卡 → " + LastExportDirectory;
        MiunaHelperHost.Logger.LogMessage("[CardExport] " + LastResult);
        return ok;
    }

    static string BuildExportPath(ChaControl cha, string stamp, int index)
    {
        ChaFileParameter p = cha.chaFile.parameter;
        byte sex = p != null ? p.sex : (byte)1;
        string sexFolder = sex == 0 ? "male" : "female";
        string display = SanitizeFileName(GetDisplayName(p));
        string file = "StudioExtract_" + display + "_" + stamp + "_" + index.ToString("00") + ".png";
        return Path.Combine(Path.Combine(Path.Combine(UserData.Path, "chara"), sexFolder), file);
    }

    static string GetDisplayName(ChaFileParameter p)
    {
        if (p == null)
            return "chara";

        if (!string.IsNullOrEmpty(p.fullname))
            return p.fullname.Trim();

        string combined = ((p.lastname ?? "") + " " + (p.firstname ?? "")).Trim();
        return combined.Length > 0 ? combined : "chara";
    }

    static string SanitizeFileName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "chara";

        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            bool bad = false;
            for (int j = 0; j < invalid.Length; j++)
            {
                if (c == invalid[j])
                {
                    bad = true;
                    break;
                }
            }

            sb.Append(bad ? '_' : c);
        }

        string s = sb.ToString().Trim();
        return s.Length > 0 ? s : "chara";
    }

    static IEnumerable<OCIChar> EnumerateSelectedCharacters()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            yield break;

        foreach (OCIChar oci in StudioAPI.GetSelectedCharacters())
        {
            if (oci != null && oci.charInfo != null)
                yield return oci;
        }
    }

    static IEnumerable<OCIChar> EnumerateSceneCharacters()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            yield break;

        var studio = Studio.Studio.Instance;
        if (studio == null || studio.dicInfo == null)
            yield break;

        foreach (ObjectCtrlInfo info in studio.dicInfo.Values)
        {
            var oci = info as OCIChar;
            if (oci != null && oci.charInfo != null)
                yield return oci;
        }
    }
}
