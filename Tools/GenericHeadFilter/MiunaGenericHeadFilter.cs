using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using ChaCustom;
using Sideloader.AutoResolver;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[assembly: AssemblyVersion("1.0.1.0")]

namespace Miuna.GenericHeads
{
    [BepInPlugin(Id, "Miuna Generic Head Filter", "1.0.1")]
    [BepInDependency("KK_MakerSearch")]
    [BepInDependency("com.bepis.bepinex.sideloader")]
    [BepInProcess("Koikatu")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string Id = "org.miuna.plugins.GenericHeadFilter";
        static bool installed;
        static int depth;
        internal static readonly List<HeadFilter> Views = new List<HeadFilter>();
        internal static readonly FieldInfo ItemsField = AccessTools.Field(typeof(CustomSelectListCtrl), "lstSelectInfo");
        public void Awake() { Install(); }

        // Also used to enable the same compiled implementation in an already-running Maker.
        public static void Install()
        {
            if (installed) return;
            var harmony = new Harmony(Id);
            foreach (var method in typeof(CustomSelectListCtrl).GetMethods().Where(m =>
                m.Name == "DisvisibleItem" || m.Name == "UpdateCategory" || m.Name == "ClearList" || m.Name == "Create"))
                Wrap(harmony, method);
            foreach (var method in new MethodBase[] {
                AccessTools.Method(typeof(KK_MakerSearch.KK_MakerSearch), "MakerSearch_Search"),
                AccessTools.Method(typeof(KK_MakerSearch.Tools), "MakerSearch_ResetDisables"),
                AccessTools.Method(typeof(KK_MakerSearch.Tools), "RememberDisvisibles"),
                AccessTools.Method(typeof(KK_MakerSearch.Tools), "ResetSearch") })
                Wrap(harmony, method);
            harmony.Patch(AccessTools.Method(typeof(CustomSelectListCtrl), "Create"),
                postfix: new HarmonyMethod(typeof(Plugin), "Created"));
            installed = true;
            foreach (var list in Resources.FindObjectsOfTypeAll<CustomSelectListCtrl>()) Created(list);
        }

        static void Wrap(Harmony h, MethodBase m)
        {
            if (m == null) throw new MissingMethodException("Generic head filter: required list/search API missing");
            h.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), "Before") { priority = Priority.First },
                finalizer: new HarmonyMethod(typeof(Plugin), "After") { priority = Priority.Last });
        }
        static void Before(MethodBase __originalMethod, out HashSet<CustomSelectInfo> __state)
        {
            // MakerSearch uses a stack-frame heuristic to exclude its own temporary hides.
            // Harmony wrappers change that stack; preserve its memory explicitly for those calls.
            __state = (__originalMethod.Name == "MakerSearch_Search" || __originalMethod.Name == "MakerSearch_ResetDisables")
                ? new HashSet<CustomSelectInfo>(KK_MakerSearch.Tools.disvisibleMemory) : null;
            if (depth++ != 0) return;
            foreach (var view in Views.ToArray()) if (view) view.Restore();
        }
        static Exception After(Exception __exception, HashSet<CustomSelectInfo> __state)
        {
            if (__state != null)
            {
                KK_MakerSearch.Tools.disvisibleMemory.Clear();
                KK_MakerSearch.Tools.disvisibleMemory.UnionWith(__state);
            }
            if (--depth == 0)
                foreach (var view in Views.ToArray()) if (view) view.Apply();
            return __exception;
        }
        static void Created(CustomSelectListCtrl __instance)
        {
            var items = Items(__instance);
            if (items == null || !items.Any(i => i.category == 100)) return;
            if (!__instance.GetComponent<HeadFilter>()) __instance.gameObject.AddComponent<HeadFilter>();
        }
        internal static List<CustomSelectInfo> Items(CustomSelectListCtrl list)
        { return ItemsField.GetValue(list) as List<CustomSelectInfo>; }
    }

    public sealed class HeadFilter : MonoBehaviour
    {
        // Exact Sideloader GUID + original slot, never transient resolved IDs or translated names.
        static readonly HashSet<string> Known = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "Augh.FaceOutline|1", "com.chieri2uma.canine_head_no_ears|1",
            "com.chieri2uma.canine_teeth|1", "com.chieri2uma.canine_teeth_i_head|1",
            "com.chieri2uma.canine_teeth_i_headmod|1", "com.chieri2uma.canine_teeth_i_turime|1",
            "com.DeathWeasel.HeadNoEars|1", "com.DeathWeasel.HeadWithFangs|1",
            "com.DeathWeasel.SplitEyebrowsHead|1", "DrHarkness.BaldHead|6416",
            "Evaan.FleshFang.Mod|1", "Evaan.Nose.Mod|1", "FrankiePasta.ExpressivePlus01|1",
            "HeadMod|1", "HeadMod_SharpTeeth|15", "HeadMod2|1", "Modified head|1",
            "Modified headmod|1", "MrDelegado.BaseHeadmod|1", "MrDelegado.Headmod01|1",
            "MrDelegado.Headmod02|1", "MrDelegado.Headmod03|1", "MrDelegado.Headmod04|1",
            "MrDelegado.Headmod05|1", "MrDelegado.Headmod07|1", "MrDelegado.Headmod08|1",
            "MrDelegado.Headmod09|1", "nakay.head.gizaba|61", "turime_face|1"
        };
        readonly Dictionary<CustomSelectInfo, bool> baseline = new Dictionary<CustomSelectInfo, bool>();
        readonly Dictionary<CustomSelectInfo, bool> classification = new Dictionary<CustomSelectInfo, bool>();
        CustomSelectListCtrl list;
        TextMeshProUGUI label;
        Image background;
        GameObject button;
        RectTransform back;
        Vector2 oldBackMin;
        public bool OnlyGeneric { get; private set; }
        public int GenericCount { get; private set; }
        public int TotalCount { get; private set; }

        void Awake() { list = GetComponent<CustomSelectListCtrl>(); Plugin.Views.Add(this); }
        IEnumerator Start()
        {
            // MakerSearch creates its controls after the native lists are populated.
            for (int i = 0; i < 120 && !transform.Find("Reset"); ++i) yield return null;
            var template = transform.Find("Reset");
            if (!template) { Debug.LogError("[GenericHeadFilter] MakerSearch Reset button missing"); yield break; }
            button = Instantiate(template.gameObject);
            button.name = "MiunaOnlyGenericHeads";
            button.transform.SetParent(transform, false);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.anchoredPosition = new Vector2(-8, -32);
            rect.sizeDelta = new Vector2(382, 28);
            label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            label.fontSize = 18;
            label.enableAutoSizing = true;
            label.fontSizeMin = 12;
            label.fontSizeMax = 18;
            background = button.GetComponent<Image>();
            var click = button.GetComponent<Button>();
            click.onClick = new Button.ButtonClickedEvent();
            click.onClick.AddListener(() => SetOnlyGeneric(!OnlyGeneric));
            back = transform.Find("BasePanel/imgWindowBack") as RectTransform;
            if (back) { oldBackMin = back.offsetMin; back.offsetMin = new Vector2(oldBackMin.x, oldBackMin.y - 34); }
            SetOnlyGeneric(false);
        }
        public void SetOnlyGeneric(bool value)
        {
            Restore(); OnlyGeneric = value; Apply();
            var scroll = GetComponentInChildren<ScrollRect>(true);
            if (scroll) scroll.verticalNormalizedPosition = 1;
        }
        public bool IsGeneric(CustomSelectInfo item)
        {
            if (item.category != 100) return false;
            bool result;
            if (classification.TryGetValue(item, out result)) return result;
            var info = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)100, item.index);
            result = info == null ? item.index == 0 || item.index == 200 || item.index == 201
                : Known.Contains(info.GUID + "|" + info.Slot);
            classification[item] = result;
            return result;
        }
        internal void Restore()
        {
            if (!list || baseline.Count == 0) return;
            foreach (var entry in baseline) SetHidden(entry.Key, entry.Value);
            baseline.Clear();
            list.UpdateStateNew(); // also invalidates the installed virtual list's layout cache
        }
        internal void Apply()
        {
            if (!list || baseline.Count != 0) return;
            var items = Plugin.Items(list);
            if (items == null) return;
            TotalCount = items.Count;
            GenericCount = 0;
            foreach (var item in items)
            {
                bool generic = IsGeneric(item);
                if (generic) ++GenericCount;
                if (!OnlyGeneric || generic) continue;
                baseline[item] = item.disvisible;
                SetHidden(item, true);
            }
            if (OnlyGeneric) list.UpdateStateNew();
            if (label) label.text = (OnlyGeneric ? "[开] " : "[关] ") + "仅显示通用头模  " + GenericCount + " / " + TotalCount;
            if (background) background.color = OnlyGeneric ? new Color(0.5f, 0.9f, 1f, 1f) : Color.white;
        }
        static void SetHidden(CustomSelectInfo item, bool hidden)
        {
            item.disvisible = hidden;
            // Virtualized entries are recycled; never hide another item's current thumbnail.
            if (item.sic && item.sic.info == item) item.sic.Disvisible(hidden);
        }
        void OnDestroy()
        {
            Restore(); Plugin.Views.Remove(this);
            if (back) back.offsetMin = oldBackMin;
            if (button) Destroy(button);
        }
    }
}
