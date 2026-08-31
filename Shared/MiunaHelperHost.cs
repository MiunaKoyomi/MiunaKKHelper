using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace MiunaKKHelper;

/// <summary>
/// KK / KKS 插件入口注入 Logger、热键、版本号。Shared 代码不要直接引用游戏侧入口类。
/// </summary>
public static class MiunaHelperHost
{
    public static ManualLogSource Logger { get; private set; }
    public static string PluginName { get; private set; } = "MiunaKKHelper";
    public static string Version { get; private set; } = string.Empty;
    public static string GUID { get; private set; } = string.Empty;
    public static ConfigEntry<KeyboardShortcut> Hotkey { get; private set; }
    public static ConfigFile Config { get; private set; }

    public static void Initialize(
        ManualLogSource logger,
        string pluginName,
        string version,
        string guid,
        ConfigEntry<KeyboardShortcut> hotkey,
        ConfigFile config)
    {
        Logger = logger;
        PluginName = pluginName ?? "MiunaKKHelper";
        Version = version ?? string.Empty;
        GUID = guid ?? string.Empty;
        Hotkey = hotkey;
        Config = config;
    }
}
