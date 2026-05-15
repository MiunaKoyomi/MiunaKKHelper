// // Decompiled with JetBrains decompiler
// // Type: CharacterAccessory
// // Assembly: KKS_CharacterAccessory, Version=1.8.2.0, Culture=neutral, PublicKeyToken=null
// // MVID: 47E714C7-77BF-4314-8152-8AA61CE09285
// // Assembly location: K:\KK\kk\Sunshine\Koikatsu Sunshine EX BetterRepack R12\BepInEx\plugins\Madevil\KKS_CharacterAccessory1.8.dll
//
// using BepInEx;
// using BepInEx.Bootstrap;
// using BepInEx.Configuration;
// using BepInEx.Logging;
// using ChaCustom;
// using ExtensibleSaveFormat;
// using HarmonyLib;
// using JetPack;
// using KK_Plugins;
// using KK_Plugins.DynamicBoneEditor;
// using KK_Plugins.MaterialEditor;
// using KKAPI;
// using KKAPI.Chara;
// using KKAPI.Maker;
// using KKAPI.Maker.UI;
// using KKAPI.Maker.UI.Sidebar;
// using KKAPI.Studio;
// using KKAPI.Studio.UI;
// using MessagePack;
// using ParadoxNotion.Serialization;
// using Sideloader.AutoResolver;
// using Studio;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using TMPro;
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.UI;
//
// #nullable disable
// namespace CharacterAccessory;
//
// [BepInPlugin("madevil.kk.ca", "Character Accessory", "1.8.2.0")]
// [BepInDependency("madevil.JetPack", "2.1.4.0")]
// [BepInDependency("marco.kkapi", "1.26")]
// [BepInDependency("com.bepis.bepinex.extendedsave", "16.8.1")]
// [BepInDependency("com.deathweasel.bepinex.materialeditor", "3.1.2")]
// [BepInIncompatibility("KK_ClothesLoadOption")]
// [BepInIncompatibility("com.jim60105.kk.studiocoordinateloadoption")]
// [BepInIncompatibility("com.jim60105.kk.coordinateloadoption")]
// public class CharacterAccessory : BaseUnityPlugin
// {
//   internal static MakerDropdown _makerDropdownReferral;
//   internal static MakerToggle _makerToggleEnable;
//   internal static MakerToggle _makerToggleAutoCopyToBlank;
//   internal static SidebarToggle _sidebarToggleEnable;
//   public const string GUID = "madevil.kk.ca";
//   public const string Name = "Character Accessory";
//   public const string Version = "1.8.2.0";
//   internal static ManualLogSource _logger;
//   internal static CharacterAccessory _instance;
//   internal static Dictionary<string, Harmony> _hooksInstance = new Dictionary<string, Harmony>();
//   internal const int PluginDataVersion = 3;
//   internal static List<string> _supportList = new List<string>();
//   internal static List<string> _cordNames = new List<string>();
//
//   public static CharacterAccessory.CharacterAccessoryController GetController(
//     ChaControl ChaControl)
//   {
//     return ChaControl?.gameObject?.GetComponent<CharacterAccessory.CharacterAccessoryController>();
//   }
//
//   public static CharacterAccessory.CharacterAccessoryController GetController(
//     OCIChar OCIChar)
//   {
//     return CharacterAccessory.GetController(OCIChar?.charInfo);
//   }
//
//   private void RegisterCustomSubCategories(object _sender, RegisterSubCategoriesEvent _args)
//   {
//     ChaControl _chaCtrl = Singleton<CustomBase>.Instance.chaCtrl;
//     CharacterAccessory.CharacterAccessoryController _pluginCtrl = CharacterAccessory.GetController(_chaCtrl);
//     CharacterAccessory._sidebarToggleEnable = _args.AddSidebarControl<SidebarToggle>(new SidebarToggle("CharaAcc", CharacterAccessory._cfgMakerMasterSwitch.Value, (BaseUnityPlugin) this));
//     CharacterAccessory._sidebarToggleEnable.ValueChanged.Subscribe<bool>((Action<bool>) (_value => CharacterAccessory._cfgMakerMasterSwitch.Value = _value));
//     MakerCategory category = new MakerCategory("03_ClothesTop", "tglCharaAcc", MakerConstants.Clothes.Copy.Position + 1, "CharaAcc");
//     _args.AddSubCategory(category);
//     _args.AddControl<MakerText>(new MakerText("The set to be used as a template to clone on load", category, (BaseUnityPlugin) this));
//     List<string> list = CharacterAccessory._cordNames.ToList<string>();
//     list.Add("CharaAcc");
//     CharacterAccessory._makerDropdownReferral = new MakerDropdown("Referral", list.ToArray(), category, 7, (BaseUnityPlugin) this);
//     CharacterAccessory._makerDropdownReferral.ValueChanged.Subscribe<int>((Action<int>) (_value => _pluginCtrl.SetReferralIndex(_value)));
//     _args.AddControl<MakerDropdown>(CharacterAccessory._makerDropdownReferral);
//     CharacterAccessory._makerToggleEnable = _args.AddControl<MakerToggle>(new MakerToggle(category, "Enable", false, (BaseUnityPlugin) this));
//     CharacterAccessory._makerToggleEnable.ValueChanged.Subscribe<bool>((Action<bool>) (_value => _pluginCtrl.FunctionEnable = _value));
//     CharacterAccessory._makerToggleAutoCopyToBlank = _args.AddControl<MakerToggle>(new MakerToggle(category, "Auto Copy To Blank", false, (BaseUnityPlugin) this));
//     CharacterAccessory._makerToggleAutoCopyToBlank.ValueChanged.Subscribe<bool>((Action<bool>) (_value => _pluginCtrl.AutoCopyToBlank = _value));
//     _args.AddControl<MakerButton>(new MakerButton("Backup", category, (BaseUnityPlugin) this)).OnClick.AddListener((UnityAction) (() =>
//     {
//       if (_pluginCtrl.DuringLoading)
//         return;
//       _pluginCtrl.Backup();
//       _pluginCtrl.SetReferralIndex(-1);
//       _pluginCtrl.FunctionEnable = true;
//       CharacterAccessory._makerToggleEnable.Value = _pluginCtrl.FunctionEnable;
//     }));
//     _args.AddControl<MakerButton>(new MakerButton("Restore", category, (BaseUnityPlugin) this)).OnClick.AddListener((UnityAction) (() =>
//     {
//       if (_pluginCtrl.DuringLoading)
//         return;
//       if (CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(_chaCtrl, _chaCtrl.fileStatus.coordinateType).Count > 0)
//       {
//         CharacterAccessory._logger.LogMessage((object) "Please clear the accessories on current coordinate before using this function");
//       }
//       else
//       {
//         _pluginCtrl.TaskLock();
//         _pluginCtrl.RestorePartsInfo();
//       }
//     }));
//     _args.AddControl<MakerButton>(new MakerButton("Reset", category, (BaseUnityPlugin) this)).OnClick.AddListener((UnityAction) (() =>
//     {
//       if (_pluginCtrl.DuringLoading)
//         return;
//       _pluginCtrl.Reset();
//       _pluginCtrl.SetReferralIndex(-1);
//       CharacterAccessory._makerToggleEnable.Value = _pluginCtrl.FunctionEnable;
//       CharacterAccessory._makerToggleAutoCopyToBlank.Value = _pluginCtrl.AutoCopyToBlank;
//     }));
//     if (!Game.ConsoleActive)
//       return;
//     _args.AddControl<MakerSeparator>(new MakerSeparator(category, (BaseUnityPlugin) this));
//     _args.AddControl<MakerButton>(new MakerButton("MaterialRouter", category, (BaseUnityPlugin) this)).OnClick.AddListener((UnityAction) (() => CharacterAccessory._logger.LogInfo((object) ("[MaterialRouter]\n" + _pluginCtrl.MaterialRouter.Report()))));
//   }
//
//   internal static ConfigEntry<bool> _cfgMakerMasterSwitch { get; set; }
//
//   internal static ConfigEntry<bool> _cfgDebugMode { get; set; }
//
//   internal static ConfigEntry<bool> _cfgStudioFallbackReload { get; set; }
//
//   internal static ConfigEntry<bool> _cfgMAHookUpdateStudioUI { get; set; }
//
//   private void Awake()
//   {
//     CharacterAccessory._logger = this.Logger;
//     CharacterAccessory._instance = this;
//     CharacterAccessory._cfgMakerMasterSwitch = this.Config.Bind<bool>("Maker", "Master Switch", true, new ConfigDescription("A quick switch on the sidebar that templary disable the function", (AcceptableValueBase) null, new object[1]
//     {
//       (object) new KKAPI.Utilities.ConfigurationManagerAttributes()
//       {
//         IsAdvanced = new bool?(true)
//       }
//     }));
//     CharacterAccessory._cfgStudioFallbackReload = this.Config.Bind<bool>("Studio", "Fallback Reload Mode", false, new ConfigDescription("Enable this if some plugins are having visual problem", (AcceptableValueBase) null, new object[1]
//     {
//       (object) new KKAPI.Utilities.ConfigurationManagerAttributes()
//       {
//         IsAdvanced = new bool?(true)
//       }
//     }));
//     CharacterAccessory._cfgDebugMode = this.Config.Bind<bool>("Debug", "Debug Mode", false, new ConfigDescription("Showing debug messages in LogWarning level", (AcceptableValueBase) null, new object[1]
//     {
//       (object) new KKAPI.Utilities.ConfigurationManagerAttributes()
//       {
//         IsAdvanced = new bool?(true)
//       }
//     }));
//     CharacterAccessory._cfgMAHookUpdateStudioUI = this.Config.Bind<bool>("Hook", "MoreAccessories UpdateStudioUI", true, new ConfigDescription("Performance tweak, disable it if having issue on studio chara state panel update", (AcceptableValueBase) null, new object[1]
//     {
//       (object) new KKAPI.Utilities.ConfigurationManagerAttributes()
//       {
//         IsAdvanced = new bool?(true)
//       }
//     }));
//   }
//
//   private void Start()
//   {
//     CharacterAccessory._cordNames = ((IEnumerable<string>) Enum.GetNames(typeof (ChaFileDefine.CoordinateType))).ToList<string>();
//     CharacterApi.RegisterExtraBehaviour<CharacterAccessory.CharacterAccessoryController>("madevil.kk.ca");
//     CharacterAccessory._hooksInstance["General"] = Harmony.CreateAndPatchAll(typeof (CharacterAccessory.Hooks));
//     CharacterAccessory.MoreAccessoriesSupport.Init();
//     int num = CharacterAccessory.MoreAccessoriesSupport._installed ? 1 : 0;
//     CharacterAccessory.MoreOutfitsSupport.Init();
//     CharacterAccessory.HairAccessoryCustomizerSupport.Init();
//     CharacterAccessory.MaterialEditorSupport.Init();
//     CharacterAccessory.MaterialRouterSupport.Init();
//     CharacterAccessory.AccStateSyncSupport.Init();
//     CharacterAccessory.DynamicBoneEditorSupport.Init();
//     CharacterAccessory.AAAPKSupport.Init();
//     CharacterAccessory.BendUrAccSupport.Init();
//     CharacterAccessory.CumOnOverSupport.Init();
//     CharacterAccessory.BonerStateSync.Init();
//     if (CharaStudio.Running)
//     {
//       CharaStudio.OnStudioLoaded += (EventHandler) ((_sender, _args) => CharacterAccessory.RegisterStudioControls());
//     }
//     else
//     {
//       MakerAPI.MakerBaseLoaded += (EventHandler<RegisterCustomControlsEvent>) ((_sender, _args) =>
//       {
//         CharacterAccessory._hooksInstance["Maker"] = Harmony.CreateAndPatchAll(typeof (CharacterAccessory.HooksMaker));
//         CharacterAccessory.MoreOutfitsSupport.MakerInit();
//         BaseUnityPlugin pluginInstance = Toolbox.GetPluginInstance("ClothingStateMenu");
//         if (!((UnityEngine.Object) pluginInstance != (UnityEngine.Object) null))
//           return;
//         CharacterAccessory._hooksInstance["Maker"].Patch((MethodBase) pluginInstance.GetType().GetMethod("OnGUI", AccessTools.all), new HarmonyMethod(typeof (CharacterAccessory.HooksMaker), "DuringLoading_Prefix"));
//       });
//       MakerAPI.MakerExiting += (EventHandler) ((_sender, _args) =>
//       {
//         CharacterAccessory._hooksInstance["Maker"].UnpatchAll(CharacterAccessory._hooksInstance["Maker"].Id);
//         CharacterAccessory._hooksInstance["Maker"] = (Harmony) null;
//         CharacterAccessory._makerDropdownReferral = (MakerDropdown) null;
//         CharacterAccessory._makerToggleEnable = (MakerToggle) null;
//         CharacterAccessory._makerToggleAutoCopyToBlank = (MakerToggle) null;
//         CharacterAccessory._sidebarToggleEnable = (SidebarToggle) null;
//       });
//       MakerAPI.RegisterCustomSubCategories += new EventHandler<RegisterSubCategoriesEvent>(this.RegisterCustomSubCategories);
//     }
//   }
//
//   internal static void DebugMsg(BepInEx.Logging.LogLevel LogLevel, string LogMsg)
//   {
//     if (CharacterAccessory._cfgDebugMode.Value)
//       CharacterAccessory._logger.Log(LogLevel, (object) LogMsg);
//     else
//       CharacterAccessory._logger.Log(BepInEx.Logging.LogLevel.Debug, (object) LogMsg);
//   }
//
//   internal static void RegisterStudioControls()
//   {
//     CurrentStateCategorySwitch control1 = new CurrentStateCategorySwitch("Enable", (Func<OCIChar, bool>) (OCIChar => CharacterAccessory.GetController(OCIChar)?.FunctionEnable.Value));
//     control1.Value.Subscribe<bool>((Action<bool>) (_value =>
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = StudioAPI.GetSelectedControllers<CharacterAccessory.CharacterAccessoryController>().FirstOrDefault<CharacterAccessory.CharacterAccessoryController>();
//       if ((UnityEngine.Object) accessoryController == (UnityEngine.Object) null)
//         return;
//       accessoryController.FunctionEnable = _value;
//     }));
//     StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategorySwitch>(control1);
//     CurrentStateCategorySwitch control2 = new CurrentStateCategorySwitch("Copy To Blank", (Func<OCIChar, bool>) (OCIChar => CharacterAccessory.GetController(OCIChar)?.AutoCopyToBlank.Value));
//     control2.Value.Subscribe<bool>((Action<bool>) (_value =>
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = StudioAPI.GetSelectedControllers<CharacterAccessory.CharacterAccessoryController>().FirstOrDefault<CharacterAccessory.CharacterAccessoryController>();
//       if ((UnityEngine.Object) accessoryController == (UnityEngine.Object) null)
//         return;
//       accessoryController.AutoCopyToBlank = _value;
//     }));
//     StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategorySwitch>(control2);
//     List<string> list = ((IEnumerable<string>) Enum.GetNames(typeof (ChaFileDefine.CoordinateType))).ToList<string>();
//     list.Add("CharaAcc");
//     CurrentStateCategoryDropdown control3 = new CurrentStateCategoryDropdown("Referral", list.ToArray(), (Func<OCIChar, int>) (OCIChar => CharacterAccessory.GetController(OCIChar)?.GetReferralIndex().Value));
//     control3.Value.Subscribe<int>((Action<int>) (_value =>
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = StudioAPI.GetSelectedControllers<CharacterAccessory.CharacterAccessoryController>().FirstOrDefault<CharacterAccessory.CharacterAccessoryController>();
//       if ((UnityEngine.Object) accessoryController == (UnityEngine.Object) null)
//         return;
//       accessoryController.SetReferralIndex(_value);
//     }));
//     StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategoryDropdown>(control3);
//   }
//
//   public class CharacterAccessoryController : CharaCustomFunctionController
//   {
//     internal CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag HairAccessoryCustomizer;
//     internal CharacterAccessory.MaterialEditorSupport.UrineBag MaterialEditor;
//     internal CharacterAccessory.MaterialRouterSupport.UrineBag MaterialRouter;
//     internal CharacterAccessory.AccStateSyncSupport.UrineBag AccStateSync;
//     internal CharacterAccessory.DynamicBoneEditorSupport.UrineBag DynamicBoneEditor;
//     internal CharacterAccessory.AAAPKSupport.UrineBag AAAPK;
//     internal CharacterAccessory.BendUrAccSupport.UrineBag BendUrAcc;
//     internal Dictionary<int, ChaFileAccessory.PartsInfo> PartsInfo = new Dictionary<int, ChaFileAccessory.PartsInfo>();
//     internal Dictionary<int, ResolveInfo> PartsResolveInfo = new Dictionary<int, ResolveInfo>();
//     internal int ReferralIndex = -1;
//     internal bool FunctionEnable;
//     internal bool AutoCopyToBlank;
//     internal bool DuringLoading;
//     internal List<CharacterAccessory.CharacterAccessoryController.QueueItem> QueueList = new List<CharacterAccessory.CharacterAccessoryController.QueueItem>();
//
//     internal int CurrentCoordinateIndex => this.ChaControl.fileStatus.coordinateType;
//
//     protected override void Start()
//     {
//       if (KoikatuAPI.GetCurrentGameMode() == GameMode.MainGame)
//         return;
//       this.HairAccessoryCustomizer = new CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag(this.ChaControl);
//       this.MaterialEditor = new CharacterAccessory.MaterialEditorSupport.UrineBag(this.ChaControl);
//       this.MaterialRouter = new CharacterAccessory.MaterialRouterSupport.UrineBag(this.ChaControl);
//       this.AccStateSync = new CharacterAccessory.AccStateSyncSupport.UrineBag(this.ChaControl);
//       this.DynamicBoneEditor = new CharacterAccessory.DynamicBoneEditorSupport.UrineBag(this.ChaControl);
//       this.AAAPK = new CharacterAccessory.AAAPKSupport.UrineBag(this.ChaControl);
//       this.BendUrAcc = new CharacterAccessory.BendUrAccSupport.UrineBag(this.ChaControl);
//       this.CurrentCoordinate.Subscribe<ChaFileDefine.CoordinateType>((Action<ChaFileDefine.CoordinateType>) (value => this.OnCoordinateChanged()));
//       base.Start();
//     }
//
//     private void OnCoordinateChanged()
//     {
//       this.TaskUnlock();
//       this.AutoCopyCheck();
//     }
//
//     protected override void OnCardBeingSaved(GameMode currentGameMode)
//     {
//       this.TaskUnlock();
//       PluginData data = new PluginData() { version = 3 };
//       data.data.Add("MoreAccessoriesExtdata", (object) MessagePackSerializer.Serialize<Dictionary<int, ChaFileAccessory.PartsInfo>>(this.PartsInfo));
//       data.data.Add("ResolutionInfoExtdata", (object) MessagePackSerializer.Serialize<Dictionary<int, ResolveInfo>>(this.PartsResolveInfo));
//       foreach (string support in CharacterAccessory._supportList)
//       {
//         object obj = Traverse.Create((object) this).Field(support).Method("Save").GetValue();
//         data.data.Add(support + "Extdata", (object) MessagePackSerializer.Serialize<object>(obj));
//       }
//       data.data.Add("FunctionEnable", (object) this.FunctionEnable);
//       data.data.Add("AutoCopyToBlank", (object) this.AutoCopyToBlank);
//       data.data.Add("ReferralIndex", (object) this.ReferralIndex);
//       data.data.Add("TextureContainer", (object) MessagePackSerializer.Serialize<Dictionary<int, byte[]>>(this.MaterialEditor.TexContainer));
//       this.SetExtendedData(data);
//     }
//
//     protected override void OnReload(GameMode currentGameMode)
//     {
//       this.TaskUnlock();
//       PluginData extendedData = this.GetExtendedData();
//       this.PartsInfo.Clear();
//       this.PartsResolveInfo.Clear();
//       this.FunctionEnable = false;
//       this.AutoCopyToBlank = false;
//       this.ReferralIndex = -1;
//       this.MaterialEditor.Reset();
//       if (extendedData != null)
//       {
//         if (extendedData.version > 3)
//         {
//           CharacterAccessory._logger.Log(BepInEx.Logging.LogLevel.Error | BepInEx.Logging.LogLevel.Message, (object) $"[OnReload] ExtendedData.version: {extendedData.version} is newer than your plugin");
//           base.OnReload(currentGameMode);
//           return;
//         }
//         if (extendedData.version < 3)
//           CharacterAccessory._logger.Log(BepInEx.Logging.LogLevel.Info, (object) $"[OnReload] Migrating from ver. {extendedData.version}");
//         object bytes1;
//         if (extendedData.data.TryGetValue("MoreAccessoriesExtdata", out bytes1) && bytes1 != null)
//           this.PartsInfo = MessagePackSerializer.Deserialize<Dictionary<int, ChaFileAccessory.PartsInfo>>((byte[]) bytes1);
//         object bytes2;
//         if (extendedData.data.TryGetValue("ResolutionInfoExtdata", out bytes2) && bytes2 != null)
//           this.PartsResolveInfo = MessagePackSerializer.Deserialize<Dictionary<int, ResolveInfo>>((byte[]) bytes2);
//         foreach (string support in CharacterAccessory._supportList)
//         {
//           object bytes3;
//           if (extendedData.data.TryGetValue(support + "Extdata", out bytes3) && bytes3 != null)
//           {
//             switch (support)
//             {
//               case "HairAccessoryCustomizer":
//                 Traverse.Create((object) this).Field(support).Method("Load", (object) MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[]) bytes3)).GetValue();
//                 continue;
//               case "AccStateSync":
//                 if (extendedData.version < 2)
//                 {
//                   Traverse.Create((object) this).Field(support).Method("Migrate", (object) MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[]) bytes3)).GetValue();
//                   continue;
//                 }
//                 Traverse.Create((object) this).Field(support).Method("Load", (object) MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[]) bytes3)).GetValue();
//                 continue;
//               case "MaterialEditor":
//                 Traverse.Create((object) this).Field(support).Method("Load", (object) MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[]) bytes3)).GetValue();
//                 continue;
//               case "MaterialRouter":
//               case "DynamicBoneEditor":
//               case "AAAPK":
//               case "BendUrAcc":
//                 Traverse.Create((object) this).Field(support).Method("Load", (object) MessagePackSerializer.Deserialize<List<string>>((byte[]) bytes3)).GetValue();
//                 continue;
//               default:
//                 continue;
//             }
//           }
//         }
//         object obj1;
//         if (extendedData.data.TryGetValue("FunctionEnable", out obj1) && obj1 != null)
//           this.FunctionEnable = (bool) obj1;
//         object obj2;
//         if (extendedData.data.TryGetValue("AutoCopyToBlank", out obj2) && obj2 != null)
//           this.AutoCopyToBlank = (bool) obj2;
//         object _index;
//         if (extendedData.data.TryGetValue("ReferralIndex", out _index) && _index != null)
//         {
//           if (extendedData.version < 3)
//             this.SetReferralIndex(-1);
//           else
//             this.SetReferralIndex((int) _index);
//           CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Info, $"[OnReload][{this.ChaControl.GetFullName()}][ReferralIndex: {this.ReferralIndex}]");
//         }
//         object bytes4;
//         if (extendedData.data.TryGetValue("TextureContainer", out bytes4) && bytes4 != null)
//           this.MaterialEditor.TexContainer = MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[]) bytes4);
//         Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = this.PartsInfo;
//         // ISSUE: explicit non-virtual call
//         if ((partsInfo != null ? partsInfo.Count > 0 ? 1 : 0 : 0) != 0)
//         {
//           Dictionary<int, ResolveInfo> partsResolveInfo = this.PartsResolveInfo;
//           // ISSUE: explicit non-virtual call
//           if ((partsResolveInfo != null ? partsResolveInfo.Count > 0 ? 1 : 0 : 0) != 0)
//           {
//             foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> keyValuePair in this.PartsInfo)
//             {
//               ResolveInfo resolutionInfo;
//               this.PartsResolveInfo.TryGetValue(keyValuePair.Key, out resolutionInfo);
//               if (resolutionInfo != null)
//               {
//                 CharacterAccessoryController.MigrateData(ref resolutionInfo);
//                 if (resolutionInfo != null && !resolutionInfo.GUID.IsNullOrWhiteSpace())
//                 {
//                   resolutionInfo = UniversalAutoResolver.TryGetResolutionInfo(this.PartsResolveInfo[keyValuePair.Key].Slot, this.PartsResolveInfo[keyValuePair.Key].CategoryNo, this.PartsResolveInfo[keyValuePair.Key].GUID);
//                   if (resolutionInfo != null)
//                   {
//                     this.PartsResolveInfo[keyValuePair.Key] = resolutionInfo.JsonClone() as ResolveInfo;
//                     keyValuePair.Value.id = resolutionInfo.LocalSlot;
//                   }
//                 }
//               }
//             }
//           }
//         }
//       }
//       if (MakerAPI.InsideAndLoaded)
//       {
//         CharacterAccessory._makerToggleEnable.Value = this.FunctionEnable;
//         CharacterAccessory._makerToggleAutoCopyToBlank.Value = this.AutoCopyToBlank;
//         CharacterAccessory.MoreOutfitsSupport.BuildMakerDropdownRef();
//       }
//       if (CharaStudio.Running && (UnityEngine.Object) CharaStudio.CurOCIChar?.charInfo == (UnityEngine.Object) this.ChaControl)
//         CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
//       this.ChaControl.StartCoroutine(OnReloadCoroutine());
//       base.OnReload(currentGameMode);
//
//       IEnumerator OnReloadCoroutine()
//       {
//         CharacterAccessory.CharacterAccessoryController accessoryController = this;
//         CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[OnReloadCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//         yield return (object) Toolbox.WaitForEndOfFrame;
//         yield return (object) Toolbox.WaitForEndOfFrame;
//         accessoryController.AutoCopyCheck();
//       }
//     }
//
//     protected override void OnCoordinateBeingLoaded(ChaFileCoordinate _coordinate)
//     {
//       this.TaskUnlock();
//       bool flag = true;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[OnCoordinateBeingLoaded][{this.ChaControl.GetFullName()}][FunctionEnable: {this.FunctionEnable}][ReferralIndex: {this.ReferralIndex}][PartsInfo.Count: {this.PartsInfo.Count}]");
//       if (!this.FunctionEnable)
//         flag = false;
//       if (this.ReferralIndex == -1 && this.PartsInfo.Count == 0)
//         flag = false;
//       if (MakerAPI.InsideAndLoaded && !CharacterAccessory._cfgMakerMasterSwitch.Value)
//         flag = false;
//       CoordinateLoadFlags coordinateLoadFlags = MakerAPI.GetCoordinateLoadFlags();
//       if (MakerAPI.InsideAndLoaded && coordinateLoadFlags != null && !coordinateLoadFlags.Accessories)
//         flag = false;
//       if (flag)
//       {
//         this.TaskLock();
//         this.ChaControl.StartCoroutine(this.OnCoordinateBeingLoadedCoroutine());
//       }
//       else if (MakerAPI.InsideAndLoaded)
//         Singleton<CustomBase>.Instance.updateCustomUI = true;
//       base.OnCoordinateBeingLoaded(_coordinate);
//     }
//
//     internal IEnumerator OnCoordinateBeingLoadedCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[OnCoordinateBeingLoadedCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.TaskLock();
//       accessoryController.PrepareQueue();
//     }
//
//     internal IEnumerator RefreshCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RefreshCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.TaskUnlock();
//       if (CharaStudio.Running)
//       {
//         if (CharacterAccessory._cfgStudioFallbackReload.Value)
//         {
//           accessoryController.BigReload();
//         }
//         else
//         {
//           accessoryController.FastReload();
//           accessoryController.ChaControl.ChangeCoordinateTypeAndReload(false);
//         }
//       }
//       else
//       {
//         accessoryController.ChaControl.ChangeCoordinateTypeAndReload(false);
//         if (MakerAPI.InsideAndLoaded)
//           Singleton<CustomBase>.Instance.updateCustomUI = true;
//       }
//     }
//
//     internal IEnumerator PreviewCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[PreviewCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.AccStateSync.InitCurOutfitTriggerInfo("OnCoordinateBeingLoaded");
//       if (CharaStudio.Loaded)
//         accessoryController.StartCoroutine(accessoryController.RefreshCharaStatePanelCoroutine());
//     }
//
//     internal IEnumerator RefreshCharaStatePanelCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RefreshCharaStatePanelCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.HairAccessoryCustomizer.UpdateAccessories(false);
//       CharaStudio.RefreshCharaStatePanel();
//       CharacterAccessory.MoreAccessoriesSupport.UpdateStudioUI(accessoryController.ChaControl);
//     }
//
//     internal void SetReferralIndex(int _index)
//     {
//       if (this.ReferralIndex != _index)
//         this.ReferralIndex = _index >= this.ChaControl.chaFile.coordinate.Length || _index < 0 ? -1 : _index;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[SetReferralIndex][{this.ChaControl.GetFullName()}][_index: {_index}][ReferralIndex: {this.ReferralIndex}]");
//     }
//
//     internal int GetReferralIndex()
//     {
//       if (CharaStudio.Running && (UnityEngine.Object) CharaStudio.CurOCIChar?.charInfo == (UnityEngine.Object) this.ChaControl)
//         CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
//       int referralIndex = this.ReferralIndex < 0 ? this.ChaControl.chaFile.coordinate.Length : this.ReferralIndex;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Info, $"[GetReferralIndex][{this.ChaControl.GetFullName()}][_index: {referralIndex}][ReferralIndex: {this.ReferralIndex}]");
//       return referralIndex;
//     }
//
//     internal void FastReload(bool _noLoadStatus = true)
//     {
//       byte[] buffer = (byte[]) null;
//       using (MemoryStream output = new MemoryStream())
//       {
//         using (BinaryWriter bw = new BinaryWriter((Stream) output))
//         {
//           this.ChaControl.chaFile.SaveCharaFile(bw, false);
//           buffer = output.ToArray();
//         }
//       }
//       using (MemoryStream input = new MemoryStream(buffer))
//       {
//         using (BinaryReader br = new BinaryReader((Stream) input))
//           this.ChaControl.chaFile.LoadCharaFile(br, true, _noLoadStatus);
//       }
//     }
//
//     internal void BigReload()
//     {
//       string str = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileNameWithoutExtension(BepInEx.Paths.ExecutablePath) + "_CA.png");
//       using (FileStream st = new FileStream(str, FileMode.Create, FileAccess.Write))
//         this.ChaControl.chaFile.SaveCharaFile((Stream) st, true);
//       Singleton<Studio.Studio>.Instance.dicInfo.Values.OfType<OCIChar>().FirstOrDefault<OCIChar>((Func<OCIChar, bool>) (x => (UnityEngine.Object) x.charInfo == (UnityEngine.Object) this.ChaControl)).ChangeChara(str);
//     }
//
//     internal string GetCordName() => this.GetCordName(this.CurrentCoordinateIndex);
//
//     internal string GetCordName(int CoordinateIndex)
//     {
//       return CoordinateIndex < CharacterAccessory._cordNames.Count ? CharacterAccessory._cordNames[CoordinateIndex] : $"Extra {CoordinateIndex - CharacterAccessory._cordNames.Count + 1}";
//     }
//
//     internal void TaskLock() => this.DuringLoading = true;
//
//     internal void TaskUnlock() => this.DuringLoading = false;
//
//     internal void AutoCopyCheck()
//     {
//       bool flag = true;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[OnCoordinateChanged][{this.ChaControl.GetFullName()}][CurrentCoordinateIndex: {this.CurrentCoordinateIndex}]");
//       if (!this.AutoCopyToBlank)
//         flag = false;
//       if (!this.FunctionEnable)
//         flag = false;
//       if (this.ReferralIndex == -1 && this.PartsInfo.Count == 0)
//         flag = false;
//       if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length && this.ReferralIndex == this.CurrentCoordinateIndex)
//         flag = false;
//       if (MakerAPI.InsideAndLoaded && !CharacterAccessory._cfgMakerMasterSwitch.Value)
//         flag = false;
//       if (!flag)
//         return;
//       this.ChaControl.StartCoroutine(this.OnCoordinateChangedCoroutine());
//     }
//
//     internal IEnumerator OnCoordinateChangedCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[OnCoordinateChangedCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       if (CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(accessoryController.ChaControl, accessoryController.CurrentCoordinateIndex).Count <= 0)
//       {
//         accessoryController.TaskLock();
//         if (accessoryController.ReferralIndex > -1 && accessoryController.ReferralIndex < accessoryController.ChaControl.chaFile.coordinate.Length)
//           accessoryController.CopyPartsInfo();
//         else
//           accessoryController.RestorePartsInfo();
//       }
//     }
//
//     internal void PrepareQueue()
//     {
//       this.QueueList = new List<CharacterAccessory.CharacterAccessoryController.QueueItem>();
//       if (this.ReferralIndex >= this.ChaControl.chaFile.coordinate.Length)
//         this.TaskUnlock();
//       else if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length && this.ReferralIndex == this.CurrentCoordinateIndex)
//       {
//         this.TaskUnlock();
//       }
//       else
//       {
//         int num1 = -1;
//         if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length)
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(this.ChaControl, this.ReferralIndex);
//           num1 = dictionary.Count == 0 ? -1 : dictionary.Keys.Max();
//         }
//         else if (this.ReferralIndex == -1)
//           num1 = this.PartsInfo.Count == 0 ? -1 : this.PartsInfo.Keys.Max();
//         CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[PrepareQueue][{this.ChaControl.GetFullName()}][ReferralIndex: {this.ReferralIndex}][SrcLastNotEmpty: Slot{num1 + 1:00}]");
//         if (num1 < 0)
//         {
//           this.TaskUnlock();
//         }
//         else
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(this.ChaControl, this.CurrentCoordinateIndex);
//           if (dictionary.Count == 0)
//           {
//             if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length)
//             {
//               this.CopyPartsInfo();
//             }
//             else
//             {
//               if (this.ReferralIndex != -1)
//                 return;
//               this.RestorePartsInfo();
//             }
//           }
//           else
//           {
//             int num2 = 0;
//             int num3 = 0;
//             int num4 = dictionary.Keys.Min();
//             int num5 = dictionary.Keys.Max();
//             List<int> list = dictionary.Keys.ToList<int>();
//             List<int> intList = new List<int>();
//             intList.AddRange((IEnumerable<int>) list);
//             intList.Reverse();
//             CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[PrepareQueue][{this.ChaControl.GetFullName()}][CurrentCoordinateIndex: {this.CurrentCoordinateIndex}][CurFirstNotEmpty: Slot{num4 + 1:00}][CurLastNotEmpty: Slot{num5 + 1:00}]");
//             if (num4 <= num1)
//             {
//               num3 = num1 - num4 + 1;
//               num2 = num5 + num3 - CharacterAccessory.MoreAccessoriesSupport.GetPartsCount(this.ChaControl, this.CurrentCoordinateIndex);
//             }
//             if (num3 > 0)
//             {
//               foreach (int _src in intList)
//                 this.QueueList.Add(new CharacterAccessory.CharacterAccessoryController.QueueItem(_src, _src + num3));
//             }
//             if (num2 > 0)
//             {
//               CharacterAccessory.MoreAccessoriesSupport.CheckAndPadPartInfo(this.ChaControl, this.CurrentCoordinateIndex, num5 + num3);
//               this.StartCoroutine(this.TransferPartsInfoCoroutine());
//             }
//             else if (this.QueueList.Count > 0)
//               this.TransferPartsInfo();
//             else if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length)
//             {
//               this.CopyPartsInfo();
//             }
//             else
//             {
//               if (this.ReferralIndex != -1)
//                 return;
//               this.ChaControl.ChangeCoordinateTypeAndReload(false);
//               this.StartCoroutine(this.RestorePartsInfoCoroutine());
//             }
//           }
//         }
//       }
//     }
//
//     internal IEnumerator TransferPartsInfoCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[TransferPartsInfoCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.TransferPartsInfo();
//     }
//
//     internal void TransferPartsInfo()
//     {
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[TransferPartsInfo][{this.ChaControl.GetFullName()}] fired");
//       if (this.QueueList.Count == 0)
//       {
//         this.TaskUnlock();
//       }
//       else
//       {
//         for (int index = 0; index < this.QueueList.Count; ++index)
//         {
//           int srcSlot = this.QueueList[index].SrcSlot;
//           int dstSlot = this.QueueList[index].DstSlot;
//           CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[TransferPartsInfo][{this.ChaControl.GetFullName()}][{srcSlot}][{dstSlot}]");
//           AccessoryTransferEventArgs ev = new AccessoryTransferEventArgs(srcSlot, dstSlot);
//           CharacterAccessory.MoreAccessoriesSupport.TransferPartsInfo(this.ChaControl, ev);
//           CharacterAccessory.MoreAccessoriesSupport.RemovePartsInfo(this.ChaControl, this.CurrentCoordinateIndex, srcSlot);
//           foreach (string support in CharacterAccessory._supportList)
//           {
//             Traverse.Create((object) this).Field(support).Method(nameof (TransferPartsInfo), (object) ev).GetValue();
//             Traverse.Create((object) this).Field(support).Method("RemovePartsInfo", (object) srcSlot).GetValue();
//           }
//         }
//         if (this.ReferralIndex > -1 && this.ReferralIndex < this.ChaControl.chaFile.coordinate.Length)
//         {
//           this.CopyPartsInfo();
//         }
//         else
//         {
//           if (this.ReferralIndex != -1)
//             return;
//           this.ChaControl.ChangeCoordinateTypeAndReload(false);
//           this.ChaControl.StartCoroutine(this.RestorePartsInfoCoroutine());
//         }
//       }
//     }
//
//     internal void CopyPartsInfo()
//     {
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[CopyPartsInfo][{this.ChaControl.GetFullName()}] fired");
//       if (!this.DuringLoading)
//         this.TaskUnlock();
//       else if (!this.FunctionEnable)
//         this.TaskUnlock();
//       else if (this.ReferralIndex == this.CurrentCoordinateIndex)
//       {
//         this.TaskUnlock();
//       }
//       else
//       {
//         List<ChaFileAccessory.PartsInfo> partsInfoList = CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(this.ChaControl, this.ReferralIndex);
//         List<int> intList = new List<int>();
//         for (int index = 0; index < partsInfoList.Count; ++index)
//         {
//           if (partsInfoList[index].type > 120)
//             intList.Add(index);
//         }
//         CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[CopyPartsInfo][{this.ChaControl.GetFullName()}][Slots: {string.Join(",", intList.Select<int, string>((Func<int, string>) (Slot => Slot.ToString())).ToArray<string>())}]");
//         AccessoryCopyEventArgs ev = new AccessoryCopyEventArgs((IEnumerable<int>) intList, (ChaFileDefine.CoordinateType) this.ReferralIndex, (ChaFileDefine.CoordinateType) this.CurrentCoordinateIndex);
//         CharacterAccessory.MoreAccessoriesSupport.CopyPartsInfo(this.ChaControl, ev);
//         if (CharaStudio.Running)
//         {
//           this.ChaControl.ChangeCoordinateTypeAndReload(false);
//           this.ChaControl.StartCoroutine(this.CopyPluginSettingCoroutine(ev));
//         }
//         else
//         {
//           foreach (string support in CharacterAccessory._supportList)
//             Traverse.Create((object) this).Field(support).Method(nameof (CopyPartsInfo), (object) ev).GetValue();
//           this.ChaControl.StartCoroutine(this.RefreshCoroutine());
//         }
//       }
//     }
//
//     internal IEnumerator CopyPluginSettingCoroutine(AccessoryCopyEventArgs ev)
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[CopyPluginSettingCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.CopyPluginSetting(ev);
//     }
//
//     internal void CopyPluginSetting(AccessoryCopyEventArgs ev)
//     {
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[CopyPluginSetting][{this.ChaControl.GetFullName()}] fired");
//       foreach (string support in CharacterAccessory._supportList)
//         Traverse.Create((object) this).Field(support).Method("CopyPartsInfo", (object) ev).GetValue();
//       this.ChaControl.StartCoroutine(this.RefreshCoroutine());
//     }
//
//     internal void Backup()
//     {
//       int coordinateType = this.ChaControl.fileStatus.coordinateType;
//       List<ChaFileAccessory.PartsInfo> partsInfoList = CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(this.ChaControl, coordinateType);
//       this.PartsInfo.Clear();
//       this.PartsResolveInfo.Clear();
//       for (int index = 0; index < partsInfoList.Count; ++index)
//       {
//         ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(this.ChaControl, coordinateType, index);
//         if (partsInfo.type > 120)
//         {
//           byte[] bytes = MessagePackSerializer.Serialize<ChaFileAccessory.PartsInfo>(partsInfo);
//           this.PartsInfo[index] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes);
//           this.PartsResolveInfo[index] = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo) partsInfo.type, partsInfo.id);
//         }
//       }
//       foreach (string support in CharacterAccessory._supportList)
//         Traverse.Create((object) this).Field(support).Method(nameof (Backup)).GetValue();
//     }
//
//     internal static void MigrateData(ref ResolveInfo extResolve)
//     {
//       if (extResolve.GUID.IsNullOrWhiteSpace())
//         return;
//       List<MigrationInfo> migrationInfo1 = UniversalAutoResolver.GetMigrationInfo(extResolve.GUID);
//       if (migrationInfo1.Any<MigrationInfo>((Func<MigrationInfo, bool>) (x => x.MigrationType == MigrationType.StripAll)))
//       {
//         extResolve.GUID = "";
//       }
//       else
//       {
//         int slot = extResolve.Slot;
//         ChaListDefine.CategoryNo categoryNo = extResolve.CategoryNo;
//         foreach (MigrationInfo migrationInfo2 in migrationInfo1.Where<MigrationInfo>((Func<MigrationInfo, bool>) (x => x.IDOld == slot && x.Category == categoryNo)))
//         {
//           if (Sideloader.Sideloader.GetManifest(migrationInfo2.GUIDNew) != null)
//           {
//             extResolve.GUID = migrationInfo2.GUIDNew;
//             extResolve.Slot = migrationInfo2.IDNew;
//             return;
//           }
//         }
//         foreach (MigrationInfo migrationInfo3 in migrationInfo1.Where<MigrationInfo>((Func<MigrationInfo, bool>) (x => x.MigrationType == MigrationType.MigrateAll)))
//         {
//           if (Sideloader.Sideloader.GetManifest(migrationInfo3.GUIDNew) != null)
//             extResolve.GUID = migrationInfo3.GUIDNew;
//         }
//       }
//     }
//
//     internal IEnumerator RestorePartsInfoCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RestoreCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.RestorePartsInfo();
//     }
//
//     internal void Reset()
//     {
//       this.FunctionEnable = false;
//       this.AutoCopyToBlank = false;
//       this.ReferralIndex = -1;
//       this.PartsInfo.Clear();
//       this.PartsResolveInfo.Clear();
//       foreach (string support in CharacterAccessory._supportList)
//         Traverse.Create((object) this).Field(support).Method(nameof (Reset)).GetValue();
//     }
//
//     internal void RestorePartsInfo()
//     {
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RestorePartsInfo][{this.ChaControl.GetFullName()}] fired");
//       if (!this.DuringLoading)
//         return;
//       if (!this.FunctionEnable)
//         this.TaskUnlock();
//       else if (this.PartsInfo.Count == 0)
//       {
//         CharacterAccessory._logger.LogMessage((object) "Nothing to restore");
//         this.TaskUnlock();
//       }
//       else
//       {
//         int coordinateType = this.ChaControl.fileStatus.coordinateType;
//         Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(this.ChaControl, coordinateType);
//         if (dictionary.Count > 0 && dictionary.Keys.Min() <= this.PartsInfo.Keys.Max())
//         {
//           CharacterAccessory._logger.LogMessage((object) $"Error: parts overlap [RefUsedPartsInfo.Keys.Min(): {dictionary.Keys.Min()}][PartsInfo.Keys.Max(): {this.PartsInfo.Keys.Max()}]");
//           this.TaskUnlock();
//         }
//         else
//         {
//           CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Info, $"[RestorePartsInfo][{this.ChaControl.GetFullName()}][Slots: {string.Join(",", this.PartsInfo.Keys.Select<int, string>((Func<int, string>) (Slot => Slot.ToString())).ToArray<string>())}]");
//           foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> keyValuePair in this.PartsInfo)
//             CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(this.ChaControl, coordinateType, keyValuePair.Key, keyValuePair.Value);
//           if (CharaStudio.Running)
//           {
//             this.ChaControl.ChangeCoordinateTypeAndReload(false);
//             this.StartCoroutine(this.RestorePluginSettingCoroutine());
//           }
//           else
//           {
//             foreach (string support in CharacterAccessory._supportList)
//               Traverse.Create((object) this).Field(support).Method("Restore").GetValue();
//             this.StartCoroutine(this.RefreshCoroutine());
//           }
//         }
//       }
//     }
//
//     internal IEnumerator RestorePluginSettingCoroutine()
//     {
//       CharacterAccessory.CharacterAccessoryController accessoryController = this;
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RestorePluginSettingCoroutine][{accessoryController.ChaControl.GetFullName()}] fired");
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       yield return (object) Toolbox.WaitForEndOfFrame;
//       accessoryController.RestorePluginSetting();
//     }
//
//     internal void RestorePluginSetting()
//     {
//       CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[RestorePluginSetting][{this.ChaControl.GetFullName()}] fired");
//       foreach (string support in CharacterAccessory._supportList)
//         Traverse.Create((object) this).Field(support).Method("Restore").GetValue();
//       this.StartCoroutine(this.RefreshCoroutine());
//     }
//
//     internal class QueueItem
//     {
//       public int SrcSlot { get; set; }
//
//       public int DstSlot { get; set; }
//
//       public QueueItem(int _src, int _dst)
//       {
//         this.SrcSlot = _src;
//         this.DstSlot = _dst;
//       }
//     }
//   }
//
//   internal class Hooks
//   {
//     internal static bool DuringLoading_Prefix(CharaCustomFunctionController __instance)
//     {
//       return !CharacterAccessory.GetController(__instance.ChaControl).DuringLoading;
//     }
//
//     internal static bool DuringLoading_IEnumerator_Prefix(
//       CharaCustomFunctionController __instance,
//       ref IEnumerator __result)
//     {
//       if (!CharacterAccessory.GetController(__instance.ChaControl).DuringLoading)
//         return true;
//       IEnumerator enumerator = __result;
//       __result = new IEnumerator[2]
//       {
//         enumerator,
//         YieldBreak()
//       }.GetEnumerator();
//       return false;
//
//       static IEnumerator YieldBreak()
//       {
//         yield break;
//       }
//     }
//   }
//
//   internal class HooksMaker
//   {
//     internal static bool DuringLoading_Prefix()
//     {
//       CharacterAccessory.CharacterAccessoryController controller = CharacterAccessory.GetController(Singleton<CustomBase>.Instance.chaCtrl);
//       return (UnityEngine.Object) controller == (UnityEngine.Object) null || !controller.DuringLoading;
//     }
//   }
//
//   internal static class AAAPKSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static bool _legacy = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("madevil.kk.AAAPK", out pluginInfo);
//       CharacterAccessory.AAAPKSupport._instance = pluginInfo?.Instance;
//       if (!((UnityEngine.Object) CharacterAccessory.AAAPKSupport._instance != (UnityEngine.Object) null))
//         return;
//       CharacterAccessory.AAAPKSupport._legacy = pluginInfo.Metadata.Version.CompareTo(new System.Version("1.1.0.0")) < 0;
//       if (CharacterAccessory.AAAPKSupport._legacy)
//       {
//         CharacterAccessory._logger.LogError((object) $"AAAPK version {pluginInfo.Metadata.Version} found, minimun version 1.1 is reqired");
//       }
//       else
//       {
//         CharacterAccessory.AAAPKSupport._installed = true;
//         CharacterAccessory._supportList.Add("AAAPK");
//         Assembly assembly = CharacterAccessory.AAAPKSupport._instance.GetType().Assembly;
//         CharacterAccessory.AAAPKSupport._types["AAAPKController"] = assembly.GetType("AAAPK.AAAPK+AAAPKController");
//         CharacterAccessory.AAAPKSupport._types["ParentRule"] = assembly.GetType("AAAPK.AAAPK+ParentRule");
//         CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.AAAPKSupport._types["AAAPKController"].GetMethod("ApplyParentRuleList", AccessTools.all, (System.Reflection.Binder) null, new System.Type[1]
//         {
//           typeof (string)
//         }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_Prefix"));
//       }
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       return Traverse.Create((object) CharacterAccessory.AAAPKSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaCustomFunctionController _pluginCtrl;
//       private readonly List<object> _charaAccData = new List<object>();
//       private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.AAAPKSupport.GetController(this._chaCtrl);
//         this._traverses["pluginCtrl"] = Traverse.Create((object) this._pluginCtrl);
//       }
//
//       internal object GetExtDataLink()
//       {
//         return this._traverses["pluginCtrl"].Field("ParentRuleList").GetValue();
//       }
//
//       internal void Reset()
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         this._charaAccData.Clear();
//       }
//
//       internal List<string> Save()
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return (List<string>) null;
//         List<string> stringList = new List<string>();
//         foreach (object obj in this._charaAccData)
//           stringList.Add(JSONSerializer.Serialize(CharacterAccessory.AAAPKSupport._types["ParentRule"], obj));
//         return stringList;
//       }
//
//       internal void Load(List<string> _json)
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         if (_json == null)
//           return;
//         foreach (string serializedState in _json)
//           this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.AAAPKSupport._types["ParentRule"], serializedState));
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
//         List<int> intList1;
//         if (partsInfo == null)
//         {
//           intList1 = (List<int>) null;
//         }
//         else
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
//           intList1 = keys != null ? keys.ToList<int>() : (List<int>) null;
//         }
//         List<int> intList2 = intList1;
//         int count = (extDataLink as IList).Count;
//         for (int _key = 0; _key < count; ++_key)
//         {
//           object root = extDataLink.RefElementAt(_key).JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           if (traverse.Property("Coordinate").GetValue<int>() == coordinateType && intList2.Contains(traverse.Property("Slot").GetValue<int>()))
//           {
//             traverse.Property("Coordinate").SetValue((object) -1);
//             this._charaAccData.Add(root);
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         for (int index = 0; index < this._charaAccData.Count; ++index)
//         {
//           object root = this._charaAccData[index].JsonClone();
//           Traverse.Create(root).Property("Coordinate").SetValue((object) coordinateType);
//           (extDataLink as IList).Add(root);
//         }
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         foreach (int copiedSlotIndex in _args.CopiedSlotIndexes)
//           this._traverses["pluginCtrl"].Method("CloneRule", (object) copiedSlotIndex, (object) copiedSlotIndex, (object) (int) _args.CopySource, (object) (int) _args.CopyDestination).GetValue();
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         this._traverses["pluginCtrl"].Method("MoveRule", (object) _args.SourceSlotIndex, (object) _args.DestinationSlotIndex, (object) coordinateType).GetValue();
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         if (!CharacterAccessory.AAAPKSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("RemoveRule", (object) _slotIndex).GetValue();
//       }
//     }
//   }
//
//   internal static class BendUrAccSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("madevil.kk.BendUrAcc", out pluginInfo);
//       CharacterAccessory.BendUrAccSupport._instance = pluginInfo?.Instance;
//       if (!((UnityEngine.Object) CharacterAccessory.BendUrAccSupport._instance != (UnityEngine.Object) null))
//         return;
//       if (pluginInfo.Metadata.Version.CompareTo(new System.Version("1.0.5.0")) < 0)
//       {
//         CharacterAccessory._logger.LogError((object) $"BendUrAcc version {pluginInfo.Metadata.Version} found, minimun version 1.0.5.0 is reqired");
//       }
//       else
//       {
//         CharacterAccessory.BendUrAccSupport._installed = true;
//         CharacterAccessory._supportList.Add("BendUrAcc");
//         Assembly assembly = CharacterAccessory.BendUrAccSupport._instance.GetType().Assembly;
//         CharacterAccessory.BendUrAccSupport._types["BendUrAccController"] = assembly.GetType("BendUrAcc.BendUrAcc+BendUrAccController");
//         CharacterAccessory.BendUrAccSupport._types["BendModifier"] = assembly.GetType("BendUrAcc.BendUrAcc+BendModifier");
//         CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.BendUrAccSupport._types["BendUrAccController"].GetMethod("ApplyBendModifierList", AccessTools.all, (System.Reflection.Binder) null, new System.Type[1]
//         {
//           typeof (string)
//         }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_Prefix"));
//       }
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       return Traverse.Create((object) CharacterAccessory.BendUrAccSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaCustomFunctionController _pluginCtrl;
//       private readonly List<object> _charaAccData = new List<object>();
//       private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.BendUrAccSupport.GetController(this._chaCtrl);
//         this._traverses["pluginCtrl"] = Traverse.Create((object) this._pluginCtrl);
//       }
//
//       internal object GetExtDataLink()
//       {
//         return this._traverses["pluginCtrl"].Field("BendModifierList").GetValue();
//       }
//
//       internal void Reset()
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         this._charaAccData.Clear();
//       }
//
//       internal List<string> Save()
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return (List<string>) null;
//         List<string> stringList = new List<string>();
//         foreach (object obj in this._charaAccData)
//           stringList.Add(JSONSerializer.Serialize(CharacterAccessory.BendUrAccSupport._types["BendModifier"], obj));
//         return stringList;
//       }
//
//       internal void Load(List<string> _json)
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         if (_json == null)
//           return;
//         foreach (string serializedState in _json)
//           this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.BendUrAccSupport._types["BendModifier"], serializedState));
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
//         List<int> intList1;
//         if (partsInfo == null)
//         {
//           intList1 = (List<int>) null;
//         }
//         else
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
//           intList1 = keys != null ? keys.ToList<int>() : (List<int>) null;
//         }
//         List<int> intList2 = intList1;
//         int count = (extDataLink as IList).Count;
//         for (int _key = 0; _key < count; ++_key)
//         {
//           object root = extDataLink.RefElementAt(_key).JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           if (traverse.Property("Coordinate").GetValue<int>() == coordinateType && intList2.Contains(traverse.Property("Slot").GetValue<int>()))
//           {
//             traverse.Property("Coordinate").SetValue((object) -1);
//             this._charaAccData.Add(root);
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         for (int index = 0; index < this._charaAccData.Count; ++index)
//         {
//           object root = this._charaAccData[index].JsonClone();
//           Traverse.Create(root).Property("Coordinate").SetValue((object) coordinateType);
//           (extDataLink as IList).Add(root);
//         }
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         foreach (int copiedSlotIndex in _args.CopiedSlotIndexes)
//           this._traverses["pluginCtrl"].Method("CloneModifier", (object) copiedSlotIndex, (object) copiedSlotIndex, (object) (int) _args.CopySource, (object) (int) _args.CopyDestination).GetValue();
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         this._traverses["pluginCtrl"].Method("CloneModifier", (object) _args.SourceSlotIndex, (object) _args.DestinationSlotIndex, (object) coordinateType, (object) coordinateType).GetValue();
//         this._traverses["pluginCtrl"].Method("RemoveSlotModifier", (object) coordinateType, (object) _args.SourceSlotIndex).GetValue();
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         if (!CharacterAccessory.BendUrAccSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("RemoveSlotModifier", (object) _slotIndex).GetValue();
//       }
//     }
//   }
//
//   internal static class BonerStateSync
//   {
//     internal static BaseUnityPlugin _instance;
//     internal static bool _installed;
//
//     internal static void Init()
//     {
//       CharacterAccessory.BonerStateSync._instance = Toolbox.GetPluginInstance(nameof (BonerStateSync));
//       if ((UnityEngine.Object) CharacterAccessory.BonerStateSync._instance != (UnityEngine.Object) null)
//         CharacterAccessory.BonerStateSync._installed = true;
//       if (!CharacterAccessory.BonerStateSync._installed)
//       {
//         CharacterAccessory.BonerStateSync._instance = Toolbox.GetPluginInstance("madevil.kk.BonerStateSync");
//         if ((UnityEngine.Object) CharacterAccessory.BonerStateSync._instance != (UnityEngine.Object) null)
//           CharacterAccessory.BonerStateSync._installed = true;
//       }
//       if (!CharacterAccessory.BonerStateSync._installed)
//         return;
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.BonerStateSync._instance.GetType().Assembly.GetType("BonerStateSync.BonerStateSync+BonerStateSyncController").GetMethod("InitCurOutfitTriggerInfo", AccessTools.all, (System.Reflection.Binder) null, new System.Type[1]
//       {
//         typeof (string)
//       }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_Prefix"));
//     }
//   }
//
//   internal static class CumOnOverSupport
//   {
//     internal static BaseUnityPlugin _instance;
//     internal static bool _installed;
//
//     internal static void Init()
//     {
//       CharacterAccessory.CumOnOverSupport._instance = Toolbox.GetPluginInstance("madevil.kk.CumOnOver");
//       if ((UnityEngine.Object) CharacterAccessory.CumOnOverSupport._instance != (UnityEngine.Object) null)
//         CharacterAccessory.CumOnOverSupport._installed = true;
//       if (!CharacterAccessory.CumOnOverSupport._installed)
//         return;
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.CumOnOverSupport._instance.GetType().Assembly.GetType("CumOnOver.CumOnOver+Hooks").GetMethod("ChaControl_UpdateClothesSiru", AccessTools.all), new HarmonyMethod(typeof (CharacterAccessory.CumOnOverSupport.Hooks), "ChaControl_UpdateClothesSiru_Prefix"));
//     }
//
//     internal static class Hooks
//     {
//       internal static bool ChaControl_UpdateClothesSiru_Prefix(ChaControl __0)
//       {
//         bool flag = true;
//         if (CharacterAccessory.GetController(__0).DuringLoading)
//           flag = false;
//         if (flag)
//           return flag;
//         CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[ChaControl_UpdateClothesSiru_Prefix][{__0.GetFullName()}] await loading");
//         return false;
//       }
//     }
//   }
//
//   internal static class MoreAccessoriesSupport
//   {
//     internal static BaseUnityPlugin _instance;
//     internal static bool _installed;
//     internal static bool BuggyBootleg;
//
//     internal static void Init()
//     {
//       CharacterAccessory.MoreAccessoriesSupport._installed = MoreAccessories.Installed;
//       if (!CharacterAccessory.MoreAccessoriesSupport._installed)
//         return;
//       CharacterAccessory.MoreAccessoriesSupport._instance = MoreAccessories.Instance;
//       Assembly assembly = CharacterAccessory.MoreAccessoriesSupport._instance.GetType().Assembly;
//       CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg = MoreAccessories.BuggyBootleg;
//       if (CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg)
//         return;
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.MoreAccessoriesSupport._instance.GetType().Assembly.GetType("MoreAccessoriesKOI.ChaControl_UpdateVisible_Patches").GetMethod("Postfix", AccessTools.all, (System.Reflection.Binder) null, new System.Type[1]
//       {
//         typeof (ChaControl)
//       }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.MoreAccessoriesSupport.Hooks), "ChaControl_UpdateVisible_Patches_Prefix"));
//       if (!CharaStudio.Running)
//         return;
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.MoreAccessoriesSupport._instance.GetType().GetMethod("UpdateStudioUI", AccessTools.all, (System.Reflection.Binder) null, new System.Type[0], (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.MoreAccessoriesSupport.Hooks), "MoreAccessories_UpdateStudioUI_Prefix"));
//     }
//
//     internal static void UpdateStudioUI(ChaControl _chaCtrl)
//     {
//       if (CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg || CharaStudio.CurOCIChar == null || (UnityEngine.Object) CharaStudio.CurOCIChar.charInfo != (UnityEngine.Object) _chaCtrl)
//         return;
//       AccessTools.Method(CharacterAccessory.MoreAccessoriesSupport._instance.GetType(), "UpdateUI").Invoke((object) CharacterAccessory.MoreAccessoriesSupport._instance, (object[]) null);
//     }
//
//     internal static int GetPartsCount(ChaControl chaCtrl, int CoordinateIndex)
//     {
//       return CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(chaCtrl, CoordinateIndex)?.Count.Value + 20;
//     }
//
//     internal static Dictionary<int, ChaFileAccessory.PartsInfo> ListUsedPartsInfo(
//       ChaControl _chaCtrl,
//       int _coordinateIndex)
//     {
//       Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = new Dictionary<int, ChaFileAccessory.PartsInfo>();
//       int key = 0;
//       foreach (ChaFileAccessory.PartsInfo partsInfo in CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(_chaCtrl, _coordinateIndex))
//       {
//         if (partsInfo.type > 120)
//           dictionary[key] = partsInfo;
//         ++key;
//       }
//       return dictionary;
//     }
//
//     internal static ChaFileAccessory.PartsInfo GetPartsInfo(
//       ChaControl _chaCtrl,
//       int _coordinateIndex,
//       int _slotIndex)
//     {
//       return Accessory.GetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex);
//     }
//
//     internal static void SetPartsInfo(
//       ChaControl _chaCtrl,
//       int _coordinateIndex,
//       int _slotIndex,
//       ChaFileAccessory.PartsInfo _part)
//     {
//       byte[] bytes = MessagePackSerializer.Serialize<ChaFileAccessory.PartsInfo>(_part);
//       Accessory.SetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex, MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(bytes));
//     }
//
//     internal static List<ChaFileAccessory.PartsInfo> ListPartsInfo(
//       ChaControl _chaCtrl,
//       int _coordinateIndex)
//     {
//       return Accessory.ListPartsInfo(_chaCtrl, _coordinateIndex);
//     }
//
//     internal static void CheckAndPadPartInfo(
//       ChaControl _chaCtrl,
//       int _coordinateIndex,
//       int _slotIndex)
//     {
//       MoreAccessories.CheckAndPadPartInfo(_chaCtrl, _coordinateIndex, _slotIndex);
//     }
//
//     internal static ChaAccessoryComponent GetChaAccessoryComponent(
//       ChaControl _chaCtrl,
//       int _slotIndex)
//     {
//       return Accessory.GetChaAccessoryComponent(_chaCtrl, _slotIndex);
//     }
//
//     internal static bool IsHairAccessory(ChaControl _chaCtrl, int _slotIndex)
//     {
//       return Accessory.IsHairAccessory(_chaCtrl, _slotIndex);
//     }
//
//     internal static void CopyPartsInfo(ChaControl _chaCtrl, AccessoryCopyEventArgs ev)
//     {
//       foreach (int copiedSlotIndex in ev.CopiedSlotIndexes)
//       {
//         ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(_chaCtrl, (int) ev.CopySource, copiedSlotIndex);
//         CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, (int) ev.CopyDestination, copiedSlotIndex, partsInfo);
//       }
//     }
//
//     internal static void TransferPartsInfo(ChaControl _chaCtrl, AccessoryTransferEventArgs ev)
//     {
//       int coordinateType = _chaCtrl.fileStatus.coordinateType;
//       ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(_chaCtrl, coordinateType, ev.SourceSlotIndex);
//       CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, coordinateType, ev.DestinationSlotIndex, partsInfo);
//     }
//
//     internal static void RemovePartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex)
//     {
//       CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex, new ChaFileAccessory.PartsInfo());
//     }
//
//     internal static class Hooks
//     {
//       internal static bool ChaControl_UpdateVisible_Patches_Prefix(ChaControl __0)
//       {
//         CharacterAccessory.CharacterAccessoryController controller = CharacterAccessory.GetController(__0);
//         return (UnityEngine.Object) controller == (UnityEngine.Object) null || !controller.DuringLoading;
//       }
//
//       internal static bool MoreAccessories_UpdateStudioUI_Prefix(object __instance)
//       {
//         if (!CharacterAccessory._cfgMAHookUpdateStudioUI.Value)
//           return true;
//         bool flag = true;
//         if (CharaStudio.CurOCIChar != null && CharacterAccessory.GetController(CharaStudio.CurOCIChar).DuringLoading)
//           flag = false;
//         return flag && flag;
//       }
//     }
//   }
//
//   internal static class MoreOutfitsSupport
//   {
//     private static BaseUnityPlugin _instance;
//     private static bool _installed;
//     internal static TMP_Dropdown _makerDropdownRef;
//     internal static Dropdown _studioDropdownRef;
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.moreoutfits", out pluginInfo);
//       CharacterAccessory.MoreOutfitsSupport._instance = pluginInfo?.Instance;
//       if (!((UnityEngine.Object) CharacterAccessory.MoreOutfitsSupport._instance != (UnityEngine.Object) null))
//         return;
//       CharacterAccessory.MoreOutfitsSupport._installed = true;
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       return Traverse.Create((object) CharacterAccessory.MoreOutfitsSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal static Dictionary<int, string> CoordinateNames(
//       CharaCustomFunctionController _pluginCtrl)
//     {
//       return Traverse.Create((object) _pluginCtrl).Field(nameof (CoordinateNames)).GetValue<Dictionary<int, string>>();
//     }
//
//     internal static string GetCoodinateName(ChaControl _chaCtrl, int _coordinateIndex)
//     {
//       return MoreOutfits.GetCoodinateName(_chaCtrl, _coordinateIndex);
//     }
//
//     internal static string GetCoodinateName(
//       CharaCustomFunctionController _pluginCtrl,
//       int _coordinateIndex)
//     {
//       return Traverse.Create((object) _pluginCtrl).Method(nameof (GetCoodinateName), (object) _coordinateIndex).GetValue<string>();
//     }
//
//     internal static void MakerInit()
//     {
//       if (!CharacterAccessory.MoreOutfitsSupport._installed)
//         return;
//       CharacterAccessory.MoreOutfitsSupport._makerDropdownRef = (TMP_Dropdown) null;
//       CharacterAccessory._hooksInstance["Maker"].Patch((MethodBase) CharacterAccessory.MoreOutfitsSupport._instance.GetType().Assembly.GetType("KK_Plugins.MoreOutfits.MakerUI").GetMethod("UpdateMakerUI", AccessTools.all), postfix: new HarmonyMethod(typeof (CharacterAccessory.MoreOutfitsSupport.Hooks), "UpdateMakerUI_Postfix"));
//     }
//
//     internal static void StudioInit()
//     {
//       if (!CharacterAccessory.MoreOutfitsSupport._installed)
//         return;
//       CharacterAccessory._hooksInstance["Studio"].Patch((MethodBase) CharacterAccessory.MoreOutfitsSupport._instance.GetType().Assembly.GetType("KK_Plugins.MoreOutfits.StudioUI").GetMethod("InitializeStudioUI", AccessTools.all), postfix: new HarmonyMethod(typeof (CharacterAccessory.MoreOutfitsSupport.Hooks), "InitializeStudioUI_Postfix"));
//     }
//
//     internal static void BuildMakerDropdownRef()
//     {
//       ChaControl chaCtrl = Singleton<CustomBase>.Instance.chaCtrl;
//       int newValue = chaCtrl?.gameObject?.GetComponent<CharacterAccessory.CharacterAccessoryController>()?.GetReferralIndex().Value;
//       if (!CharacterAccessory.MoreOutfitsSupport._installed)
//       {
//         CharacterAccessory._makerDropdownReferral.SetValue(newValue);
//       }
//       else
//       {
//         if ((UnityEngine.Object) CharacterAccessory.MoreOutfitsSupport._makerDropdownRef == (UnityEngine.Object) null)
//           CharacterAccessory.MoreOutfitsSupport._makerDropdownRef = GameObject.Find("tglCharaAcc")?.GetComponentInChildren<TMP_Dropdown>(true);
//         if ((UnityEngine.Object) CharacterAccessory.MoreOutfitsSupport._makerDropdownRef == (UnityEngine.Object) null)
//         {
//           CharacterAccessory._logger.LogError((object) "[BuildDropdownRef] failed to get dropdown component");
//         }
//         else
//         {
//           List<string> list = CharacterAccessory._cordNames.ToList<string>();
//           for (int count = list.Count; count < chaCtrl.chaFile.coordinate.Length; ++count)
//             list.Add(CharacterAccessory.MoreOutfitsSupport.GetCoodinateName(chaCtrl, count));
//           list.Add("CharaAcc");
//           CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.ClearOptions();
//           CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.options.AddRange(list.Select<string, TMP_Dropdown.OptionData>((Func<string, TMP_Dropdown.OptionData>) (x => new TMP_Dropdown.OptionData(x))));
//           CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.value = newValue;
//           CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.RefreshShownValue();
//         }
//       }
//     }
//
//     internal static void BuildStudioDropdownRef()
//     {
//       if (!CharacterAccessory.MoreOutfitsSupport._installed)
//         return;
//       if ((UnityEngine.Object) CharacterAccessory.MoreOutfitsSupport._studioDropdownRef == (UnityEngine.Object) null)
//         CharacterAccessory.MoreOutfitsSupport._studioDropdownRef = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/CharaAcc_Items_SAPI/CustomDropdown Referral/Dropdown")?.GetComponent<Dropdown>();
//       if ((UnityEngine.Object) CharacterAccessory.MoreOutfitsSupport._studioDropdownRef == (UnityEngine.Object) null)
//       {
//         CharacterAccessory._logger.LogError((object) "[BuildDropdownRef] failed to get dropdown component");
//       }
//       else
//       {
//         List<Dropdown.OptionData> options = CharacterAccessory.MoreOutfitsSupport._studioDropdownRef.options;
//         ChaControl charInfo = CharaStudio.CurOCIChar?.charInfo;
//         if ((UnityEngine.Object) charInfo == (UnityEngine.Object) null)
//         {
//           options.RemoveRange(CharacterAccessory._cordNames.Count, options.Count - CharacterAccessory._cordNames.Count);
//           options.Add(new Dropdown.OptionData("CharaAcc"));
//         }
//         else
//         {
//           options.RemoveRange(CharacterAccessory._cordNames.Count, options.Count - CharacterAccessory._cordNames.Count);
//           for (int count = CharacterAccessory._cordNames.Count; count < charInfo.chaFile.coordinate.Length; ++count)
//           {
//             if (count < CharacterAccessory._cordNames.Count)
//               options.Add(new Dropdown.OptionData(CharacterAccessory._cordNames[count]));
//             else
//               options.Add(new Dropdown.OptionData(CharacterAccessory.MoreOutfitsSupport.GetCoodinateName(charInfo, count)));
//           }
//           options.Add(new Dropdown.OptionData("CharaAcc"));
//         }
//       }
//     }
//
//     internal static class Hooks
//     {
//       internal static void UpdateMakerUI_Postfix()
//       {
//         CharacterAccessory.MoreOutfitsSupport.BuildMakerDropdownRef();
//       }
//
//       internal static void InitializeStudioUI_Postfix(MPCharCtrl __0)
//       {
//         CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
//       }
//     }
//   }
//
//   internal static class DynamicBoneEditorSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//
//     internal static void Init()
//     {
//       CharacterAccessory.DynamicBoneEditorSupport._instance = Toolbox.GetPluginInstance("com.deathweasel.bepinex.dynamicboneeditor");
//       if (!((UnityEngine.Object) CharacterAccessory.DynamicBoneEditorSupport._instance != (UnityEngine.Object) null))
//         return;
//       CharacterAccessory.DynamicBoneEditorSupport._installed = true;
//       CharacterAccessory._supportList.Add("DynamicBoneEditor");
//       Assembly assembly = CharacterAccessory.DynamicBoneEditorSupport._instance.GetType().Assembly;
//       CharacterAccessory.DynamicBoneEditorSupport._types["CharaController"] = assembly.GetType("KK_Plugins.DynamicBoneEditor.CharaController");
//       CharacterAccessory.DynamicBoneEditorSupport._types["DynamicBoneData"] = assembly.GetType("KK_Plugins.DynamicBoneEditor.DynamicBoneData");
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.DynamicBoneEditorSupport._types["CharaController"].GetMethod("ApplyData", AccessTools.all), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_IEnumerator_Prefix"));
//     }
//
//     internal static CharaController GetController(ChaControl _chaCtrl)
//     {
//       return Plugin.GetCharaController(_chaCtrl);
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaController _pluginCtrl;
//       private readonly List<DynamicBoneData> _charaAccData = new List<DynamicBoneData>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.DynamicBoneEditorSupport.GetController(this._chaCtrl);
//       }
//
//       internal List<DynamicBoneData> GetExtDataLink() => this._pluginCtrl.AccessoryDynamicBoneData;
//
//       internal void Reset()
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._charaAccData.Clear();
//       }
//
//       internal List<string> Save()
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return (List<string>) null;
//         List<string> stringList = new List<string>();
//         foreach (object obj in this._charaAccData)
//           stringList.Add(JSONSerializer.Serialize(typeof (DynamicBoneData), obj));
//         return stringList;
//       }
//
//       internal void Load(List<string> _json)
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         if (_json == null)
//           return;
//         foreach (string serializedState in _json)
//           this._charaAccData.Add(JSONSerializer.Deserialize<DynamicBoneData>(serializedState));
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         List<DynamicBoneData> extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int _coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
//         List<int> intList;
//         if (partsInfo == null)
//         {
//           intList = (List<int>) null;
//         }
//         else
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
//           intList = keys != null ? keys.ToList<int>() : (List<int>) null;
//         }
//         List<int> _slots = intList;
//         this._charaAccData.AddRange((IEnumerable<DynamicBoneData>) extDataLink.Where<DynamicBoneData>((Func<DynamicBoneData, bool>) (x => x.CoordinateIndex == _coordinateIndex && _slots.Contains(x.Slot))).ToList<DynamicBoneData>().JsonClone<List<DynamicBoneData>>());
//         this._charaAccData.ForEach((Action<DynamicBoneData>) (x => x.CoordinateIndex = -1));
//         int count = extDataLink.Count;
//         for (int index = 0; index < count; ++index)
//         {
//           DynamicBoneData root = extDataLink.ElementAtOrDefault<DynamicBoneData>(index).JsonClone<DynamicBoneData>();
//           Traverse traverse = Traverse.Create((object) root);
//           if (traverse.Field("CoordinateIndex").GetValue<int>() == _coordinateIndex && _slots.IndexOf(traverse.Field("Slot").GetValue<int>()) >= 0)
//           {
//             traverse.Field("CoordinateIndex").SetValue((object) -1);
//             this._charaAccData.Add(root);
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         List<DynamicBoneData> extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int _coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
//         List<DynamicBoneData> collection = this._charaAccData.JsonClone<List<DynamicBoneData>>();
//         collection.ForEach((Action<DynamicBoneData>) (x => x.CoordinateIndex = _coordinateIndex));
//         extDataLink.AddRange((IEnumerable<DynamicBoneData>) collection);
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._pluginCtrl.AccessoriesCopiedEvent((object) null, _args);
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         List<DynamicBoneData> extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         this.RemovePartsInfo(_args.DestinationSlotIndex);
//         int _coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
//         List<DynamicBoneData> collection = extDataLink.Where<DynamicBoneData>((Func<DynamicBoneData, bool>) (x => x.CoordinateIndex == _coordinateIndex && x.Slot == _args.SourceSlotIndex)).ToList<DynamicBoneData>().JsonClone<List<DynamicBoneData>>();
//         collection.ForEach((Action<DynamicBoneData>) (x => x.Slot = _args.DestinationSlotIndex));
//         extDataLink.AddRange((IEnumerable<DynamicBoneData>) collection);
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
//           return;
//         this._pluginCtrl.AccessoryKindChangeEvent((object) null, new AccessorySlotEventArgs(_slotIndex));
//       }
//     }
//   }
//
//   internal static class HairAccessoryCustomizerSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//
//     internal static void Init()
//     {
//       CharacterAccessory.HairAccessoryCustomizerSupport._instance = Toolbox.GetPluginInstance("com.deathweasel.bepinex.hairaccessorycustomizer");
//       if (!((UnityEngine.Object) CharacterAccessory.HairAccessoryCustomizerSupport._instance != (UnityEngine.Object) null))
//         return;
//       CharacterAccessory.HairAccessoryCustomizerSupport._installed = true;
//       CharacterAccessory._supportList.Add("HairAccessoryCustomizer");
//       Assembly assembly = CharacterAccessory.HairAccessoryCustomizerSupport._instance.GetType().Assembly;
//       CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryController"] = assembly.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController");
//       CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryInfo"] = assembly.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController+HairAccessoryInfo");
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryController"].GetMethod("UpdateAccessories", AccessTools.all, (System.Reflection.Binder) null, new System.Type[1]
//       {
//         typeof (bool)
//       }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_Prefix"));
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//         return (CharaCustomFunctionController) null;
//       return Traverse.Create((object) CharacterAccessory.HairAccessoryCustomizerSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaCustomFunctionController _pluginCtrl;
//       private readonly Dictionary<int, object> _charaAccData = new Dictionary<int, object>();
//       private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.HairAccessoryCustomizerSupport.GetController(this._chaCtrl);
//         this._traverses["pluginCtrl"] = Traverse.Create((object) this._pluginCtrl);
//       }
//
//       internal object GetExtDataLink(int _coordinateIndex)
//       {
//         object _self = this._traverses["pluginCtrl"].Field("HairAccessories").GetValue();
//         return _self == null ? (object) null : _self.RefTryGetValue((object) _coordinateIndex);
//       }
//
//       internal void Reset() => this._charaAccData.Clear();
//
//       internal Dictionary<int, string> Save()
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return (Dictionary<int, string>) null;
//         Dictionary<int, string> dictionary = new Dictionary<int, string>();
//         foreach (KeyValuePair<int, object> keyValuePair in this._charaAccData)
//         {
//           CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo hairAccessoryInfo = new CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo(keyValuePair.Value);
//           dictionary[keyValuePair.Key] = JSONSerializer.Serialize(typeof (CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo), (object) hairAccessoryInfo);
//         }
//         return dictionary;
//       }
//
//       internal void Load(Dictionary<int, string> _json)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         if (_json == null)
//           return;
//         foreach (KeyValuePair<int, string> keyValuePair in _json)
//         {
//           CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo hairAccessoryInfo = JSONSerializer.Deserialize<CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo>(keyValuePair.Value);
//           this._charaAccData[keyValuePair.Key] = hairAccessoryInfo.Convert();
//         }
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         object extDataLink = this.GetExtDataLink(this._chaCtrl.fileStatus.coordinateType);
//         if (extDataLink == null)
//           return;
//         foreach (int num in Traverse.Create(extDataLink).Property("Keys").GetValue<ICollection<int>>().ToList<int>())
//         {
//           if (CharacterAccessory.MoreAccessoriesSupport.IsHairAccessory(this._chaCtrl, num))
//           {
//             object _self = extDataLink.RefTryGetValue((object) num);
//             if (_self != null)
//               this._charaAccData[num] = _self.JsonClone();
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         object extDataLink = this.GetExtDataLink(this._chaCtrl.fileStatus.coordinateType);
//         if (extDataLink == null)
//           return;
//         foreach (KeyValuePair<int, object> keyValuePair in this._charaAccData)
//         {
//           if (extDataLink.RefTryGetValue((object) keyValuePair.Key) != null)
//           {
//             CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[HairAccessoryCustomizer][Restore][{this._chaCtrl.GetFullName()}][{keyValuePair.Key}] remove HairAccessoryInfo");
//             (extDataLink as IDictionary).Remove((object) keyValuePair.Key);
//           }
//           (extDataLink as IDictionary).Add((object) keyValuePair.Key, keyValuePair.Value.JsonClone());
//         }
//       }
//
//       internal void UpdateAccessories(bool _updateHairInfo = true)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (UpdateAccessories), (object) _updateHairInfo).GetValue();
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("CopyAccessoriesHandler", (object) _args).GetValue();
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("TransferAccessoriesHandler", (object) _args).GetValue();
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("RemoveHairAccessoryInfo", (object) _slotIndex).GetValue();
//       }
//
//       internal class FakeHairAccessoryInfo
//       {
//         public bool HairGloss;
//         public bool ColorMatch;
//         public Color OutlineColor = Color.white;
//         public Color AccessoryColor = Color.white;
//         public float HairLength;
//
//         public FakeHairAccessoryInfo(object _info)
//         {
//           Traverse traverse = Traverse.Create(_info);
//           this.HairGloss = traverse.Field(nameof (HairGloss)).GetValue<bool>();
//           this.ColorMatch = traverse.Field(nameof (ColorMatch)).GetValue<bool>();
//           this.OutlineColor = traverse.Field(nameof (OutlineColor)).GetValue<Color>();
//           this.AccessoryColor = traverse.Field(nameof (AccessoryColor)).GetValue<Color>();
//           this.HairLength = traverse.Field(nameof (HairLength)).GetValue<float>();
//         }
//
//         public object Convert()
//         {
//           object instance = Activator.CreateInstance(CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryInfo"]);
//           Traverse traverse = Traverse.Create(instance);
//           traverse.Field<bool>("HairGloss").Value = this.HairGloss;
//           traverse.Field<bool>("ColorMatch").Value = this.ColorMatch;
//           traverse.Field<Color>("OutlineColor").Value = this.OutlineColor;
//           traverse.Field<Color>("AccessoryColor").Value = this.AccessoryColor;
//           traverse.Field<float>("HairLength").Value = this.HairLength;
//           return instance;
//         }
//       }
//     }
//   }
//
//   internal static class MaterialEditorSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _legacy = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//     private static readonly List<string> _containerKeys = new List<string>()
//     {
//       "RendererPropertyList",
//       "MaterialShaderList",
//       "MaterialFloatPropertyList",
//       "MaterialColorPropertyList",
//       "MaterialTexturePropertyList"
//     };
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out pluginInfo);
//       CharacterAccessory.MaterialEditorSupport._instance = pluginInfo.Instance;
//       CharacterAccessory._supportList.Add("MaterialEditor");
//       Assembly assembly = CharacterAccessory.MaterialEditorSupport._instance.GetType().Assembly;
//       CharacterAccessory.MaterialEditorSupport._types["MaterialAPI"] = assembly.GetType("MaterialEditorAPI.MaterialAPI");
//       CharacterAccessory.MaterialEditorSupport._types["MaterialEditorCharaController"] = assembly.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController");
//       CharacterAccessory.MaterialEditorSupport._types["ObjectType"] = assembly.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController+ObjectType");
//       CharacterAccessory.MaterialEditorSupport._legacy = pluginInfo.Metadata.Version.CompareTo(new System.Version("3.0")) < 0;
//       if (CharacterAccessory.MaterialEditorSupport._legacy)
//         CharacterAccessory._logger.LogWarning((object) $"Material Editor version {pluginInfo.Metadata.Version} found, running in legacy mode");
//       else
//         CharacterAccessory.MaterialEditorSupport._containerKeys.Add("MaterialCopyList");
//       CharacterAccessory._hooksInstance["General"].Patch((MethodBase) CharacterAccessory.MaterialEditorSupport._types["MaterialEditorCharaController"].GetMethod("LoadData", AccessTools.all, (System.Reflection.Binder) null, new System.Type[3]
//       {
//         typeof (bool),
//         typeof (bool),
//         typeof (bool)
//       }, (ParameterModifier[]) null), new HarmonyMethod(typeof (CharacterAccessory.Hooks), "DuringLoading_IEnumerator_Prefix"));
//     }
//
//     internal static MaterialEditorCharaController GetController(ChaControl _chaCtrl)
//     {
//       return _chaCtrl?.gameObject?.GetComponent<MaterialEditorCharaController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly MaterialEditorCharaController _pluginCtrl;
//       private readonly Dictionary<string, object> _extdataLink = new Dictionary<string, object>();
//       private readonly Dictionary<string, object> _charaAccData = new Dictionary<string, object>();
//       private Dictionary<int, byte[]> _texData = new Dictionary<int, byte[]>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.MaterialEditorSupport.GetController(this._chaCtrl);
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           string name = "KK_Plugins.MaterialEditor.MaterialEditorCharaController+" + containerKey.Replace("List", "");
//           System.Type type = typeof (List<>).MakeGenericType(CharacterAccessory.MaterialEditorSupport._instance.GetType().Assembly.GetType(name));
//           this._charaAccData[containerKey] = Activator.CreateInstance(type);
//         }
//       }
//
//       internal void Reset()
//       {
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           this._extdataLink[containerKey] = Traverse.Create((object) this._pluginCtrl).Field(containerKey).GetValue();
//           Traverse.Create(this._charaAccData[containerKey]).Method("Clear").GetValue();
//         }
//         this._texData.Clear();
//       }
//
//       internal Dictionary<string, string> Save()
//       {
//         Dictionary<string, string> dictionary = new Dictionary<string, string>();
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//           dictionary[containerKey] = JSONSerializer.Serialize(this._charaAccData[containerKey].GetType(), this._charaAccData[containerKey]);
//         return dictionary;
//       }
//
//       internal void Load(Dictionary<string, string> _json)
//       {
//         this.Reset();
//         if (_json == null)
//           return;
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           if (_json.ContainsKey(containerKey) && _json[containerKey] != null)
//             this._charaAccData[containerKey] = JSONSerializer.Deserialize(this._charaAccData[containerKey].GetType(), _json[containerKey]);
//         }
//       }
//
//       internal void Backup()
//       {
//         this.Reset();
//         CharacterAccessory.CharacterAccessoryController controller = CharacterAccessory.GetController(this._chaCtrl);
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         List<int> list = controller.PartsInfo.Keys.ToList<int>();
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           int num = Traverse.Create(this._extdataLink[containerKey]).Property("Count").GetValue<int>();
//           CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[MaterialEditor][Backup][{this._chaCtrl.GetFullName()}][_extdataLink[{containerKey}] count: {num}]");
//           for (int _key = 0; _key < num; ++_key)
//           {
//             object root = this._extdataLink[containerKey].RefElementAt(_key).JsonClone();
//             Traverse traverse = Traverse.Create(root);
//             if (!(traverse.Field("ObjectType").Method("ToString").GetValue<string>() != "Accessory") && traverse.Field("CoordinateIndex").GetValue<int>() == coordinateType && list.IndexOf(traverse.Field("Slot").GetValue<int>()) >= 0)
//             {
//               traverse.Field("CoordinateIndex").SetValue((object) -1);
//               (this._charaAccData[containerKey] as IList).Add(root);
//             }
//           }
//         }
//         foreach (MaterialEditorCharaController.MaterialTextureProperty materialTextureProperty in this._charaAccData["MaterialTexturePropertyList"] as List<MaterialEditorCharaController.MaterialTextureProperty>)
//         {
//           if (materialTextureProperty.TexID.HasValue)
//           {
//             int key = materialTextureProperty.TexID.Value;
//             if (!this._texData.ContainsKey(key))
//             {
//               TextureContainer textureContainer;
//               this._pluginCtrl.TextureDictionary.TryGetValue(key, out textureContainer);
//               if (textureContainer != null)
//               {
//                 this._texData[key] = textureContainer.Data;
//                 CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, $"[TexID: {key}][Length: {this._texData[key].Length}]");
//               }
//             }
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, int> dictionary = new Dictionary<int, int>();
//         foreach (KeyValuePair<int, byte[]> keyValuePair in this._texData)
//           dictionary[keyValuePair.Key] = this._pluginCtrl.SetAndGetTextureID(keyValuePair.Value);
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           int num = Traverse.Create(this._charaAccData[containerKey]).Property("Count").GetValue<int>();
//           for (int _key = 0; _key < num; ++_key)
//           {
//             object root = this._charaAccData[containerKey].RefElementAt(_key).JsonClone();
//             Traverse traverse = Traverse.Create(root);
//             traverse.Field("CoordinateIndex").SetValue((object) coordinateType);
//             if (containerKey == "MaterialTexturePropertyList")
//             {
//               int? nullable = traverse.Field("TexID").GetValue<int?>();
//               if (nullable.HasValue)
//                 traverse.Field("TexID").SetValue((object) dictionary[nullable.Value]);
//             }
//             (this._extdataLink[containerKey] as IList).Add(root);
//           }
//         }
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         this._pluginCtrl.AccessoriesCopiedEvent((object) null, _args);
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         this.RemovePartsInfo(_args.DestinationSlotIndex);
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         foreach (string containerKey in CharacterAccessory.MaterialEditorSupport._containerKeys)
//         {
//           int num = Traverse.Create(this._extdataLink[containerKey]).Property("Count").GetValue<int>();
//           for (int _key = 0; _key < num; ++_key)
//           {
//             object obj = this.MoveSlot(this._extdataLink[containerKey].RefElementAt(_key).JsonClone(), coordinateType, _args.SourceSlotIndex, _args.DestinationSlotIndex);
//             if (obj != null)
//               (this._extdataLink[containerKey] as IList).Add(obj);
//           }
//         }
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         this._pluginCtrl.AccessoryKindChangeEvent((object) null, new AccessorySlotEventArgs(_slotIndex));
//       }
//
//       internal Dictionary<int, byte[]> TexContainer
//       {
//         get => this._texData;
//         set => this._texData = value;
//       }
//
//       private object MoveSlot(
//         object _obj,
//         int _coordinateIndex,
//         int _srcSlotIndex,
//         int _dstSlotIndex)
//       {
//         if (_obj == null)
//           return (object) null;
//         Traverse traverse = Traverse.Create(_obj);
//         if (traverse.Field("ObjectType").Method("ToString").GetValue<string>() != "Accessory")
//           return (object) null;
//         if (traverse.Field("CoordinateIndex").GetValue<int>() != _coordinateIndex)
//           return (object) null;
//         if (traverse.Field("Slot").GetValue<int>() != _srcSlotIndex)
//           return (object) null;
//         traverse.Field("Slot").SetValue((object) _dstSlotIndex);
//         return _obj;
//       }
//     }
//   }
//
//   internal static class MaterialRouterSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("madevil.kk.mr", out pluginInfo);
//       CharacterAccessory.MaterialRouterSupport._instance = pluginInfo?.Instance;
//       if (!((UnityEngine.Object) CharacterAccessory.MaterialRouterSupport._instance != (UnityEngine.Object) null))
//         return;
//       if (pluginInfo.Metadata.Version.CompareTo(new System.Version("2.0.0.0")) < 0)
//       {
//         CharacterAccessory._logger.LogError((object) $"Material Router version {pluginInfo.Metadata.Version} found, minimun version 2 is reqired");
//       }
//       else
//       {
//         CharacterAccessory.MaterialRouterSupport._installed = true;
//         CharacterAccessory._supportList.Add("MaterialRouter");
//         Assembly assembly = CharacterAccessory.MaterialRouterSupport._instance.GetType().Assembly;
//         CharacterAccessory.MaterialRouterSupport._types["MaterialRouterController"] = assembly.GetType("MaterialRouter.MaterialRouter+MaterialRouterController");
//         CharacterAccessory.MaterialRouterSupport._types["RouteRule"] = assembly.GetType("MaterialRouter.MaterialRouter+RouteRule");
//         CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"] = assembly.GetType("MaterialRouter.MaterialRouter+RouteRuleV1");
//       }
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       if (!CharacterAccessory.MaterialRouterSupport._installed)
//         return (CharaCustomFunctionController) null;
//       return Traverse.Create((object) CharacterAccessory.MaterialRouterSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaCustomFunctionController _pluginCtrl;
//       private readonly List<object> _charaAccData = new List<object>();
//       private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.MaterialRouterSupport.GetController(this._chaCtrl);
//         this._traverses["pluginCtrl"] = Traverse.Create((object) this._pluginCtrl);
//       }
//
//       internal object GetExtDataLink()
//       {
//         return this._traverses["pluginCtrl"].Field("RouteRuleList").GetValue();
//       }
//
//       internal void Reset()
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._charaAccData.Clear();
//       }
//
//       internal List<string> Save()
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return (List<string>) null;
//         List<string> stringList = new List<string>();
//         foreach (object obj in this._charaAccData)
//           stringList.Add(JSONSerializer.Serialize(CharacterAccessory.MaterialRouterSupport._types["RouteRule"], obj));
//         return stringList;
//       }
//
//       internal void Load(List<string> _json)
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._charaAccData?.Clear();
//         if (_json == null)
//           return;
//         bool flag = false;
//         System.Type type = typeof (List<>).MakeGenericType(CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"]);
//         object instance = Activator.CreateInstance(type);
//         foreach (string serializedState in _json)
//         {
//           if (serializedState.IndexOf("GameObjectPath") > -1)
//           {
//             flag = true;
//             (instance as IList).Add(JSONSerializer.Deserialize(CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"], serializedState));
//           }
//           else
//             this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.MaterialRouterSupport._types["RouteRule"], serializedState));
//         }
//         if (!flag)
//           return;
//         CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Warning, "[MaterialRouterSupport][Migration]");
//         foreach (object obj in (IEnumerable) (Traverse.Create((object) CharacterAccessory.MaterialRouterSupport._instance).Method("MigrationV1", new System.Type[1]
//         {
//           type
//         }, new object[1]{ instance }).GetValue() as IList))
//           this._charaAccData.Add(obj);
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._charaAccData.Clear();
//         CharacterAccessory.CharacterAccessoryController controller = CharacterAccessory.GetController(this._chaCtrl);
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = controller.PartsInfo;
//         List<int> intList1;
//         if (partsInfo == null)
//         {
//           intList1 = (List<int>) null;
//         }
//         else
//         {
//           Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
//           intList1 = keys != null ? keys.ToList<int>() : (List<int>) null;
//         }
//         List<int> intList2 = intList1;
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         int count = (extDataLink as IList).Count;
//         for (int _key = 0; _key < count; ++_key)
//         {
//           object root = extDataLink.RefElementAt(_key).JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           if (!(traverse.Property("ObjectType").Method("ToString").GetValue<string>() != "Accessory") && traverse.Property("Coordinate").GetValue<int>() == coordinateType)
//           {
//             int num = int.Parse(traverse.Property("GameObjectName").GetValue<string>().Replace("ca_slot", ""));
//             if (intList2.Contains(num))
//             {
//               traverse.Property("Coordinate").SetValue((object) -1);
//               ((IList) this._charaAccData).Add(root);
//             }
//           }
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         object extDataLink = this.GetExtDataLink();
//         if (extDataLink == null)
//           return;
//         for (int index = 0; index < this._charaAccData.Count; ++index)
//         {
//           object root = this._charaAccData[index].JsonClone();
//           Traverse.Create(root).Property("Coordinate").SetValue((object) this._chaCtrl.fileStatus.coordinateType);
//           (extDataLink as IList).Add(root);
//         }
//       }
//
//       internal string Report()
//       {
//         return !CharacterAccessory.MaterialRouterSupport._installed ? "" : JSONSerializer.Serialize(this._charaAccData.GetType(), (object) this._charaAccData, true);
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("AccessoryCopyEvent", (object) _args).GetValue();
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("TransferAccSlotInfo", (object) this._chaCtrl.fileStatus.coordinateType, (object) _args).GetValue();
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         if (!CharacterAccessory.MaterialRouterSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("RemoveAccSlotInfo", (object) this._chaCtrl.fileStatus.coordinateType, (object) _slotIndex).GetValue();
//       }
//     }
//   }
//
//   internal static class AccStateSyncSupport
//   {
//     internal static BaseUnityPlugin _instance = (BaseUnityPlugin) null;
//     internal static bool _installed = false;
//     internal static bool _legacy = false;
//     internal static readonly Dictionary<string, System.Type> _types = new Dictionary<string, System.Type>();
//     internal static readonly List<string> _containerKeys = new List<string>()
//     {
//       "TriggerPropertyList",
//       "TriggerGroupList"
//     };
//     internal static readonly Dictionary<string, string> _accParentNames = new Dictionary<string, string>();
//     internal static readonly Dictionary<string, int> _guidMapping = new Dictionary<string, int>();
//
//     internal static void Init()
//     {
//       PluginInfo pluginInfo;
//       Chainloader.PluginInfos.TryGetValue("madevil.kk.ass", out pluginInfo);
//       CharacterAccessory.AccStateSyncSupport._instance = pluginInfo?.Instance;
//       if (!((UnityEngine.Object) CharacterAccessory.AccStateSyncSupport._instance != (UnityEngine.Object) null))
//         return;
//       CharacterAccessory.AccStateSyncSupport._legacy = pluginInfo.Metadata.Version.CompareTo(new System.Version("4.0.0.0")) < 0;
//       if (CharacterAccessory.AccStateSyncSupport._legacy)
//       {
//         CharacterAccessory._logger.LogError((object) $"AccStateSync version {pluginInfo.Metadata.Version} found, minimun version 4 is reqired");
//       }
//       else
//       {
//         CharacterAccessory.AccStateSyncSupport._installed = true;
//         CharacterAccessory._supportList.Add("AccStateSync");
//         Assembly assembly = CharacterAccessory.AccStateSyncSupport._instance.GetType().Assembly;
//         CharacterAccessory.AccStateSyncSupport._types["AccStateSyncController"] = assembly.GetType("AccStateSync.AccStateSync+AccStateSyncController");
//         CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"] = assembly.GetType("AccStateSync.AccStateSync+TriggerProperty");
//         CharacterAccessory.AccStateSyncSupport._types["TriggerGroup"] = assembly.GetType("AccStateSync.AccStateSync+TriggerGroup");
//         foreach (object key in Enum.GetValues(typeof (ChaAccessoryDefine.AccessoryParentKey)))
//           CharacterAccessory.AccStateSyncSupport._accParentNames[key.ToString()] = ChaAccessoryDefine.dictAccessoryParent[(int) key];
//       }
//     }
//
//     internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
//     {
//       if (!CharacterAccessory.AccStateSyncSupport._installed)
//         return (CharaCustomFunctionController) null;
//       return Traverse.Create((object) CharacterAccessory.AccStateSyncSupport._instance).Method(nameof (GetController), (object) _chaCtrl).GetValue<CharaCustomFunctionController>();
//     }
//
//     internal class UrineBag
//     {
//       private readonly ChaControl _chaCtrl;
//       private readonly CharaCustomFunctionController _pluginCtrl;
//       private readonly Dictionary<string, object> _charaAccData = new Dictionary<string, object>();
//       private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
//       internal UrineBag(ChaControl ChaControl)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._chaCtrl = ChaControl;
//         this._pluginCtrl = CharacterAccessory.AccStateSyncSupport.GetController(this._chaCtrl);
//         this._traverses["pluginCtrl"] = Traverse.Create((object) this._pluginCtrl);
//         foreach (string containerKey in CharacterAccessory.AccStateSyncSupport._containerKeys)
//         {
//           System.Type type = typeof (List<>).MakeGenericType(CharacterAccessory.AccStateSyncSupport._types[containerKey.Replace("List", "")]);
//           this._charaAccData[containerKey] = Activator.CreateInstance(type);
//           CharacterAccessory.AccStateSyncSupport._types[containerKey] = type;
//         }
//       }
//
//       internal object GetTriggerPropertyList()
//       {
//         return this._traverses["pluginCtrl"].Field("TriggerPropertyList").GetValue();
//       }
//
//       internal object GetTriggerGroupList()
//       {
//         return this._traverses["pluginCtrl"].Field("TriggerGroupList").GetValue();
//       }
//
//       internal void Reset()
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         (this._charaAccData["TriggerPropertyList"] as IList).Clear();
//         (this._charaAccData["TriggerGroupList"] as IList).Clear();
//         CharacterAccessory.AccStateSyncSupport._guidMapping.Clear();
//       }
//
//       internal Dictionary<string, string> Save()
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return (Dictionary<string, string>) null;
//         Dictionary<string, string> dictionary = new Dictionary<string, string>();
//         foreach (string containerKey in CharacterAccessory.AccStateSyncSupport._containerKeys)
//           dictionary[containerKey] = JSONSerializer.Serialize(CharacterAccessory.AccStateSyncSupport._types[containerKey], this._charaAccData[containerKey]);
//         return dictionary;
//       }
//
//       internal void Migrate(Dictionary<int, string> _json)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this.Reset();
//         if (_json == null)
//           return;
//         List<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo> accTriggerInfoList = new List<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo>();
//         int num1 = -1;
//         int num2 = 9;
//         Dictionary<string, int> source = new Dictionary<string, int>();
//         foreach (string serializedState in _json.Values)
//         {
//           CharacterAccessory.AccStateSyncSupport.AccTriggerInfo accTriggerInfo = JSONSerializer.Deserialize<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo>(serializedState);
//           accTriggerInfoList.Add(accTriggerInfo);
//           if (accTriggerInfo.Kind >= 9)
//             source[accTriggerInfo.Group] = accTriggerInfo.Kind;
//         }
//         Dictionary<string, int> dictionary1 = source.OrderBy<KeyValuePair<string, int>, int>((Func<KeyValuePair<string, int>, int>) (x => x.Value)).ThenBy<KeyValuePair<string, int>, string>((Func<KeyValuePair<string, int>, string>) (x => x.Key)).ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (x => x.Key), (Func<KeyValuePair<string, int>, int>) (x => x.Value));
//         Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
//         foreach (string key in dictionary1.Keys)
//         {
//           dictionary2[key] = num2;
//           string accParentName = CharacterAccessory.AccStateSyncSupport._accParentNames.ContainsKey(key) ? CharacterAccessory.AccStateSyncSupport._accParentNames[key] : "";
//           (this._charaAccData["TriggerGroupList"] as IList).Add(Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerGroup"], (object) num1, (object) num2, (object) accParentName));
//           ++num2;
//         }
//         foreach (CharacterAccessory.AccStateSyncSupport.AccTriggerInfo accTriggerInfo in accTriggerInfoList)
//         {
//           if (accTriggerInfo.Kind >= 9)
//           {
//             accTriggerInfo.Kind = dictionary2[accTriggerInfo.Group];
//             (this._charaAccData["TriggerPropertyList"] as IList).Add(Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], (object) num1, (object) accTriggerInfo.Slot, (object) accTriggerInfo.Kind, (object) 0, (object) accTriggerInfo.State[0], (object) 0));
//             (this._charaAccData["TriggerPropertyList"] as IList).Add(Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], (object) num1, (object) accTriggerInfo.Slot, (object) accTriggerInfo.Kind, (object) 1, (object) accTriggerInfo.State[3], (object) 0));
//           }
//           else
//           {
//             for (int index = 0; index <= 3; ++index)
//               (this._charaAccData["TriggerPropertyList"] as IList).Add(Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], (object) num1, (object) accTriggerInfo.Slot, (object) accTriggerInfo.Kind, (object) index, (object) accTriggerInfo.State[index], (object) 0));
//           }
//         }
//         for (int _key = 0; _key < (this._charaAccData["TriggerGroupList"] as IList).Count; ++_key)
//         {
//           Traverse traverse = Traverse.Create(this._charaAccData["TriggerGroupList"].RefElementAt(_key));
//           int num3 = traverse.Property("Kind").GetValue<int>();
//           string key = traverse.Property("GUID").GetValue<string>();
//           CharacterAccessory.AccStateSyncSupport._guidMapping[key] = num3;
//         }
//       }
//
//       internal void Load(Dictionary<string, string> _json)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this.Reset();
//         if (_json == null || !_json.ContainsKey("TriggerPropertyList"))
//           return;
//         this._charaAccData["TriggerPropertyList"] = JSONSerializer.Deserialize(this._charaAccData["TriggerPropertyList"].GetType(), _json["TriggerPropertyList"]);
//         this._charaAccData["TriggerGroupList"] = JSONSerializer.Deserialize(this._charaAccData["TriggerGroupList"].GetType(), _json["TriggerGroupList"]);
//         for (int _key = 0; _key < (this._charaAccData["TriggerGroupList"] as IList).Count; ++_key)
//         {
//           Traverse traverse = Traverse.Create(this._charaAccData["TriggerGroupList"].RefElementAt(_key));
//           int num = traverse.Property("Kind").GetValue<int>();
//           string key = traverse.Property("GUID").GetValue<string>();
//           CharacterAccessory.AccStateSyncSupport._guidMapping[key] = num;
//         }
//       }
//
//       internal void Backup()
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this.Reset();
//         this.RefreshCache();
//         object _self1 = this._traverses["pluginCtrl"].Field("_cachedCoordinatePropertyList").GetValue();
//         object _self2 = this._traverses["pluginCtrl"].Field("_cachedCoordinateGroupList").GetValue();
//         if (_self1 == null)
//           return;
//         HashSet<int> intSet = new HashSet<int>((IEnumerable<int>) CharacterAccessory.GetController(this._chaCtrl).PartsInfo?.Keys);
//         for (int _key = 0; _key < (_self1 as IList).Count; ++_key)
//         {
//           object root = _self1.RefElementAt(_key).JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           if (intSet.Contains(traverse.Property("Slot").GetValue<int>()))
//           {
//             traverse.Property("Coordinate").SetValue((object) -1);
//             (this._charaAccData["TriggerPropertyList"] as IList).Add(root);
//           }
//         }
//         for (int _key = 0; _key < (_self2 as IList).Count; ++_key)
//         {
//           object root = _self2.RefElementAt(_key).JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           traverse.Property("Coordinate").SetValue((object) -1);
//           (this._charaAccData["TriggerGroupList"] as IList).Add(root);
//           int num = traverse.Property("Kind").GetValue<int>();
//           string key = traverse.Property("GUID").GetValue<string>();
//           CharacterAccessory.AccStateSyncSupport._guidMapping[key] = num;
//         }
//       }
//
//       internal void Restore()
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         object obj1 = this._traverses["pluginCtrl"].Field("TriggerPropertyList").GetValue();
//         object obj2 = this._traverses["pluginCtrl"].Field("TriggerGroupList").GetValue();
//         if (obj1 == null)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         Dictionary<int, int> dictionary = new Dictionary<int, int>();
//         foreach (string key1 in CharacterAccessory.AccStateSyncSupport._guidMapping.Keys)
//         {
//           object root = (object) null;
//           foreach (object obj3 in (IEnumerable) (this._charaAccData["TriggerGroupList"] as IList))
//           {
//             if (!(Traverse.Create(obj3).Property("GUID").GetValue<string>() != key1))
//               root = obj3.JsonClone();
//           }
//           if (root == null)
//           {
//             CharacterAccessory.DebugMsg(BepInEx.Logging.LogLevel.Error, "[Restore] cannot find group setting for " + key1);
//           }
//           else
//           {
//             Traverse traverse = Traverse.Create(root);
//             traverse.Property("Coordinate").SetValue((object) coordinateType);
//             object triggerGroupByGuid = this.GetTriggerGroupByGUID(coordinateType, key1);
//             int key2 = CharacterAccessory.AccStateSyncSupport._guidMapping[key1];
//             if (triggerGroupByGuid == null)
//             {
//               int nextGroupId = this.GetNextGroupID(coordinateType);
//               dictionary[key2] = nextGroupId;
//               traverse.Property("Kind").SetValue((object) nextGroupId);
//             }
//             (obj2 as IList).Add(root);
//           }
//         }
//         foreach (object _self in (IEnumerable) (this._charaAccData["TriggerPropertyList"] as IList))
//         {
//           object root = _self.JsonClone();
//           Traverse traverse = Traverse.Create(root);
//           traverse.Property("Coordinate").SetValue((object) coordinateType);
//           int key = traverse.Property("RefKind").GetValue<int>();
//           if (dictionary.ContainsKey(key))
//             traverse.Property("RefKind").SetValue((object) dictionary[key]);
//           (obj1 as IList).Add(root);
//         }
//         this.RefreshCache();
//       }
//
//       internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         foreach (int copiedSlotIndex in _args.CopiedSlotIndexes)
//           this._traverses["pluginCtrl"].Method("CloneSlotTriggerProperty", (object) copiedSlotIndex, (object) copiedSlotIndex, (object) (int) _args.CopySource, (object) (int) _args.CopyDestination).GetValue();
//       }
//
//       internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         int coordinateType = this._chaCtrl.fileStatus.coordinateType;
//         this._traverses["pluginCtrl"].Method("CloneSlotTriggerProperty", (object) _args.SourceSlotIndex, (object) _args.DestinationSlotIndex, (object) coordinateType, (object) coordinateType).GetValue();
//       }
//
//       internal void RemovePartsInfo(int _slotIndex)
//       {
//         this.RemovePartsInfo(this._chaCtrl.fileStatus.coordinateType, _slotIndex);
//       }
//
//       internal void RemovePartsInfo(int _coordinateIndex, int _slotIndex)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method("RemoveSlotTriggerProperty", (object) _coordinateIndex, (object) _slotIndex).GetValue();
//       }
//
//       internal object GetTriggerGroupByGUID(int _coordinateIndex, string _guid)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return (object) -1;
//         return this._traverses["pluginCtrl"].Method(nameof (GetTriggerGroupByGUID), (object) _coordinateIndex, (object) _guid).GetValue();
//       }
//
//       internal int GetNextGroupID(int _coordinateIndex)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return -1;
//         return this._traverses["pluginCtrl"].Method(nameof (GetNextGroupID), (object) _coordinateIndex).GetValue<int>();
//       }
//
//       internal void PackGroupID(int _coordinateIndex)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (PackGroupID), (object) _coordinateIndex).GetValue();
//       }
//
//       internal void RefreshCache()
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (RefreshCache)).GetValue();
//       }
//
//       internal void InitCurOutfitTriggerInfo(string _caller)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (InitCurOutfitTriggerInfo), (object) _caller).GetValue();
//       }
//
//       internal void SetAccessoryStateAll(bool _show = true)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (SetAccessoryStateAll), (object) _show).GetValue();
//       }
//
//       internal void SyncAllAccToggle(string _caller)
//       {
//         if (!CharacterAccessory.AccStateSyncSupport._installed)
//           return;
//         this._traverses["pluginCtrl"].Method(nameof (SyncAllAccToggle), (object) _caller).GetValue();
//       }
//     }
//
//     public class AccTriggerInfo
//     {
//       public int Slot { get; set; }
//
//       public int Kind { get; set; } = -1;
//
//       public string Group { get; set; } = "";
//
//       public List<bool> State { get; set; } = new List<bool>()
//       {
//         true,
//         false,
//         false,
//         false
//       };
//
//       public AccTriggerInfo(int slot) => this.Slot = slot;
//     }
//   }
// }
