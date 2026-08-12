using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using KKAPI;
using KKAPI.Studio.SaveLoad;
using MiunaKKHelper.StudioTools;
using UnityEngine;

namespace MiunaKKHelper;

/// <summary>Miuna 个人 Koikatsu Sunshine / CharaStudio 插件功能集合。热键默认 LeftAlt+M。</summary>
[BepInPlugin(GUID, PluginName, Version)]
[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
public class MiunaKKSAPI : BaseUnityPlugin
{
    public const string PluginName = "MiunaKKSHelper";
    public const string GUID = "org.miuna.plugins.KKSHelper";
    public const string Version = "1.1.0";

    internal new static ManualLogSource Logger;

    public static ConfigEntry<KeyboardShortcut> hotkey;

    public static MiunaKKAPIUI UI;

    public void Awake()
    {
        Logger = base.Logger;
        KeyboardShortcut defaultShortcut = new KeyboardShortcut(KeyCode.M, KeyCode.LeftAlt);
        hotkey = Config.Bind("Miuna_KKSHelper", "Hotkey", defaultShortcut, "Press this key to open the UI");
        MiunaHelperHost.Initialize(Logger, PluginName, Version, GUID, hotkey);
        new Harmony(GUID).PatchAll(typeof(MiunaKKSAPI).Assembly);
        UI = this.GetOrAddComponent<MiunaKKAPIUI>();
        StudioSaveLoadApi.RegisterExtraBehaviour<StudioAmbientLightSceneController>(GUID);
        Logger.LogInfo($"MiunaKKSHelper v{Version} loaded from {Info.Location}");
    }
}
