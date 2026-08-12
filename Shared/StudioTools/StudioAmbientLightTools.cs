using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiunaKKHelper.StudioTools;

/// <summary>
/// Studio 全局环境光（RenderSettings.ambient*）。
/// 原版 Studio 只有 ambientShadow（全体陰影），不能调 Unity Ambient。
/// </summary>
public static class StudioAmbientLightTools
{
    public const string ExtendedDataKey = "StudioAmbientLight";

    /// <summary>开启后持续覆盖 Map 可能写回的环境光。</summary>
    public static bool OverrideEnabled;

    public static AmbientMode Mode = AmbientMode.Flat;
    public static float Intensity = 1f;
    public static Color FlatColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    public static Color SkyColor = new Color(0.55f, 0.55f, 0.60f, 1f);
    public static Color EquatorColor = new Color(0.40f, 0.40f, 0.40f, 1f);
    public static Color GroundColor = new Color(0.25f, 0.25f, 0.28f, 1f);

    static bool _capturedBaseline;
    static AmbientMode _baselineMode;
    static float _baselineIntensity;
    static Color _baselineFlat;
    static Color _baselineSky;
    static Color _baselineEquator;
    static Color _baselineGround;

    public static void CaptureFromRenderSettings()
    {
        Mode = RenderSettings.ambientMode;
        Intensity = RenderSettings.ambientIntensity;
        FlatColor = RenderSettings.ambientLight;
        SkyColor = RenderSettings.ambientSkyColor;
        EquatorColor = RenderSettings.ambientEquatorColor;
        GroundColor = RenderSettings.ambientGroundColor;
    }

    public static void CaptureBaselineIfNeeded()
    {
        if (_capturedBaseline)
            return;

        _baselineMode = RenderSettings.ambientMode;
        _baselineIntensity = RenderSettings.ambientIntensity;
        _baselineFlat = RenderSettings.ambientLight;
        _baselineSky = RenderSettings.ambientSkyColor;
        _baselineEquator = RenderSettings.ambientEquatorColor;
        _baselineGround = RenderSettings.ambientGroundColor;
        _capturedBaseline = true;
    }

    public static void ResetBaselineFlag() => _capturedBaseline = false;

    public static void Apply()
    {
        if (!OverrideEnabled)
            return;

        RenderSettings.ambientMode = Mode;
        RenderSettings.ambientIntensity = Mathf.Clamp(Intensity, 0f, 8f);

        switch (Mode)
        {
            case AmbientMode.Flat:
                RenderSettings.ambientLight = FlatColor;
                break;
            case AmbientMode.Trilight:
                RenderSettings.ambientSkyColor = SkyColor;
                RenderSettings.ambientEquatorColor = EquatorColor;
                RenderSettings.ambientGroundColor = GroundColor;
                break;
            case AmbientMode.Skybox:
            case AmbientMode.Custom:
            default:
                // Skybox / Custom：主要靠 intensity；颜色仍写一份便于预览/导出
                RenderSettings.ambientSkyColor = SkyColor;
                RenderSettings.ambientLight = FlatColor;
                break;
        }
    }

    public static void RestoreBaseline()
    {
        OverrideEnabled = false;
        if (!_capturedBaseline)
            return;

        RenderSettings.ambientMode = _baselineMode;
        RenderSettings.ambientIntensity = _baselineIntensity;
        RenderSettings.ambientLight = _baselineFlat;
        RenderSettings.ambientSkyColor = _baselineSky;
        RenderSettings.ambientEquatorColor = _baselineEquator;
        RenderSettings.ambientGroundColor = _baselineGround;
    }

    public static string DescribeMode(AmbientMode mode)
    {
        switch (mode)
        {
            case AmbientMode.Skybox: return "Skybox";
            case AmbientMode.Trilight: return "Trilight";
            case AmbientMode.Flat: return "Flat";
            case AmbientMode.Custom: return "Custom";
            default: return mode.ToString();
        }
    }

    public static float[] ColorToArray(Color c) => new[] { c.r, c.g, c.b, c.a };

    public static Color ArrayToColor(object raw, Color fallback)
    {
        if (raw is float[] f && f.Length >= 3)
            return new Color(f[0], f[1], f[2], f.Length >= 4 ? f[3] : 1f);
        if (raw is double[] d && d.Length >= 3)
            return new Color((float)d[0], (float)d[1], (float)d[2], d.Length >= 4 ? (float)d[3] : 1f);
        return fallback;
    }

    public static object ReadBoxed(object value, Type prefer)
    {
        if (value == null)
            return null;
        if (prefer == typeof(bool))
        {
            if (value is bool b) return b;
            if (value is int i) return i != 0;
        }
        if (prefer == typeof(int))
        {
            if (value is int i) return i;
            if (value is long l) return (int)l;
            if (value is float f) return (int)f;
        }
        if (prefer == typeof(float))
        {
            if (value is float f) return f;
            if (value is double d) return (float)d;
            if (value is int i) return (float)i;
        }
        return value;
    }
}
