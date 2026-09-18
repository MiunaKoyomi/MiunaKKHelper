using System;
using System.Collections;
using System.IO;
using BepInEx;
using ChaCustom;
using HarmonyLib;
using KKAPI;
using UnityEngine;

namespace MiunaKKHelper.CardFavorites;

/// <summary>仅作用于 Maker 读取角色卡窗口；不会给每张卡增加操作按钮。</summary>
public sealed class MakerCardFavorites : MonoBehaviour
{
    internal static MakerCardFavorites Instance;
    internal CardPreferenceStore Store;
    internal MakerCardView Selected;
    string error;
    GUIStyle titleStyle, nameStyle, buttonStyle, hintStyle;
    Rect panel;
    readonly Vector3[] corners = new Vector3[4];
    CustomFileListCtrl charaList;
    CustomFileWindow charaWindow;
    RectTransform charaAnchor;
    CanvasGroup[] charaGroups;
    bool filter4, filter5, filterFavorite;
    bool wasLoadVisible;
    int filterShown, filterTotal;

    bool AnyFilter => CardPreferenceFilter.Active(filter4, filter5, filterFavorite);
    bool RatingMode => Selected != null && Selected.Visible && Selected.Source != null && Selected.Source.tgl.isOn;

    void Awake()
    {
        Instance = this;
        try
        {
            Store = new CardPreferenceStore(Path.Combine(Paths.ConfigPath, MiunaHelperHost.GUID + ".card-preferences.xml"), Paths.GameRootPath);
        }
        catch (Exception ex)
        {
            error = "收藏文件读取失败，已停止写入。请查看日志。";
            MiunaHelperHost.Logger.LogError("Card favorites load failed: " + ex);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Selected = null;
    }

    internal static void Attach(CustomFileListCtrl list)
    {
        if (Instance == null || list == null) return;
        var window = GetWindow(list);
        if (window == null) return;
        foreach (var card in list.GetComponentsInChildren<CustomFileInfoComponent>(true))
        {
            if (card.info == null || card.imgThumb == null || card.tgl == null) continue;
            var view = card.GetComponent<MakerCardView>();
            if (view == null) view = card.gameObject.AddComponent<MakerCardView>();
            view.Bind(card, window, list.transform as RectTransform);
        }
        if (IsCharaLoad(window))
        {
            Instance.BindList(list, window);
            Instance.ApplyFilter(list);
        }
    }

    internal static CustomFileWindow GetWindow(CustomFileListCtrl list)
    {
        if (list == null) return null;
        var field = AccessTools.Field(typeof(CustomFileListCtrl), "cfWindow");
        return field == null ? null : field.GetValue(list) as CustomFileWindow;
    }

    static bool IsCharaLoad(CustomFileWindow window) =>
        window != null && window.fwType == CustomFileWindow.FileWindowType.CharaLoad;

    void BindList(CustomFileListCtrl list, CustomFileWindow window)
    {
        charaList = list;
        charaWindow = window;
        charaAnchor = list.transform as RectTransform;
        charaGroups = list.GetComponentsInParent<CanvasGroup>(true);
    }

    void EnsureCharaList()
    {
        if (charaList != null && charaWindow != null) return;
        var host = FindObjectOfType<CustomCharaFile>();
        if (host == null) return;
        var listField = AccessTools.Field(typeof(CustomCharaFile), "listCtrl");
        var windowField = AccessTools.Field(typeof(CustomCharaFile), "fileWindow");
        var list = listField == null ? null : listField.GetValue(host) as CustomFileListCtrl;
        var window = windowField == null ? null : windowField.GetValue(host) as CustomFileWindow;
        if (window == null) window = GetWindow(list);
        if (list == null || window == null) return;
        BindList(list, window);
    }

    bool CharaLoadVisible
    {
        get
        {
            EnsureCharaList();
            if (charaList == null || charaWindow == null || !charaList.isActiveAndEnabled
                || !charaWindow.isActiveAndEnabled || !IsCharaLoad(charaWindow) || charaAnchor == null)
                return false;
            if (charaGroups != null)
                foreach (var group in charaGroups)
                    if (group != null && (group.alpha < .01f || !group.blocksRaycasts)) return false;
            return true;
        }
    }

    void Update()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
        {
            wasLoadVisible = false;
            return;
        }
        EnsureCharaList();
        bool loadTab = charaWindow != null && charaWindow.isActiveAndEnabled && IsCharaLoad(charaWindow);
        if (loadTab && CharaLoadVisible && (!wasLoadVisible || AnyFilter && Time.frameCount % 30 == 0))
            ApplyFilter(charaList);
        wasLoadVisible = loadTab;
    }

    void OnGUI()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Maker) return;
        bool rating = RatingMode;
        if (!rating && !CharaLoadVisible) return;
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 1080f, .75f, 1.6f);
        float width = Mathf.Min(254 * scale, Screen.width - 16);
        float height = 211 * scale;
        RectTransform anchor = rating ? Selected.SideAnchor : charaAnchor;
        Canvas canvas = null;
        if (rating && Selected.Source.imgThumb != null) canvas = Selected.Source.imgThumb.canvas;
        if (canvas == null && charaList != null) canvas = charaList.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        float left = 260 * scale;
        if (anchor != null)
        {
            anchor.GetWorldCorners(corners);
            left = RectTransformUtility.WorldToScreenPoint(camera, corners[0]).x;
        }
        width = Mathf.Min(width, Mathf.Max(160 * scale, left - 20));
        panel = new Rect(Mathf.Max(8, left - width - 10),
            Mathf.Clamp(Screen.height * .30f, 8, Mathf.Max(8, Screen.height - height - 8)), width, height);
        XuaImguiGuard.Run(() =>
        {
            Color saved = GUI.color;
            GUI.color = new Color(.055f, .065f, .09f, .98f);
            GUI.DrawTexture(panel, Texture2D.whiteTexture);
            GUI.color = saved;
            panel = GUI.Window(73364, panel, DrawPanel, GUIContent.none, GUIStyle.none);
            KKAPI.Utilities.IMGUIUtils.EatInputInRect(panel);
        });
    }

    void EnsureStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 17 };
        titleStyle.normal.textColor = Color.white;
        nameStyle = new GUIStyle(titleStyle) { fontSize = 13, clipping = TextClipping.Clip };
        hintStyle = new GUIStyle(nameStyle) { fontSize = 12, wordWrap = true };
        hintStyle.normal.textColor = new Color(.65f, .7f, .8f);
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, border = new RectOffset(0, 0, 0, 0) };
        buttonStyle.normal.background = Texture2D.whiteTexture;
        buttonStyle.hover.background = Texture2D.whiteTexture;
        buttonStyle.active.background = Texture2D.whiteTexture;
        buttonStyle.normal.textColor = new Color(.06f, .08f, .12f);
        buttonStyle.hover.textColor = buttonStyle.normal.textColor;
        buttonStyle.active.textColor = buttonStyle.normal.textColor;
    }

    void DrawPanel(int id)
    {
        float s = panel.width / 254f;
        titleStyle.fontSize = Mathf.RoundToInt(17*s);
        nameStyle.fontSize = Mathf.RoundToInt(13*s);
        hintStyle.fontSize = Mathf.RoundToInt(12*s);
        buttonStyle.fontSize = Mathf.RoundToInt(14*s);
        if (RatingMode) DrawRating(s);
        else DrawFilter(s);
    }

    void DrawRating(float s)
    {
        GUI.Label(new Rect(14*s, 10*s, 226*s, 25*s), XuaImguiGuard.Plain("卡片收藏"), titleStyle);
        GUI.Label(new Rect(14*s, 38*s, 226*s, 24*s), XuaImguiGuard.Plain(Selected.DisplayName), nameStyle);
        if (Store == null)
        {
            GUI.Label(new Rect(14*s, 72*s, 226*s, 100*s), XuaImguiGuard.Plain(error), hintStyle);
            return;
        }
        CardPreference value = Selected.Value;
        Color saved = GUI.backgroundColor;
        GUI.backgroundColor = value.Rating == 4 ? new Color(.3f, .7f, 1f) : new Color(.55f, .7f, .85f);
        if (GUI.Button(new Rect(14*s, 74*s, 109*s, 34*s), XuaImguiGuard.Plain(value.Rating == 4 ? "✓ 4 分 · 蓝" : "4 分 · 蓝"), buttonStyle))
            Save(4, value.Favorite);
        GUI.backgroundColor = value.Rating == 5 ? new Color(1f, .76f, .24f) : new Color(.9f, .78f, .53f);
        if (GUI.Button(new Rect(131*s, 74*s, 109*s, 34*s), XuaImguiGuard.Plain(value.Rating == 5 ? "✓ 5 分 · 金" : "5 分 · 金"), buttonStyle))
            Save(5, value.Favorite);
        GUI.backgroundColor = value.Favorite ? new Color(1f, .4f, .58f) : new Color(.75f, .8f, .88f);
        if (GUI.Button(new Rect(14*s, 116*s, 226*s, 32*s), XuaImguiGuard.Plain(value.Favorite ? "♥ 已喜欢 · 点击取消" : "♡ 喜欢这张卡"), buttonStyle))
            Save(value.Rating, !value.Favorite);
        GUI.backgroundColor = new Color(.75f, .8f, .88f);
        GUI.enabled = value.Rating != 0;
        if (GUI.Button(new Rect(14*s, 156*s, 92*s, 26*s), XuaImguiGuard.Plain("取消评分"), buttonStyle)) Save(0, value.Favorite);
        GUI.enabled = true;
        GUI.backgroundColor = saved;
        GUI.Label(new Rect(114*s, 157*s, 126*s, 26*s), XuaImguiGuard.Plain(value.Rating == 0 ? "未评分" : "评分与喜欢独立"), hintStyle);
        GUI.Label(new Rect(14*s, 186*s, 226*s, 23*s), XuaImguiGuard.Plain(error ?? "自动保存 · 不修改角色卡"), hintStyle);
    }

    void DrawFilter(float s)
    {
        GUI.Label(new Rect(14*s, 10*s, 226*s, 25*s), XuaImguiGuard.Plain("卡片筛选"), titleStyle);
        GUI.Label(new Rect(14*s, 38*s, 226*s, 24*s), XuaImguiGuard.Plain("未选中角色 · 多选"), nameStyle);
        if (Store == null)
        {
            GUI.Label(new Rect(14*s, 72*s, 226*s, 100*s), XuaImguiGuard.Plain(error), hintStyle);
            return;
        }
        Color saved = GUI.backgroundColor;
        GUI.backgroundColor = filter4 ? new Color(.3f, .7f, 1f) : new Color(.55f, .7f, .85f);
        if (GUI.Button(new Rect(14*s, 74*s, 109*s, 34*s), XuaImguiGuard.Plain(filter4 ? "✓ 4 分 · 蓝" : "4 分 · 蓝"), buttonStyle))
            SetFilter(ref filter4, !filter4);
        GUI.backgroundColor = filter5 ? new Color(1f, .76f, .24f) : new Color(.9f, .78f, .53f);
        if (GUI.Button(new Rect(131*s, 74*s, 109*s, 34*s), XuaImguiGuard.Plain(filter5 ? "✓ 5 分 · 金" : "5 分 · 金"), buttonStyle))
            SetFilter(ref filter5, !filter5);
        GUI.backgroundColor = filterFavorite ? new Color(1f, .4f, .58f) : new Color(.75f, .8f, .88f);
        if (GUI.Button(new Rect(14*s, 116*s, 226*s, 32*s), XuaImguiGuard.Plain(filterFavorite ? "♥ 喜欢 · 已开" : "♡ 喜欢"), buttonStyle))
            SetFilter(ref filterFavorite, !filterFavorite);
        GUI.backgroundColor = new Color(.75f, .8f, .88f);
        GUI.enabled = AnyFilter;
        if (GUI.Button(new Rect(14*s, 156*s, 92*s, 26*s), XuaImguiGuard.Plain("清除筛选"), buttonStyle))
        {
            filter4 = filter5 = filterFavorite = false;
            RefreshFilter();
        }
        GUI.enabled = true;
        GUI.backgroundColor = saved;
        string count = AnyFilter ? "显示 " + filterShown + " / " + filterTotal : "未筛选";
        GUI.Label(new Rect(114*s, 157*s, 126*s, 26*s), XuaImguiGuard.Plain(count), hintStyle);
        GUI.Label(new Rect(14*s, 186*s, 226*s, 23*s), XuaImguiGuard.Plain(AnyFilter ? "多选 · 符合任一项即显示" : "点选卡片可评分收藏"), hintStyle);
    }

    void SetFilter(ref bool flag, bool value)
    {
        flag = value;
        RefreshFilter();
    }

    void RefreshFilter()
    {
        EnsureCharaList();
        if (charaList == null) return;
        try { charaList.UpdateCategory(); }
        catch (Exception ex) { MiunaHelperHost.Logger.LogWarning("Card filter restore failed: " + ex.Message); }
        ApplyFilter(charaList);
    }

    internal void ApplyFilter(CustomFileListCtrl list)
    {
        if (list == null || !IsCharaLoad(GetWindow(list))) return;
        IList files = GetFileInfos(list);
        if (files == null) return;
        bool filtering = AnyFilter;
        filterShown = filterTotal = 0;
        for (int i = 0; i < files.Count; i++)
        {
            var info = files[i] as CustomFileInfo;
            if (info == null || info.fic == null) continue;
            filterTotal++;
            var fic = info.fic;
            bool show;
            if (!filtering)
            {
#if KKS
                // Sunshine restores visibility in GroupDataVisible; forcing !disvisible would undo 自作/他作.
                if (fic.gameObject.activeSelf) filterShown++;
                continue;
#else
                show = !info.disvisible;
#endif
            }
            else show = !info.disvisible && MatchesPath(info.FullPath);
            if (fic.gameObject.activeSelf != show)
            {
                if (!show && fic.tgl != null && fic.tgl.isOn) fic.tgl.isOn = false;
                fic.gameObject.SetActive(show);
            }
            if (fic.gameObject.activeSelf) filterShown++;
        }
    }

    static IList GetFileInfos(CustomFileListCtrl list)
    {
        var field = AccessTools.Field(list.GetType(), "lstFileInfo")
            ?? AccessTools.Field(typeof(CustomFileListCtrl), "lstFileInfo");
        if (field != null)
        {
            var value = field.GetValue(list) as IList;
            if (value != null) return value;
        }
        var prop = AccessTools.Property(list.GetType(), "lstFileInfo");
        return prop == null ? null : prop.GetValue(list, null) as IList;
    }

    bool MatchesPath(string path)
    {
        if (Store == null || string.IsNullOrEmpty(path)) return false;
        try { return CardPreferenceFilter.Matches(Store.Get(path), filter4, filter5, filterFavorite); }
        catch (Exception) { return false; }
    }

    void Save(int rating, bool favorite)
    {
        try
        {
            Store.Set(Selected.CardPath, rating, favorite);
            error = null;
            if (AnyFilter) ApplyFilter(charaList);
        }
        catch (Exception ex)
        {
            error = "保存失败，标记未更改；请查看日志。";
            MiunaHelperHost.Logger.LogError("Card favorites save failed: " + ex);
        }
    }
}

public sealed class MakerCardView : MonoBehaviour
{
    internal CustomFileInfoComponent Source;
    internal RectTransform SideAnchor;
    CustomFileWindow window;
    CanvasGroup[] groups;
    CardFrameGraphic frame;
    internal CardPreference Value;
    float nextIdentityCheck;
    int revision = -1;
    string path;
    internal string CardPath => path;
    internal string DisplayName => Source.info.name;
    internal bool Visible
    {
        get
        {
            if (Source == null || Source.info == null || !Source.isActiveAndEnabled || !Source.info.show
                || window == null || !window.isActiveAndEnabled || window.fwType != CustomFileWindow.FileWindowType.CharaLoad
                || string.IsNullOrEmpty(path)) return false;
            foreach (var group in groups)
                if (group != null && (group.alpha < .01f || !group.blocksRaycasts)) return false;
            return true;
        }
    }

    internal void Bind(CustomFileInfoComponent source, CustomFileWindow owner, RectTransform sideAnchor)
    {
        if (Source != null) Source.tgl.onValueChanged.RemoveListener(SelectionChanged);
        Source = source;
        SideAnchor = sideAnchor;
        window = owner;
        groups = source.GetComponentsInParent<CanvasGroup>(true);
        Source.tgl.onValueChanged.AddListener(SelectionChanged);
        path = null;
        revision = -1;
        if (source.tgl.isOn) SelectionChanged(true);
    }

    void SelectionChanged(bool selected)
    {
        var host = MakerCardFavorites.Instance;
        if (host == null) return;
        if (selected) host.Selected = this;
        else if (host.Selected == this) host.Selected = null;
    }

    void Update()
    {
        var host = MakerCardFavorites.Instance;
        if (Source == null || Source.info == null || host == null) return;
        string currentPath = Source.info.FullPath;
        if (currentPath != path) { path = currentPath; revision = -1; }
        if (host.Store != null && Visible && !Source.imgThumb.canvasRenderer.cull
            && (revision != host.Store.Revision || Time.unscaledTime >= nextIdentityCheck))
        {
            nextIdentityCheck = Time.unscaledTime + 2f;
            CardPreference value;
            try { value = host.Store.Get(path); }
            catch (Exception ex)
            {
                MiunaHelperHost.Logger.LogWarning("Invalid card path: " + ex.Message);
                if (frame != null) frame.enabled = false;
                return;
            }
            Value = value;
            revision = host.Store.Revision;
            if (frame == null && (value.Rating != 0 || value.Favorite))
            {
                var go = new GameObject("Miuna Card Frame", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(Source.imgThumb.transform, false);
                var rect = (RectTransform)go.transform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                frame = go.AddComponent<CardFrameGraphic>();
                frame.raycastTarget = false;
            }
            if (frame != null && (frame.Rating != value.Rating || frame.Favorite != value.Favorite))
            {
                frame.Rating = value.Rating;
                frame.Favorite = value.Favorite;
                frame.SetVerticesDirty();
            }
        }
        if (frame != null) frame.enabled = Visible && (frame.Rating != 0 || frame.Favorite);
    }

    void OnDestroy()
    {
        if (Source != null && Source.tgl != null) Source.tgl.onValueChanged.RemoveListener(SelectionChanged);
        var host = MakerCardFavorites.Instance;
        if (host != null && host.Selected == this) host.Selected = null;
        if (frame != null) Destroy(frame.gameObject);
    }
}

[HarmonyPatch(typeof(CustomFileListCtrl), "Create")]
static class MakerCardFavoritesHooks
{
    static void Postfix(CustomFileListCtrl __instance)
    {
        try { MakerCardFavorites.Attach(__instance); }
        catch (Exception ex) { MiunaHelperHost.Logger.LogError("Maker card favorites binding failed: " + ex); }
    }
}

[HarmonyPatch(typeof(CustomFileListCtrl), "UpdateCategory")]
static class MakerCardFilterCategoryHook
{
    static void Postfix(CustomFileListCtrl __instance)
    {
        var host = MakerCardFavorites.Instance;
        if (host == null) return;
        try { host.ApplyFilter(__instance); }
        catch (Exception ex) { MiunaHelperHost.Logger.LogWarning("Card filter overlay failed: " + ex.Message); }
    }
}

#if KKS
[HarmonyPatch(typeof(CustomFileListCtrl), "GroupDataVisible")]
static class MakerCardFilterGroupHook
{
    static void Postfix(CustomFileListCtrl __instance)
    {
        var host = MakerCardFavorites.Instance;
        if (host == null) return;
        try { host.ApplyFilter(__instance); }
        catch (Exception ex) { MiunaHelperHost.Logger.LogWarning("Card filter overlay failed: " + ex.Message); }
    }
}
#endif
