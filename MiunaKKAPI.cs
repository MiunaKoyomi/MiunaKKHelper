using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Maker;
using KKAPI.Studio.SaveLoad;
using UnityEngine;

namespace KK_API_DOC;


[BepInPlugin(GUID, PluginName, Version)]
[BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
public class MiunaKKAPI: BaseUnityPlugin
{
    public const string PluginName = "MiunaKKHelper";
    public const string GUID = "org.miuna.plugins.KKHelper";
    public const string Version = "1.0.0";

    internal new static ManualLogSource Logger;
    
    public static ConfigEntry<KeyboardShortcut> hotkey;

    public static MiunaKKAPIUI UI;
    private void LogIfEnabled(LogLevel level, object data)
    {
      
    }
// internal Dictionary<int, ChaFileAccessory.PartsInfo> PartsInfo = new Dictionary<int, ChaFileAccessory.PartsInfo>();
    public void Awake()
    {
        Logger = base.Logger;
        KeyboardShortcut defaultShortcut = new KeyboardShortcut(KeyCode.M, KeyCode.LeftAlt);
        hotkey = Config.Bind("Miuna_KKHelper", "Hotkey", defaultShortcut, "Press this key to open the UI");
        //只有注册了才会调用 即addComponent GUID 和 extendData绑定
        UI = this.GetOrAddComponent<MiunaKKAPIUI>();
    }
}
