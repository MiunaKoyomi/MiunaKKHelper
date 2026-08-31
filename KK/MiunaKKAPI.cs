using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using KKAPI;
using KKAPI.Studio.SaveLoad;
using MiunaKKHelper.StudioTools;
using UnityEngine;

namespace MiunaKKHelper;

/// <summary>Miuna 个人 Koikatsu / CharaStudio 插件功能集合。热键默认 LeftAlt+M。</summary>
[BepInPlugin(GUID, PluginName, Version)]
[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
public class MiunaKKAPI : BaseUnityPlugin
{
    public const string PluginName = "MiunaKKHelper";
    public const string GUID = "org.miuna.plugins.KKHelper";
    public const string Version = "1.1.3";

    internal new static ManualLogSource Logger;

    public static ConfigEntry<KeyboardShortcut> hotkey;

    public static MiunaKKAPIUI UI;

    public void Awake()
    {
        Logger = base.Logger;
        KeyboardShortcut defaultShortcut = new KeyboardShortcut(KeyCode.M, KeyCode.LeftAlt);
        hotkey = Config.Bind("Miuna_KKHelper", "Hotkey", defaultShortcut, "Press this key to open the UI");
        MiunaHelperHost.Initialize(Logger, PluginName, Version, GUID, hotkey, Config);
        new Harmony(GUID).PatchAll(typeof(MiunaKKAPI).Assembly);
        UI = this.GetOrAddComponent<MiunaKKAPIUI>();
        Logger.LogInfo($"MiunaKKHelper v{Version} loaded from {Info.Location}");
    }

    public void Start()
    {
        StudioSaveLoadApi.RegisterExtraBehaviour<StudioAmbientLightSceneController>(GUID);
        StudioSaveLoadApi.RegisterExtraBehaviour<StudioSceneNameSceneController>(StudioSceneNameTools.ExtendedDataId);
    }
}
