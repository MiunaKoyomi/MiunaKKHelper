using System.Collections.Generic;
using ExtensibleSaveFormat;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using Studio;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiunaKKHelper.StudioTools;

/// <summary>把 Studio 环境光覆盖写入场景扩展数据，读档后恢复。</summary>
public class StudioAmbientLightSceneController : SceneCustomFunctionController
{
    protected override void OnSceneSave()
    {
        if (!StudioAmbientLightTools.OverrideEnabled)
        {
            SetExtendedData(null);
            return;
        }

        var data = new PluginData { version = 1 };
        data.data["enabled"] = true;
        data.data["mode"] = (int)StudioAmbientLightTools.Mode;
        data.data["intensity"] = StudioAmbientLightTools.Intensity;
        data.data["flat"] = StudioAmbientLightTools.ColorToArray(StudioAmbientLightTools.FlatColor);
        data.data["sky"] = StudioAmbientLightTools.ColorToArray(StudioAmbientLightTools.SkyColor);
        data.data["equator"] = StudioAmbientLightTools.ColorToArray(StudioAmbientLightTools.EquatorColor);
        data.data["ground"] = StudioAmbientLightTools.ColorToArray(StudioAmbientLightTools.GroundColor);
        SetExtendedData(data);
        MiunaHelperHost.Logger.LogInfo(
            $"[Ambient] 场景已保存环境光覆盖 mode={StudioAmbientLightTools.DescribeMode(StudioAmbientLightTools.Mode)} " +
            $"intensity={StudioAmbientLightTools.Intensity:0.###}");
    }

    protected override void OnSceneLoad(
        SceneOperationKind operation,
        ReadOnlyDictionary<int, ObjectCtrlInfo> loadedItems)
    {
        if (operation == SceneOperationKind.Clear)
        {
            StudioAmbientLightTools.RestoreBaseline();
            StudioAmbientLightTools.ResetBaselineFlag();
            return;
        }

        PluginData data = GetExtendedData();
        if (data?.data == null || data.data.Count == 0)
        {
            // 新场景无扩展数据：不强制关覆盖（用户可能正在调），仅打日志
            MiunaHelperHost.Logger.LogDebug($"[Ambient] OnSceneLoad({operation}) 无环境光扩展数据");
            return;
        }

        StudioAmbientLightTools.CaptureBaselineIfNeeded();

        if (data.data.TryGetValue("enabled", out object enabledObj))
        {
            object boxed = StudioAmbientLightTools.ReadBoxed(enabledObj, typeof(bool));
            StudioAmbientLightTools.OverrideEnabled = boxed is bool b && b;
        }

        if (data.data.TryGetValue("mode", out object modeObj))
        {
            object boxed = StudioAmbientLightTools.ReadBoxed(modeObj, typeof(int));
            if (boxed is int modeInt)
                StudioAmbientLightTools.Mode = (AmbientMode)modeInt;
        }

        if (data.data.TryGetValue("intensity", out object intensityObj))
        {
            object boxed = StudioAmbientLightTools.ReadBoxed(intensityObj, typeof(float));
            if (boxed is float intensity)
                StudioAmbientLightTools.Intensity = intensity;
        }

        if (data.data.TryGetValue("flat", out object flat))
            StudioAmbientLightTools.FlatColor = StudioAmbientLightTools.ArrayToColor(flat, StudioAmbientLightTools.FlatColor);
        if (data.data.TryGetValue("sky", out object sky))
            StudioAmbientLightTools.SkyColor = StudioAmbientLightTools.ArrayToColor(sky, StudioAmbientLightTools.SkyColor);
        if (data.data.TryGetValue("equator", out object equator))
            StudioAmbientLightTools.EquatorColor = StudioAmbientLightTools.ArrayToColor(equator, StudioAmbientLightTools.EquatorColor);
        if (data.data.TryGetValue("ground", out object ground))
            StudioAmbientLightTools.GroundColor = StudioAmbientLightTools.ArrayToColor(ground, StudioAmbientLightTools.GroundColor);

        StudioAmbientLightTools.Apply();
        MiunaHelperHost.Logger.LogInfo(
            $"[Ambient] OnSceneLoad({operation}) 已恢复环境光覆盖 " +
            $"enabled={StudioAmbientLightTools.OverrideEnabled} " +
            $"mode={StudioAmbientLightTools.DescribeMode(StudioAmbientLightTools.Mode)} " +
            $"intensity={StudioAmbientLightTools.Intensity:0.###}");
    }
}
