using System;
using System.Collections;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using KKAPI;
using MiunaKKHelper.StudioTools;
using static MiunaKKHelper.StudioTools.SharedTools;
using UnityEngine;
using UnityEngine.Rendering;

namespace MiunaKKHelper;

public class MiunaKKAPIUI : MonoBehaviour
{
    internal static Rect MainWindowRect = new(500, 40, 280, 420);
    public static bool UIActive;

    static ManualLogSource Logger => MiunaHelperHost.Logger;
    // --- 状态控制变量（默认都是开启状态 true） ---
    private bool _isDynamicBoneEnabled = true;
    private bool _isDynamicBoneVer02Enabled = true;
    private Vector2 _scroll;

    const string PaletteTitleFlat = "Miuna Ambient Flat";
    const string PaletteTitleSky = "Miuna Ambient Sky";
    const string PaletteTitleEquator = "Miuna Ambient Equator";
    const string PaletteTitleGround = "Miuna Ambient Ground";

    /// <summary>IMGUI 色块用：GUI.backgroundColor 在游戏皮肤下染不上色。</summary>
    static Texture2D _imguiWhitePixel;

    void Update()
    {
        if (MiunaHelperHost.Hotkey != null && MiunaHelperHost.Hotkey.Value.IsDown() && (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker 
                                                 || KoikatuAPI.GetCurrentGameMode() == GameMode.Studio))
        {
            UIActive = !UIActive;
            return;
        }
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio && KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            UIActive = false;
    }

    void LateUpdate()
    {
        // Map 切换可能改回环境光；覆盖开启时每帧写回
        if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio && StudioAmbientLightTools.OverrideEnabled)
            StudioAmbientLightTools.Apply();
    }

    void OnGUI()
    {
        if (!(KoikatuAPI.GetCurrentGameMode() == GameMode.Maker || KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)) return;
        if (UIActive)
        {
            MainWindowRect = GUILayout.Window(33361, MainWindowRect, WindowFunction, MiunaHelperHost.PluginName + " " + MiunaHelperHost.Version, GUILayout.Width(280));
            KKAPI.Utilities.IMGUIUtils.EatInputInRect(MainWindowRect);
        }
    }

    private void WindowFunction(int windowID)
    {
        GUILayoutOption[] elementOptions = { GUILayout.Width(250), GUILayout.Height(28) };

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.Height(380));
        GUILayout.BeginVertical();
        
        // 1. 标题区域
        GUILayout.Label("MiunaTest", GUILayout.Height(22));
        GUILayout.Space(4);
        
        // 2. Clear Log 按钮
        if (GUILayout.Button("Clear Log", elementOptions))
        {
            ClearConsole();
        }
        
        GUILayout.Space(4);

        // 获取当前选中的角色 Transform
        Transform currentSelection = GetSelectionTransform();
        // 如果没有选中任何东西，按钮应该置灰禁用，防止空指针报错
        GUI.enabled = (currentSelection != null);

        // 3. DynamicBone 切换按钮（根据当前状态动态改变文本）
        string dbButtonText = _isDynamicBoneEnabled ? "Disable DynamicBone" : "Enable DynamicBone";
        if (GUILayout.Button(dbButtonText, elementOptions))
        {
            // 状态取反
            _isDynamicBoneEnabled = !_isDynamicBoneEnabled;
            SetDynamicBoneEnabled(currentSelection, _isDynamicBoneEnabled);
        }
        
        GUILayout.Space(4);

        // 4. DynamicBoneVer02 切换按钮
        string dbv2ButtonText = _isDynamicBoneVer02Enabled ? "Disable DynamicBoneVer02" : "Enable DynamicBoneVer02";
        if (GUILayout.Button(dbv2ButtonText, elementOptions))
        {
            // 状态取反
            _isDynamicBoneVer02Enabled = !_isDynamicBoneVer02Enabled;
            SetDynamicBoneVer02Enabled(currentSelection, _isDynamicBoneVer02Enabled);
        }
        
        GUI.enabled = true; // 恢复 GUI 状态

        if (GUILayout.Button("ShowDynamicBone", elementOptions))
        {
            ShowDynamicBone(GetSelectionTransform());
        }

        GUILayout.Space(4);
        GUI.enabled = KoikatuAPI.GetCurrentGameMode() == GameMode.Studio;
        string exportLabel = AnimationListExporter.HasCache
            ? "Export Animation Catalog (" + AnimationListExporter.CachedCount + ")"
            : "Export Animation Catalog (no cache)";
        if (GUILayout.Button(exportLabel, elementOptions))
        {
            StartCoroutine(ExportAnimationCatalogCoroutine());
        }
        GUI.enabled = true;

        if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
        {
            DrawStudioCardExportSection(elementOptions);
            DrawStudioAmbientSection(elementOptions);
        }

        GUILayout.EndVertical();
        GUILayout.EndScrollView();

        // 顶部的拖拽区域
        GUI.DragWindow(new Rect(0, 0, MainWindowRect.width, 25));
    }

    void DrawStudioCardExportSection(GUILayoutOption[] elementOptions)
    {
        GUILayout.Space(8);
        GUILayout.Label("—— 提取角色卡 ——", GUILayout.Height(20));
        GUILayout.Label(
            "把 Studio 场景角色存成 png 卡，写入 UserData/chara/male 或 female。",
            GUILayout.Height(32));

        int selected = StudioCharacterCardExport.CountSelectedCharacters();
        GUI.enabled = selected > 0;
        if (GUILayout.Button("提取选中角色 (" + selected + ")", elementOptions))
            StudioCharacterCardExport.ExportSelected();
        GUI.enabled = true;

        int sceneCount = StudioCharacterCardExport.CountSceneCharacters();
        GUI.enabled = sceneCount > 0;
        if (GUILayout.Button("提取场景全部角色 (" + sceneCount + ")", elementOptions))
            StudioCharacterCardExport.ExportAllInScene();
        GUI.enabled = true;

        if (!string.IsNullOrEmpty(StudioCharacterCardExport.LastResult))
            GUILayout.Label(StudioCharacterCardExport.LastResult, GUILayout.Height(36));

        GUI.enabled = !string.IsNullOrEmpty(StudioCharacterCardExport.LastExportDirectory);
        if (GUILayout.Button("打开导出目录", elementOptions))
            StudioCharacterCardExport.OpenLastExportFolder();
        GUI.enabled = true;
    }

    void DrawStudioAmbientSection(GUILayoutOption[] elementOptions)
    {
        GUILayout.Space(8);
        GUILayout.Label("—— Studio 环境光 ——", GUILayout.Height(20));
        GUILayout.Label(
            "原版只有全体陰影，不能调 RenderSettings Ambient。\n开启覆盖后可调色/强度，并写入场景存档。",
            GUILayout.Height(36));

        bool enabled = GUILayout.Toggle(StudioAmbientLightTools.OverrideEnabled, "覆盖环境光 (Ambient)", elementOptions);
        if (enabled != StudioAmbientLightTools.OverrideEnabled)
        {
            if (enabled)
            {
                StudioAmbientLightTools.CaptureBaselineIfNeeded();
                StudioAmbientLightTools.CaptureFromRenderSettings();
                StudioAmbientLightTools.OverrideEnabled = true;
                StudioAmbientLightTools.Apply();
            }
            else
            {
                StudioAmbientLightTools.RestoreBaseline();
            }
        }

        GUI.enabled = StudioAmbientLightTools.OverrideEnabled;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Flat", GUILayout.Height(24)))
        {
            StudioAmbientLightTools.Mode = AmbientMode.Flat;
            StudioAmbientLightTools.Apply();
        }
        if (GUILayout.Button("Trilight", GUILayout.Height(24)))
        {
            StudioAmbientLightTools.Mode = AmbientMode.Trilight;
            StudioAmbientLightTools.Apply();
        }
        if (GUILayout.Button("Skybox", GUILayout.Height(24)))
        {
            StudioAmbientLightTools.Mode = AmbientMode.Skybox;
            StudioAmbientLightTools.Apply();
        }
        GUILayout.EndHorizontal();

        GUILayout.Label(
            "Mode: " + StudioAmbientLightTools.DescribeMode(StudioAmbientLightTools.Mode)
            + " | live: " + StudioAmbientLightTools.DescribeMode(RenderSettings.ambientMode));

        float intensity = GUILayout.HorizontalSlider(StudioAmbientLightTools.Intensity, 0f, 4f, elementOptions);
        if (!Mathf.Approximately(intensity, StudioAmbientLightTools.Intensity))
        {
            StudioAmbientLightTools.Intensity = intensity;
            StudioAmbientLightTools.Apply();
        }
        GUILayout.Label("Intensity: " + StudioAmbientLightTools.Intensity.ToString("0.00"));

        // 与 MaterialEditor / 原版雾效相同：点色块 → Studio 调色盘
        if (StudioAmbientLightTools.Mode == AmbientMode.Flat
            || StudioAmbientLightTools.Mode == AmbientMode.Skybox
            || StudioAmbientLightTools.Mode == AmbientMode.Custom)
        {
            DrawStudioColorProp(
                "Ambient Flat",
                StudioAmbientLightTools.FlatColor,
                c =>
                {
                    StudioAmbientLightTools.FlatColor = c;
                    StudioAmbientLightTools.Apply();
                });
        }

        if (StudioAmbientLightTools.Mode == AmbientMode.Trilight
            || StudioAmbientLightTools.Mode == AmbientMode.Skybox)
        {
            DrawStudioColorProp(
                "Ambient Sky",
                StudioAmbientLightTools.SkyColor,
                c =>
                {
                    StudioAmbientLightTools.SkyColor = c;
                    StudioAmbientLightTools.Apply();
                });
            if (StudioAmbientLightTools.Mode == AmbientMode.Trilight)
            {
                DrawStudioColorProp(
                    "Ambient Equator",
                    StudioAmbientLightTools.EquatorColor,
                    c =>
                    {
                        StudioAmbientLightTools.EquatorColor = c;
                        StudioAmbientLightTools.Apply();
                    });
                DrawStudioColorProp(
                    "Ambient Ground",
                    StudioAmbientLightTools.GroundColor,
                    c =>
                    {
                        StudioAmbientLightTools.GroundColor = c;
                        StudioAmbientLightTools.Apply();
                    });
            }
        }

        if (GUILayout.Button("从当前场景读取 Ambient", elementOptions))
        {
            StudioAmbientLightTools.CaptureFromRenderSettings();
            StudioAmbientLightTools.Apply();
            RefreshOpenStudioColorPalette();
        }

        if (GUILayout.Button("恢复开启前的 Ambient", elementOptions))
        {
            StudioAmbientLightTools.RestoreBaseline();
            CloseStudioColorPalette();
        }

        GUI.enabled = true;
    }

    void DrawStudioColorProp(string label, Color color, Action<Color> onChanged)
    {
        string title = LabelToPaletteTitle(label);
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100), GUILayout.Height(28));

        bool clicked = DrawColorSwatchButton(color, 56, 28);
        if (GUILayout.Button("调色盘", GUILayout.Width(60), GUILayout.Height(28)))
            clicked = true;

        GUILayout.EndHorizontal();

        if (clicked)
            ToggleStudioColorPalette(title, color, onChanged);
    }

    /// <summary>画可点击色块；用白贴图 + GUI.color，不用 backgroundColor。</summary>
    static bool DrawColorSwatchButton(Color color, float width, float height)
    {
        Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.Width(width), GUILayout.Height(height));
        Texture2D pixel = GetImguiWhitePixel();

        // 边框
        Color prev = GUI.color;
        GUI.color = Color.black;
        GUI.DrawTexture(rect, pixel);

        // 内填充
        var fill = new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f);
        GUI.color = new Color(Mathf.Clamp01(color.r), Mathf.Clamp01(color.g), Mathf.Clamp01(color.b), 1f);
        GUI.DrawTexture(fill, pixel);
        GUI.color = prev;

        // 透明热区按钮
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    static Texture2D GetImguiWhitePixel()
    {
        if (_imguiWhitePixel != null)
            return _imguiWhitePixel;

        _imguiWhitePixel = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        _imguiWhitePixel.SetPixel(0, 0, Color.white);
        _imguiWhitePixel.Apply(false, true);
        _imguiWhitePixel.hideFlags = HideFlags.HideAndDontSave;
        return _imguiWhitePixel;
    }

    static string LabelToPaletteTitle(string label)
    {
        if (label.IndexOf("Flat", StringComparison.OrdinalIgnoreCase) >= 0) return PaletteTitleFlat;
        if (label.IndexOf("Equator", StringComparison.OrdinalIgnoreCase) >= 0) return PaletteTitleEquator;
        if (label.IndexOf("Ground", StringComparison.OrdinalIgnoreCase) >= 0) return PaletteTitleGround;
        if (label.IndexOf("Sky", StringComparison.OrdinalIgnoreCase) >= 0) return PaletteTitleSky;
        return "Miuna Ambient " + label;
    }

    static void ToggleStudioColorPalette(string title, Color color, Action<Color> onChanged)
    {
        var studio = Studio.Studio.Instance;
        if (studio == null || studio.colorPalette == null)
        {
            MiunaHelperHost.Logger.LogWarning("[Ambient] Studio.colorPalette 不可用");
            return;
        }

        // 与原版雾/陰影、MaterialEditor 相同：同一标题再点一次则关闭
        if (studio.colorPalette.Check(title))
        {
            studio.colorPalette.visible = false;
            return;
        }

        studio.colorPalette.Setup(title, color, onChanged, false);
    }

    static void CloseStudioColorPalette()
    {
        var studio = Studio.Studio.Instance;
        if (studio?.colorPalette == null)
            return;
        studio.colorPalette.visible = false;
    }

    /// <summary>若调色盘正开着对应标题，用当前色刷新（读场景后同步预览）。</summary>
    static void RefreshOpenStudioColorPalette()
    {
        var studio = Studio.Studio.Instance;
        if (studio?.colorPalette == null || !studio.colorPalette.visible)
            return;

        if (studio.colorPalette.Check(PaletteTitleFlat))
            studio.colorPalette.Setup(PaletteTitleFlat, StudioAmbientLightTools.FlatColor,
                c => { StudioAmbientLightTools.FlatColor = c; StudioAmbientLightTools.Apply(); }, false);
        else if (studio.colorPalette.Check(PaletteTitleSky))
            studio.colorPalette.Setup(PaletteTitleSky, StudioAmbientLightTools.SkyColor,
                c => { StudioAmbientLightTools.SkyColor = c; StudioAmbientLightTools.Apply(); }, false);
        else if (studio.colorPalette.Check(PaletteTitleEquator))
            studio.colorPalette.Setup(PaletteTitleEquator, StudioAmbientLightTools.EquatorColor,
                c => { StudioAmbientLightTools.EquatorColor = c; StudioAmbientLightTools.Apply(); }, false);
        else if (studio.colorPalette.Check(PaletteTitleGround))
            studio.colorPalette.Setup(PaletteTitleGround, StudioAmbientLightTools.GroundColor,
                c => { StudioAmbientLightTools.GroundColor = c; StudioAmbientLightTools.Apply(); }, false);
    }

    IEnumerator ExportAnimationCatalogCoroutine()
    {
        yield return AnimationListExporter.TryCaptureCurrentCoroutine(this);
        string path = AnimationListExporter.ExportToUserData();
        if (!string.IsNullOrEmpty(path))
            Logger.LogInfo("[AnimExport] 文件: " + path);
    }

    private static void ShowDynamicBone(Transform root)
    {
        if (root == null) return;

        // 直接获取并遍历，节省内存开销
        var components = root.GetComponentsInChildren<DynamicBone>(true);
        foreach (DynamicBone bone in components)
        {
            if (bone != null)
            {
                Logger.LogDebug($"dynamicBone : {bone}");
            }
        }
        var components2 = root.GetComponentsInChildren<DynamicBone_Ver02>(true);
        foreach (DynamicBone_Ver02 bone in components2)
        {
            if (bone != null)
            {
                Logger.LogDebug($"dynamicBoneVer02 : {bone}");
            }
        }
    }
    private static void SetDynamicBoneEnabled(Transform root, bool enabled)
    {
        if (root == null) return;

        // 直接获取并遍历，节省内存开销
        var components = root.GetComponentsInChildren<DynamicBone>(true);
        foreach (var bone in components)
        {
            if (bone != null) bone.enabled = enabled;
        }
        
        MiunaHelperHost.Logger.LogWarning($"已将选中目标的 {components.Length} 个 DynamicBone 状态设为: {enabled}");
    }

    private static void SetDynamicBoneVer02Enabled(Transform root, bool enabled)
    {
        if (root == null) return;

        var components = root.GetComponentsInChildren<DynamicBone_Ver02>(true);
        foreach (var boneV2 in components)
        {
            if (boneV2 != null) boneV2.enabled = enabled;
        }

        MiunaHelperHost.Logger.LogWarning($"已将选中目标的 {components.Length} 个 DynamicBone_Ver02 状态设为: {enabled}");
    }

    
    // 在你的类内部加上这段 Win32 API 引入
    #region Win32 API

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FillConsoleOutputCharacter(IntPtr hConsoleOutput, char cCharacter, uint nLength, COORD dwWriteCoord, out uint lpNumberOfCharsWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool FillConsoleOutputAttribute(IntPtr hConsoleOutput, ushort wAttribute, uint nLength, COORD dwWriteCoord, out uint lpNumberOfAttrsWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleCursorPosition(IntPtr hConsoleOutput, COORD dwCursorPosition);

    private const int STD_OUTPUT_HANDLE = -11;

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD
    {
        public short X;
        public short Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SMALL_RECT
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public ushort wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
    }

    #endregion

    // 替换你原本的 ClearConsole 方法
    public static void ClearConsole()
    {
        try
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT) return;

            IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
            if (hConsole == IntPtr.Zero || hConsole == new IntPtr(-1)) return;

            // 获取控制台屏幕缓冲区的当前状态（大小、光标位置等）
            if (GetConsoleScreenBufferInfo(hConsole, out CONSOLE_SCREEN_BUFFER_INFO csbi))
            {
                uint cellsCount = (uint)(csbi.dwSize.X * csbi.dwSize.Y);
                COORD topLeft = new COORD { X = 0, Y = 0 };

                // 1. 用空格填满整个缓冲区（擦除所有文字）
                FillConsoleOutputCharacter(hConsole, ' ', cellsCount, topLeft, out _);

                // 2. 恢复缓冲区的默认颜色属性，防止背景色乱掉
                FillConsoleOutputAttribute(hConsole, csbi.wAttributes, cellsCount, topLeft, out _);

                // 3. 将光标重置到左上角 (0, 0)
                SetConsoleCursorPosition(hConsole, topLeft);

                MiunaHelperHost.Logger.LogInfo("[ClearConsole] 已强行清空 BepInEx 缓冲区。");
            }
        }
        catch (Exception ex)
        {
            MiunaHelperHost.Logger.LogError($"[ClearConsole] Win32 清理失败: {ex.Message}");
        }
    }
}