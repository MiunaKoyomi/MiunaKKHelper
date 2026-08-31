using System;
using ExtensibleSaveFormat;
using KKAPI;

namespace MiunaKKHelper.StudioTools;

/// <summary>
/// Studio 场景显示名（类似角色卡 author）。
/// 写入 ExtendedSave，ID 必须与导出器 <c>SceneNameExportHelper.ExtendedDataId</c> 相同。
/// 不引用导出器程序集；导出器也不引用本 DLL。
/// </summary>
public static class StudioSceneNameTools
{
    /// <summary>
    /// KK / KKS 共用同一数据 ID（KKS 插件 GUID 是 org.miuna.plugins.KKSHelper，但场景名数据不跟插件 GUID）。
    /// 不要改成 Ambient 用的插件 GUID，否则会和 <see cref="StudioAmbientLightSceneController"/> 抢同一份 ExtendedData。
    /// </summary>
    public const string ExtendedDataId = "org.miuna.plugins.KKHelper.scenename";
    public const string NameKey = "name";

    /// <summary>当前会话里的场景名（未 trim，方便 IMGUI 输入）。输入框改这个，点「写入」才 Persist。</summary>
    public static string CurrentName = "";

    public static string TrimmedName => (CurrentName ?? "").Trim();

    /// <summary>只改输入框，不写 ExtendedSave。</summary>
    public static void SetName(string name)
    {
        CurrentName = name ?? "";
    }

    /// <summary>清空输入框并立刻取消 ExtendedSave 绑定。</summary>
    public static void Clear()
    {
        CurrentName = "";
        PersistToExtendedSave();
    }

    /// <summary>把当前输入框写入内存中的场景 ExtendedSave；导出器不用先存 png 也能读到。</summary>
    public static bool PersistToExtendedSave()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
        {
            MiunaHelperHost.Logger?.LogWarning("[SceneName] 当前不在 Studio，未写入 ExtendedSave");
            return false;
        }

        try
        {
            string n = TrimmedName;
            if (string.IsNullOrEmpty(n))
            {
                ExtendedSave.SetSceneExtendedDataById(ExtendedDataId, null);
                return true;
            }

            var data = new PluginData { version = 1 };
            data.data[NameKey] = n;
            ExtendedSave.SetSceneExtendedDataById(ExtendedDataId, data);
            MiunaHelperHost.Logger?.LogInfo($"[SceneName] 已写入当前场景扩展数据: {n}");
            return true;
        }
        catch (Exception ex)
        {
            MiunaHelperHost.Logger?.LogWarning($"[SceneName] 写入 ExtendedSave 失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 写入扩展数据并调用 Studio 原版保存按钮流程。原版每次保存都会在 UserData/studio/scene
    /// 新建一个带时间戳的场景 PNG；只写 ExtendedSave 内存无法跨重启保留。
    /// </summary>
    public static bool PersistAndSaveScene()
    {
        if (!PersistToExtendedSave())
            return false;

        try
        {
            var studio = Studio.Studio.Instance;
            if (studio == null || studio.systemButtonCtrl == null)
            {
                MiunaHelperHost.Logger?.LogWarning("[SceneName] Studio 保存控制器不可用，未生成场景 PNG");
                return false;
            }

            studio.systemButtonCtrl.OnClickSave();
            return true;
        }
        catch (Exception ex)
        {
            MiunaHelperHost.Logger?.LogWarning($"[SceneName] 保存场景 PNG 失败: {ex.Message}");
            return false;
        }
    }

    public static string TryReadFromExtendedSave()
    {
        try
        {
            PluginData data = ExtendedSave.GetSceneExtendedDataById(ExtendedDataId);
            return ReadName(data);
        }
        catch (Exception ex)
        {
            MiunaHelperHost.Logger?.LogDebug($"[SceneName] 读取 ExtendedSave 失败: {ex.Message}");
            return "";
        }
    }

    public static string ReadName(PluginData data)
    {
        if (data?.data == null)
            return "";

        if (!data.data.TryGetValue(NameKey, out object obj) || obj == null)
            return "";

        string s = obj as string ?? obj.ToString();
        return (s ?? "").Trim();
    }

    public static PluginData BuildPluginData()
    {
        string n = TrimmedName;
        if (string.IsNullOrEmpty(n))
            return null;

        var data = new PluginData { version = 1 };
        data.data[NameKey] = n;
        return data;
    }

    /// <summary>
    /// 生成场景保存数据。若输入框暂时还没从读档回调恢复，保留 ExtendedSave 中已经存在的名字，
    /// 避免一次自动保存把有效绑定误当作「清空」删除；显式「清空」会先把 ExtendedSave 置空。
    /// </summary>
    public static PluginData BuildPluginDataForSceneSave()
    {
        PluginData draft = BuildPluginData();
        if (draft != null)
            return draft;

        try
        {
            PluginData existing = ExtendedSave.GetSceneExtendedDataById(ExtendedDataId);
            return string.IsNullOrEmpty(ReadName(existing)) ? null : existing;
        }
        catch (Exception ex)
        {
            MiunaHelperHost.Logger?.LogDebug($"[SceneName] 保存前读取 ExtendedSave 失败: {ex.Message}");
            return null;
        }
    }
}
