using ExtensibleSaveFormat;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using Studio;

namespace MiunaKKHelper.StudioTools;

/// <summary>
/// 场景 png 存档读写场景名。注册 ID 必须是 <see cref="StudioSceneNameTools.ExtendedDataId"/>，
/// 不要用插件 GUID（Ambient 已经占用 GUID 那一份 ExtendedData）。
/// </summary>
public class StudioSceneNameSceneController : SceneCustomFunctionController
{
    protected override void OnSceneSave()
    {
        PluginData data = StudioSceneNameTools.BuildPluginDataForSceneSave();
        SetExtendedData(data);
        string name = StudioSceneNameTools.ReadName(data);
        if (!string.IsNullOrEmpty(name))
            MiunaHelperHost.Logger.LogInfo($"[SceneName] 场景已保存绑定名: {name}");
    }

    protected override void OnSceneLoad(
        SceneOperationKind operation,
        ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
    {
        switch (operation)
        {
            case SceneOperationKind.Clear:
                // ExtendedSave 不会替我们清理新场景的自定义 ID，必须显式取消旧场景绑定。
                StudioSceneNameTools.Clear();
                return;

            case SceneOperationKind.Import:
                // ExtendedSave 导入时会把全局场景字典替换成被导入卡的数据；立即写回当前场景名，
                // 才能真正做到「导入物体不覆盖当前场景名」（对齐 TwoLut 的状态语义）。
                StudioSceneNameTools.PersistToExtendedSave();
                return;

            case SceneOperationKind.Load:
                StudioSceneNameTools.CurrentName = StudioSceneNameTools.ReadName(GetExtendedData());
                if (!string.IsNullOrEmpty(StudioSceneNameTools.TrimmedName))
                {
                    MiunaHelperHost.Logger.LogInfo(
                        $"[SceneName] OnSceneLoad 已恢复绑定名: {StudioSceneNameTools.TrimmedName}");
                }
                return;

            default:
                return;
        }
    }
}
