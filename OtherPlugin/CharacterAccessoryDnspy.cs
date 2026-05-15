// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Runtime.CompilerServices;
// using BepInEx;
// using BepInEx.Bootstrap;
// using BepInEx.Configuration;
// using BepInEx.Logging;
// using ChaCustom;
// using ExtensibleSaveFormat;
// using HarmonyLib;
// using JetPack;
// using KKAPI;
// using KKAPI.Chara;
// using KKAPI.Maker;
// using KKAPI.Maker.UI;
// using KKAPI.Maker.UI.Sidebar;
// using KKAPI.Studio;
// using KKAPI.Studio.UI;
// using KKAPI.Utilities;
// using KK_Plugins;
// using KK_Plugins.DynamicBoneEditor;
// using KK_Plugins.MaterialEditor;
// using MessagePack;
// using ParadoxNotion.Serialization;
// using Sideloader;
// using Sideloader.AutoResolver;
// using Studio;
// using TMPro;
// using UniRx;
// using UnityEngine;
// using UnityEngine.UI;
// using ObservableExtensions = UniRx.ObservableExtensions;
//
// namespace CharacterAccessory
// {
// 	[BepInPlugin("madevil.kk.ca", "Character Accessory", "1.8.2.0")]
// 	[BepInDependency("madevil.JetPack", "2.1.4.0")]
// 	[BepInDependency("marco.kkapi", "1.26")]
// 	[BepInDependency("com.bepis.bepinex.extendedsave", "16.8.1")]
// 	[BepInDependency("com.deathweasel.bepinex.materialeditor", "3.1.2")]
// 	[BepInIncompatibility("KK_ClothesLoadOption")]
// 	[BepInIncompatibility("com.jim60105.kk.studiocoordinateloadoption")]
// 	[BepInIncompatibility("com.jim60105.kk.coordinateloadoption")]
// 	public class CharacterAccessory : BaseUnityPlugin
// 	{
// 		public static CharacterAccessoryController GetController(ChaControl ChaControl)
// 		{
// 			if (ChaControl == null)
// 			{
// 				return null;
// 			}
// 			GameObject gameObject = ChaControl.gameObject;
// 			if (gameObject == null)
// 			{
// 				return null;
// 			}
// 			return gameObject.GetComponent<CharacterAccessoryController>();
// 		}
//
// 		// Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00000268
// 		public static CharacterAccessoryController GetController(OCIChar OCIChar)
// 		{
// 			return CharacterAccessory.GetController((OCIChar != null) ? OCIChar.charInfo : null);
// 		}
//
// 		// Token: 0x06000003 RID: 3 RVA: 0x0000207C File Offset: 0x0000027C
// 		private void RegisterCustomSubCategories(object _sender, RegisterSubCategoriesEvent _args)
// 		{
// 			ChaControl _chaCtrl = Singleton<CustomBase>.Instance.chaCtrl;
// 			CharacterAccessoryController _pluginCtrl = CharacterAccessory.GetController(_chaCtrl);
// 			CharacterAccessory._sidebarToggleEnable = _args.AddSidebarControl<SidebarToggle>(new SidebarToggle("CharaAcc", CharacterAccessory._cfgMakerMasterSwitch.Value, this));
// 			ObservableExtensions.Subscribe<bool>(CharacterAccessory._sidebarToggleEnable.ValueChanged, delegate(bool _value)
// 			{
// 				CharacterAccessory._cfgMakerMasterSwitch.Value = _value;
// 			});
// 			MakerCategory category = new MakerCategory("03_ClothesTop", "tglCharaAcc", MakerConstants.Clothes.Copy.Position + 1, "CharaAcc");
// 			_args.AddSubCategory(category);
// 			_args.AddControl<MakerText>(new MakerText("The set to be used as a template to clone on load", category, this));
// 			List<string> list = CharacterAccessory._cordNames.ToList<string>();
// 			list.Add("CharaAcc");
// 			CharacterAccessory._makerDropdownReferral = new MakerDropdown("Referral", list.ToArray(), category, 7, this);
// 			ObservableExtensions.Subscribe<int>(CharacterAccessory._makerDropdownReferral.ValueChanged, delegate(int _value)
// 			{
// 				_pluginCtrl.SetReferralIndex(_value);
// 			});
// 			_args.AddControl<MakerDropdown>(CharacterAccessory._makerDropdownReferral);
// 			CharacterAccessory._makerToggleEnable = _args.AddControl<MakerToggle>(new MakerToggle(category, "Enable", false, this));
// 			ObservableExtensions.Subscribe<bool>(CharacterAccessory._makerToggleEnable.ValueChanged, delegate(bool _value)
// 			{
// 				_pluginCtrl.FunctionEnable = _value;
// 			});
// 			CharacterAccessory._makerToggleAutoCopyToBlank = _args.AddControl<MakerToggle>(new MakerToggle(category, "Auto Copy To Blank", false, this));
// 			ObservableExtensions.Subscribe<bool>(CharacterAccessory._makerToggleAutoCopyToBlank.ValueChanged, delegate(bool _value)
// 			{
// 				_pluginCtrl.AutoCopyToBlank = _value;
// 			});
// 			_args.AddControl<MakerButton>(new MakerButton("Backup", category, this)).OnClick.AddListener(delegate()
// 			{
// 				if (_pluginCtrl.DuringLoading)
// 				{
// 					return;
// 				}
// 				_pluginCtrl.Backup();
// 				_pluginCtrl.SetReferralIndex(-1);
// 				_pluginCtrl.FunctionEnable = true;
// 				CharacterAccessory._makerToggleEnable.Value = _pluginCtrl.FunctionEnable;
// 			});
// 			_args.AddControl<MakerButton>(new MakerButton("Restore", category, this)).OnClick.AddListener(delegate()
// 			{
// 				if (_pluginCtrl.DuringLoading)
// 				{
// 					return;
// 				}
// 				if (CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(_chaCtrl, _chaCtrl.fileStatus.coordinateType).Count > 0)
// 				{
// 					CharacterAccessory._logger.LogMessage("Please clear the accessories on current coordinate before using this function");
// 					return;
// 				}
// 				_pluginCtrl.TaskLock();
// 				_pluginCtrl.RestorePartsInfo();
// 			});
// 			_args.AddControl<MakerButton>(new MakerButton("Reset", category, this)).OnClick.AddListener(delegate()
// 			{
// 				if (_pluginCtrl.DuringLoading)
// 				{
// 					return;
// 				}
// 				_pluginCtrl.Reset();
// 				_pluginCtrl.SetReferralIndex(-1);
// 				CharacterAccessory._makerToggleEnable.Value = _pluginCtrl.FunctionEnable;
// 				CharacterAccessory._makerToggleAutoCopyToBlank.Value = _pluginCtrl.AutoCopyToBlank;
// 			});
// 			if (Game.ConsoleActive)
// 			{
// 				_args.AddControl<MakerSeparator>(new MakerSeparator(category, this));
// 				_args.AddControl<MakerButton>(new MakerButton("MaterialRouter", category, this)).OnClick.AddListener(delegate()
// 				{
// 					CharacterAccessory._logger.LogInfo("[MaterialRouter]\n" + _pluginCtrl.MaterialRouter.Report());
// 				});
// 			}
// 		}
//
// 		// Token: 0x17000001 RID: 1
// 		// (get) Token: 0x06000004 RID: 4 RVA: 0x000022AD File Offset: 0x000004AD
// 		// (set) Token: 0x06000005 RID: 5 RVA: 0x000022B4 File Offset: 0x000004B4
// 		internal static ConfigEntry<bool> _cfgMakerMasterSwitch { get; set; }
//
// 		// Token: 0x17000002 RID: 2
// 		// (get) Token: 0x06000006 RID: 6 RVA: 0x000022BC File Offset: 0x000004BC
// 		// (set) Token: 0x06000007 RID: 7 RVA: 0x000022C3 File Offset: 0x000004C3
// 		internal static ConfigEntry<bool> _cfgDebugMode { get; set; }
//
// 		// Token: 0x17000003 RID: 3
// 		// (get) Token: 0x06000008 RID: 8 RVA: 0x000022CB File Offset: 0x000004CB
// 		// (set) Token: 0x06000009 RID: 9 RVA: 0x000022D2 File Offset: 0x000004D2
// 		internal static ConfigEntry<bool> _cfgStudioFallbackReload { get; set; }
//
// 		// Token: 0x17000004 RID: 4
// 		// (get) Token: 0x0600000A RID: 10 RVA: 0x000022DA File Offset: 0x000004DA
// 		// (set) Token: 0x0600000B RID: 11 RVA: 0x000022E1 File Offset: 0x000004E1
// 		internal static ConfigEntry<bool> _cfgMAHookUpdateStudioUI { get; set; }
//
// 		// Token: 0x0600000C RID: 12 RVA: 0x000022EC File Offset: 0x000004EC
// 		private void Awake()
// 		{
// 			CharacterAccessory._logger = base.Logger;
// 			CharacterAccessory._instance = this;
// 			CharacterAccessory._cfgMakerMasterSwitch = base.Config.Bind<bool>("Maker", "Master Switch", true, new ConfigDescription("A quick switch on the sidebar that templary disable the function", null, new object[]
// 			{
// 				new ConfigurationManagerAttributes
// 				{
// 					IsAdvanced = new bool?(true)
// 				}
// 			}));
// 			CharacterAccessory._cfgStudioFallbackReload = base.Config.Bind<bool>("Studio", "Fallback Reload Mode", false, new ConfigDescription("Enable this if some plugins are having visual problem", null, new object[]
// 			{
// 				new ConfigurationManagerAttributes
// 				{
// 					IsAdvanced = new bool?(true)
// 				}
// 			}));
// 			CharacterAccessory._cfgDebugMode = base.Config.Bind<bool>("Debug", "Debug Mode", false, new ConfigDescription("Showing debug messages in LogWarning level", null, new object[]
// 			{
// 				new ConfigurationManagerAttributes
// 				{
// 					IsAdvanced = new bool?(true)
// 				}
// 			}));
// 			CharacterAccessory._cfgMAHookUpdateStudioUI = base.Config.Bind<bool>("Hook", "MoreAccessories UpdateStudioUI", true, new ConfigDescription("Performance tweak, disable it if having issue on studio chara state panel update", null, new object[]
// 			{
// 				new ConfigurationManagerAttributes
// 				{
// 					IsAdvanced = new bool?(true)
// 				}
// 			}));
// 		}
//
// 		// Token: 0x0600000D RID: 13 RVA: 0x0000240C File Offset: 0x0000060C
// 		private void Start()
// 		{
// 			CharacterAccessory._cordNames = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList<string>();
// 			CharacterApi.RegisterExtraBehaviour<CharacterAccessoryController>("madevil.kk.ca");
// 			CharacterAccessory._hooksInstance["General"] = Harmony.CreateAndPatchAll(typeof(CharacterAccessory.Hooks), null);
// 			CharacterAccessory.MoreAccessoriesSupport.Init();
// 			bool installed = CharacterAccessory.MoreAccessoriesSupport._installed;
// 			CharacterAccessory.MoreOutfitsSupport.Init();
// 			CharacterAccessory.HairAccessoryCustomizerSupport.Init();
// 			CharacterAccessory.MaterialEditorSupport.Init();
// 			CharacterAccessory.MaterialRouterSupport.Init();
// 			CharacterAccessory.AccStateSyncSupport.Init();
// 			CharacterAccessory.DynamicBoneEditorSupport.Init();
// 			CharacterAccessory.AAAPKSupport.Init();
// 			CharacterAccessory.BendUrAccSupport.Init();
// 			CharacterAccessory.CumOnOverSupport.Init();
// 			CharacterAccessory.BonerStateSync.Init();
// 			if (CharaStudio.Running)
// 			{
// 				CharaStudio.OnStudioLoaded += delegate(object _sender, EventArgs _args)
// 				{
// 					CharacterAccessory.RegisterStudioControls();
// 				};
// 				return;
// 			}
// 			MakerAPI.MakerBaseLoaded += delegate(object _sender, RegisterCustomControlsEvent _args)
// 			{
// 				CharacterAccessory._hooksInstance["Maker"] = Harmony.CreateAndPatchAll(typeof(CharacterAccessory.HooksMaker), null);
// 				CharacterAccessory.MoreOutfitsSupport.MakerInit();
// 				BaseUnityPlugin pluginInstance = Toolbox.GetPluginInstance("ClothingStateMenu");
// 				if (pluginInstance != null)
// 				{
// 					CharacterAccessory._hooksInstance["Maker"].Patch(pluginInstance.GetType().GetMethod("OnGUI", AccessTools.all), new HarmonyMethod(typeof(CharacterAccessory.HooksMaker), "DuringLoading_Prefix", null), null, null, null, null);
// 				}
// 			};
// 			MakerAPI.MakerExiting += delegate(object _sender, EventArgs _args)
// 			{
// 				CharacterAccessory._hooksInstance["Maker"].UnpatchAll(CharacterAccessory._hooksInstance["Maker"].Id);
// 				CharacterAccessory._hooksInstance["Maker"] = null;
// 				CharacterAccessory._makerDropdownReferral = null;
// 				CharacterAccessory._makerToggleEnable = null;
// 				CharacterAccessory._makerToggleAutoCopyToBlank = null;
// 				CharacterAccessory._sidebarToggleEnable = null;
// 			};
// 			MakerAPI.RegisterCustomSubCategories += this.RegisterCustomSubCategories;
// 		}
//
// 		// Token: 0x0600000E RID: 14 RVA: 0x0000251D File Offset: 0x0000071D
// 		internal static void DebugMsg(LogLevel LogLevel, string LogMsg)
// 		{
// 			if (CharacterAccessory._cfgDebugMode.Value)
// 			{
// 				CharacterAccessory._logger.Log(LogLevel, LogMsg);
// 				return;
// 			}
// 			CharacterAccessory._logger.Log(32, LogMsg);
// 		}
//
// 		// Token: 0x0600000F RID: 15 RVA: 0x00002548 File Offset: 0x00000748
// 		internal static void RegisterStudioControls()
// 		{
// 			CurrentStateCategorySwitch currentStateCategorySwitch = new CurrentStateCategorySwitch("Enable", delegate(OCIChar OCIChar)
// 			{
// 				CharacterAccessoryController controller = CharacterAccessory.GetController(OCIChar);
// 				return ((controller != null) ? new bool?(controller.FunctionEnable) : null).Value;
// 			});
// 			ObservableExtensions.Subscribe<bool>(currentStateCategorySwitch.Value, delegate(bool _value)
// 			{
// 				CharacterAccessoryController characterAccessoryController = StudioAPI.GetSelectedControllers<CharacterAccessoryController>().FirstOrDefault<CharacterAccessoryController>();
// 				if (characterAccessoryController == null)
// 				{
// 					return;
// 				}
// 				characterAccessoryController.FunctionEnable = _value;
// 			});
// 			StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategorySwitch>(currentStateCategorySwitch);
// 			CurrentStateCategorySwitch currentStateCategorySwitch2 = new CurrentStateCategorySwitch("Copy To Blank", delegate(OCIChar OCIChar)
// 			{
// 				CharacterAccessoryController controller = CharacterAccessory.GetController(OCIChar);
// 				return ((controller != null) ? new bool?(controller.AutoCopyToBlank) : null).Value;
// 			});
// 			ObservableExtensions.Subscribe<bool>(currentStateCategorySwitch2.Value, delegate(bool _value)
// 			{
// 				CharacterAccessoryController characterAccessoryController = StudioAPI.GetSelectedControllers<CharacterAccessoryController>().FirstOrDefault<CharacterAccessoryController>();
// 				if (characterAccessoryController == null)
// 				{
// 					return;
// 				}
// 				characterAccessoryController.AutoCopyToBlank = _value;
// 			});
// 			StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategorySwitch>(currentStateCategorySwitch2);
// 			List<string> list = Enum.GetNames(typeof(ChaFileDefine.CoordinateType)).ToList<string>();
// 			list.Add("CharaAcc");
// 			CurrentStateCategoryDropdown currentStateCategoryDropdown = new CurrentStateCategoryDropdown("Referral", list.ToArray(), delegate(OCIChar OCIChar)
// 			{
// 				CharacterAccessoryController controller = CharacterAccessory.GetController(OCIChar);
// 				return ((controller != null) ? new int?(controller.GetReferralIndex()) : null).Value;
// 			});
// 			ObservableExtensions.Subscribe<int>(currentStateCategoryDropdown.Value, delegate(int _value)
// 			{
// 				CharacterAccessoryController characterAccessoryController = StudioAPI.GetSelectedControllers<CharacterAccessoryController>().FirstOrDefault<CharacterAccessoryController>();
// 				if (characterAccessoryController == null)
// 				{
// 					return;
// 				}
// 				characterAccessoryController.SetReferralIndex(_value);
// 			});
// 			StudioAPI.GetOrCreateCurrentStateCategory("CharaAcc").AddControl<CurrentStateCategoryDropdown>(currentStateCategoryDropdown);
// 		}
//
// 		// Token: 0x04000001 RID: 1
// 		internal static MakerDropdown _makerDropdownReferral;
//
// 		// Token: 0x04000002 RID: 2
// 		internal static MakerToggle _makerToggleEnable;
//
// 		// Token: 0x04000003 RID: 3
// 		internal static MakerToggle _makerToggleAutoCopyToBlank;
//
// 		// Token: 0x04000004 RID: 4
// 		internal static SidebarToggle _sidebarToggleEnable;
//
// 		// Token: 0x04000005 RID: 5
// 		public const string GUID = "madevil.kk.ca";
//
// 		// Token: 0x04000006 RID: 6
// 		public const string Name = "Character Accessory";
//
// 		// Token: 0x04000007 RID: 7
// 		public const string Version = "1.8.2.0";
//
// 		// Token: 0x04000008 RID: 8
// 		internal static ManualLogSource _logger;
//
// 		// Token: 0x04000009 RID: 9
// 		internal static CharacterAccessory _instance;
//
// 		// Token: 0x0400000A RID: 10
// 		internal static Dictionary<string, Harmony> _hooksInstance = new Dictionary<string, Harmony>();
//
// 		// Token: 0x0400000F RID: 15
// 		internal const int PluginDataVersion = 3;
//
// 		// Token: 0x04000010 RID: 16
// 		internal static List<string> _supportList = new List<string>();
//
// 		// Token: 0x04000011 RID: 17
// 		internal static List<string> _cordNames = new List<string>();
//
// 		// Token: 0x02000003 RID: 3
// 		public class CharacterAccessoryController : CharaCustomFunctionController
// 		{
// 			// Token: 0x17000005 RID: 5
// 			// (get) Token: 0x06000012 RID: 18 RVA: 0x000026D5 File Offset: 0x000008D5
// 			internal int CurrentCoordinateIndex
// 			{
// 				get
// 				{
// 					return base.ChaControl.fileStatus.coordinateType;
// 				}
// 			}
//
// 			// Token: 0x06000013 RID: 19 RVA: 0x000026E8 File Offset: 0x000008E8
// 			protected override void Start()
// 			{
// 				if (KoikatuAPI.GetCurrentGameMode() == GameMode.MainGame)
// 				{
// 					return;
// 				}
// 				this.HairAccessoryCustomizer = new CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag(base.ChaControl);
// 				this.MaterialEditor = new CharacterAccessory.MaterialEditorSupport.UrineBag(base.ChaControl);
// 				this.MaterialRouter = new CharacterAccessory.MaterialRouterSupport.UrineBag(base.ChaControl);
// 				this.AccStateSync = new CharacterAccessory.AccStateSyncSupport.UrineBag(base.ChaControl);
// 				this.DynamicBoneEditor = new CharacterAccessory.DynamicBoneEditorSupport.UrineBag(base.ChaControl);
// 				this.AAAPK = new CharacterAccessory.AAAPKSupport.UrineBag(base.ChaControl);
// 				this.BendUrAcc = new CharacterAccessory.BendUrAccSupport.UrineBag(base.ChaControl);
// 				ObservableExtensions.Subscribe<ChaFileDefine.CoordinateType>(base.CurrentCoordinate, delegate(ChaFileDefine.CoordinateType value)
// 				{
// 					this.OnCoordinateChanged();
// 				});
// 				base.Start();
// 			}
//
// 			// Token: 0x06000014 RID: 20 RVA: 0x00002793 File Offset: 0x00000993
// 			private void OnCoordinateChanged()
// 			{
// 				this.TaskUnlock();
// 				this.AutoCopyCheck();
// 			}
//
// 			// Token: 0x06000015 RID: 21 RVA: 0x000027A4 File Offset: 0x000009A4
// 			protected override void OnCardBeingSaved(GameMode currentGameMode)
// 			{
// 				this.TaskUnlock();
// 				ExtensibleSaveFormat.PluginData pluginData = new ExtensibleSaveFormat.PluginData
// 				{
// 					version = 3
// 				};
// 				pluginData.data.Add("MoreAccessoriesExtdata", MessagePackSerializer.Serialize<Dictionary<int, ChaFileAccessory.PartsInfo>>(this.PartsInfo));
// 				pluginData.data.Add("ResolutionInfoExtdata", MessagePackSerializer.Serialize<Dictionary<int, ResolveInfo>>(this.PartsResolveInfo));
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					object value = Traverse.Create(this).Field(text).Method("Save", Array.Empty<object>()).GetValue();
// 					pluginData.data.Add(text + "Extdata", MessagePackSerializer.Serialize<object>(value));
// 				}
// 				pluginData.data.Add("FunctionEnable", this.FunctionEnable);
// 				pluginData.data.Add("AutoCopyToBlank", this.AutoCopyToBlank);
// 				pluginData.data.Add("ReferralIndex", this.ReferralIndex);
// 				pluginData.data.Add("TextureContainer", MessagePackSerializer.Serialize<Dictionary<int, byte[]>>(this.MaterialEditor.TexContainer));
// 				base.SetExtendedData(pluginData);
// 			}
//
// 			// Token: 0x06000016 RID: 22 RVA: 0x000028F0 File Offset: 0x00000AF0
// 			protected override void OnReload(GameMode currentGameMode)
// 			{
// 				this.TaskUnlock();
// 				ExtensibleSaveFormat.PluginData extendedData = base.GetExtendedData();
// 				this.PartsInfo.Clear();
// 				this.PartsResolveInfo.Clear();
// 				this.FunctionEnable = false;
// 				this.AutoCopyToBlank = false;
// 				this.ReferralIndex = -1;
// 				this.MaterialEditor.Reset();
// 				if (extendedData != null)
// 				{
// 					if (extendedData.version > 3)
// 					{
// 						CharacterAccessory._logger.Log(10, string.Format("[OnReload] ExtendedData.version: {0} is newer than your plugin", extendedData.version));
// 						base.OnReload(currentGameMode);
// 						return;
// 					}
// 					if (extendedData.version < 3)
// 					{
// 						CharacterAccessory._logger.Log(16, string.Format("[OnReload] Migrating from ver. {0}", extendedData.version));
// 					}
// 					object obj;
// 					if (extendedData.data.TryGetValue("MoreAccessoriesExtdata", out obj) && obj != null)
// 					{
// 						this.PartsInfo = MessagePackSerializer.Deserialize<Dictionary<int, ChaFileAccessory.PartsInfo>>((byte[])obj);
// 					}
// 					object obj2;
// 					if (extendedData.data.TryGetValue("ResolutionInfoExtdata", out obj2) && obj2 != null)
// 					{
// 						this.PartsResolveInfo = MessagePackSerializer.Deserialize<Dictionary<int, ResolveInfo>>((byte[])obj2);
// 					}
// 					foreach (string text in CharacterAccessory._supportList)
// 					{
// 						object obj3;
// 						if (extendedData.data.TryGetValue(text + "Extdata", out obj3) && obj3 != null)
// 						{
// 							if (text == "HairAccessoryCustomizer")
// 							{
// 								Traverse.Create(this).Field(text).Method("Load", new object[]
// 								{
// 									MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])obj3)
// 								}).GetValue();
// 							}
// 							else if (text == "AccStateSync")
// 							{
// 								if (extendedData.version < 2)
// 								{
// 									Traverse.Create(this).Field(text).Method("Migrate", new object[]
// 									{
// 										MessagePackSerializer.Deserialize<Dictionary<int, string>>((byte[])obj3)
// 									}).GetValue();
// 								}
// 								else
// 								{
// 									Traverse.Create(this).Field(text).Method("Load", new object[]
// 									{
// 										MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])obj3)
// 									}).GetValue();
// 								}
// 							}
// 							else if (text == "MaterialEditor")
// 							{
// 								Traverse.Create(this).Field(text).Method("Load", new object[]
// 								{
// 									MessagePackSerializer.Deserialize<Dictionary<string, string>>((byte[])obj3)
// 								}).GetValue();
// 							}
// 							else if (text == "MaterialRouter" || text == "DynamicBoneEditor" || text == "AAAPK" || text == "BendUrAcc")
// 							{
// 								Traverse.Create(this).Field(text).Method("Load", new object[]
// 								{
// 									MessagePackSerializer.Deserialize<List<string>>((byte[])obj3)
// 								}).GetValue();
// 							}
// 						}
// 					}
// 					object obj4;
// 					if (extendedData.data.TryGetValue("FunctionEnable", out obj4) && obj4 != null)
// 					{
// 						this.FunctionEnable = (bool)obj4;
// 					}
// 					object obj5;
// 					if (extendedData.data.TryGetValue("AutoCopyToBlank", out obj5) && obj5 != null)
// 					{
// 						this.AutoCopyToBlank = (bool)obj5;
// 					}
// 					object obj6;
// 					if (extendedData.data.TryGetValue("ReferralIndex", out obj6) && obj6 != null)
// 					{
// 						if (extendedData.version < 3)
// 						{
// 							this.SetReferralIndex(-1);
// 						}
// 						else
// 						{
// 							this.SetReferralIndex((int)obj6);
// 						}
// 						CharacterAccessory.DebugMsg(16, string.Format("[OnReload][{0}][ReferralIndex: {1}]", JetPack.Extensions.GetFullName(base.ChaControl), this.ReferralIndex));
// 					}
// 					object obj7;
// 					if (extendedData.data.TryGetValue("TextureContainer", out obj7) && obj7 != null)
// 					{
// 						this.MaterialEditor.TexContainer = MessagePackSerializer.Deserialize<Dictionary<int, byte[]>>((byte[])obj7);
// 					}
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = this.PartsInfo;
// 					if (partsInfo != null && partsInfo.Count > 0)
// 					{
// 						Dictionary<int, ResolveInfo> partsResolveInfo = this.PartsResolveInfo;
// 						if (partsResolveInfo != null && partsResolveInfo.Count > 0)
// 						{
// 							foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> keyValuePair in this.PartsInfo)
// 							{
// 								ResolveInfo resolveInfo;
// 								this.PartsResolveInfo.TryGetValue(keyValuePair.Key, out resolveInfo);
// 								if (resolveInfo != null)
// 								{
// 									CharacterAccessoryController.MigrateData(ref resolveInfo);
// 									if (resolveInfo != null && !resolveInfo.GUID.IsNullOrWhiteSpace())
// 									{
// 										resolveInfo = UniversalAutoResolver.TryGetResolutionInfo(this.PartsResolveInfo[keyValuePair.Key].Slot, this.PartsResolveInfo[keyValuePair.Key].CategoryNo, this.PartsResolveInfo[keyValuePair.Key].GUID);
// 										if (resolveInfo != null)
// 										{
// 											this.PartsResolveInfo[keyValuePair.Key] = (Toolbox.JsonClone(resolveInfo) as ResolveInfo);
// 											keyValuePair.Value.id = resolveInfo.LocalSlot;
// 										}
// 									}
// 								}
// 							}
// 						}
// 					}
// 				}
// 				if (MakerAPI.InsideAndLoaded)
// 				{
// 					CharacterAccessory._makerToggleEnable.Value = this.FunctionEnable;
// 					CharacterAccessory._makerToggleAutoCopyToBlank.Value = this.AutoCopyToBlank;
// 					CharacterAccessory.MoreOutfitsSupport.BuildMakerDropdownRef();
// 				}
// 				if (CharaStudio.Running)
// 				{
// 					OCIChar curOCIChar = CharaStudio.CurOCIChar;
// 					if (((curOCIChar != null) ? curOCIChar.charInfo : null) == base.ChaControl)
// 					{
// 						CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
// 					}
// 				}
// 				base.ChaControl.StartCoroutine(this.<OnReload>g__OnReloadCoroutine|18_0());
// 				base.OnReload(currentGameMode);
// 			}
//
// 			// Token: 0x06000017 RID: 23 RVA: 0x00002E64 File Offset: 0x00001064
// 			protected override void OnCoordinateBeingLoaded(ChaFileCoordinate _coordinate)
// 			{
// 				this.TaskUnlock();
// 				bool flag = true;
// 				CharacterAccessory.DebugMsg(4, string.Format("[OnCoordinateBeingLoaded][{0}][FunctionEnable: {1}][ReferralIndex: {2}][PartsInfo.Count: {3}]", new object[]
// 				{
// 					JetPack.Extensions.GetFullName(base.ChaControl),
// 					this.FunctionEnable,
// 					this.ReferralIndex,
// 					this.PartsInfo.Count
// 				}));
// 				if (!this.FunctionEnable)
// 				{
// 					flag = false;
// 				}
// 				if (this.ReferralIndex == -1 && this.PartsInfo.Count == 0)
// 				{
// 					flag = false;
// 				}
// 				if (MakerAPI.InsideAndLoaded && !CharacterAccessory._cfgMakerMasterSwitch.Value)
// 				{
// 					flag = false;
// 				}
// 				CoordinateLoadFlags coordinateLoadFlags = MakerAPI.GetCoordinateLoadFlags();
// 				if (MakerAPI.InsideAndLoaded && coordinateLoadFlags != null && !coordinateLoadFlags.Accessories)
// 				{
// 					flag = false;
// 				}
// 				if (flag)
// 				{
// 					this.TaskLock();
// 					base.ChaControl.StartCoroutine(this.OnCoordinateBeingLoadedCoroutine());
// 				}
// 				else if (MakerAPI.InsideAndLoaded)
// 				{
// 					Singleton<CustomBase>.Instance.updateCustomUI = true;
// 				}
// 				base.OnCoordinateBeingLoaded(_coordinate);
// 			}
//
// 			// Token: 0x06000018 RID: 24 RVA: 0x00002F53 File Offset: 0x00001153
// 			internal IEnumerator OnCoordinateBeingLoadedCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[OnCoordinateBeingLoadedCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.TaskLock();
// 				this.PrepareQueue();
// 				yield break;
// 			}
//
// 			// Token: 0x06000019 RID: 25 RVA: 0x00002F62 File Offset: 0x00001162
// 			internal IEnumerator RefreshCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RefreshCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.TaskUnlock();
// 				if (CharaStudio.Running)
// 				{
// 					if (CharacterAccessory._cfgStudioFallbackReload.Value)
// 					{
// 						this.BigReload();
// 					}
// 					else
// 					{
// 						this.FastReload(true);
// 						base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 					}
// 				}
// 				else
// 				{
// 					base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 					if (MakerAPI.InsideAndLoaded)
// 					{
// 						Singleton<CustomBase>.Instance.updateCustomUI = true;
// 					}
// 				}
// 				yield break;
// 			}
//
// 			// Token: 0x0600001A RID: 26 RVA: 0x00002F71 File Offset: 0x00001171
// 			internal IEnumerator PreviewCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[PreviewCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.AccStateSync.InitCurOutfitTriggerInfo("OnCoordinateBeingLoaded");
// 				if (CharaStudio.Loaded)
// 				{
// 					base.StartCoroutine(this.RefreshCharaStatePanelCoroutine());
// 				}
// 				yield break;
// 			}
//
// 			// Token: 0x0600001B RID: 27 RVA: 0x00002F80 File Offset: 0x00001180
// 			internal IEnumerator RefreshCharaStatePanelCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RefreshCharaStatePanelCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.HairAccessoryCustomizer.UpdateAccessories(false);
// 				CharaStudio.RefreshCharaStatePanel();
// 				CharacterAccessory.MoreAccessoriesSupport.UpdateStudioUI(base.ChaControl);
// 				yield break;
// 			}
//
// 			// Token: 0x0600001C RID: 28 RVA: 0x00002F90 File Offset: 0x00001190
// 			internal void SetReferralIndex(int _index)
// 			{
// 				if (this.ReferralIndex != _index)
// 				{
// 					if (_index >= base.ChaControl.chaFile.coordinate.Length || _index < 0)
// 					{
// 						this.ReferralIndex = -1;
// 					}
// 					else
// 					{
// 						this.ReferralIndex = _index;
// 					}
// 				}
// 				CharacterAccessory.DebugMsg(4, string.Format("[SetReferralIndex][{0}][_index: {1}][ReferralIndex: {2}]", JetPack.Extensions.GetFullName(base.ChaControl), _index, this.ReferralIndex));
// 			}
//
// 			// Token: 0x0600001D RID: 29 RVA: 0x00002FFC File Offset: 0x000011FC
// 			internal int GetReferralIndex()
// 			{
// 				if (CharaStudio.Running)
// 				{
// 					OCIChar curOCIChar = CharaStudio.CurOCIChar;
// 					if (((curOCIChar != null) ? curOCIChar.charInfo : null) == base.ChaControl)
// 					{
// 						CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
// 					}
// 				}
// 				int num = (this.ReferralIndex < 0) ? base.ChaControl.chaFile.coordinate.Length : this.ReferralIndex;
// 				CharacterAccessory.DebugMsg(16, string.Format("[GetReferralIndex][{0}][_index: {1}][ReferralIndex: {2}]", JetPack.Extensions.GetFullName(base.ChaControl), num, this.ReferralIndex));
// 				return num;
// 			}
//
// 			// Token: 0x0600001E RID: 30 RVA: 0x00003088 File Offset: 0x00001288
// 			internal void FastReload(bool _noLoadStatus = true)
// 			{
// 				byte[] buffer = null;
// 				using (MemoryStream memoryStream = new MemoryStream())
// 				{
// 					using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
// 					{
// 						base.ChaControl.chaFile.SaveCharaFile(binaryWriter, false);
// 						buffer = memoryStream.ToArray();
// 					}
// 				}
// 				using (MemoryStream memoryStream2 = new MemoryStream(buffer))
// 				{
// 					using (BinaryReader binaryReader = new BinaryReader(memoryStream2))
// 					{
// 						base.ChaControl.chaFile.LoadCharaFile(binaryReader, true, _noLoadStatus);
// 					}
// 				}
// 			}
//
// 			// Token: 0x0600001F RID: 31 RVA: 0x00003144 File Offset: 0x00001344
// 			internal void BigReload()
// 			{
// 				string path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Paths.ExecutablePath) + "_CA.png");
// 				using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
// 				{
// 					base.ChaControl.chaFile.SaveCharaFile(fileStream, true);
// 				}
// 				Singleton<Studio>.Instance.dicInfo.Values.OfType<OCIChar>().FirstOrDefault((OCIChar x) => x.charInfo == base.ChaControl).ChangeChara(path);
// 			}
//
// 			// Token: 0x06000020 RID: 32 RVA: 0x000031D4 File Offset: 0x000013D4
// 			internal string GetCordName()
// 			{
// 				return this.GetCordName(this.CurrentCoordinateIndex);
// 			}
//
// 			// Token: 0x06000021 RID: 33 RVA: 0x000031E2 File Offset: 0x000013E2
// 			internal string GetCordName(int CoordinateIndex)
// 			{
// 				if (CoordinateIndex < CharacterAccessory._cordNames.Count)
// 				{
// 					return CharacterAccessory._cordNames[CoordinateIndex];
// 				}
// 				return string.Format("Extra {0}", CoordinateIndex - CharacterAccessory._cordNames.Count + 1);
// 			}
//
// 			// Token: 0x06000022 RID: 34 RVA: 0x0000321A File Offset: 0x0000141A
// 			internal void TaskLock()
// 			{
// 				this.DuringLoading = true;
// 			}
//
// 			// Token: 0x06000023 RID: 35 RVA: 0x00003223 File Offset: 0x00001423
// 			internal void TaskUnlock()
// 			{
// 				this.DuringLoading = false;
// 			}
//
// 			// Token: 0x06000024 RID: 36 RVA: 0x0000322C File Offset: 0x0000142C
// 			internal void AutoCopyCheck()
// 			{
// 				bool flag = true;
// 				CharacterAccessory.DebugMsg(4, string.Format("[OnCoordinateChanged][{0}][CurrentCoordinateIndex: {1}]", JetPack.Extensions.GetFullName(base.ChaControl), this.CurrentCoordinateIndex));
// 				if (!this.AutoCopyToBlank)
// 				{
// 					flag = false;
// 				}
// 				if (!this.FunctionEnable)
// 				{
// 					flag = false;
// 				}
// 				if (this.ReferralIndex == -1 && this.PartsInfo.Count == 0)
// 				{
// 					flag = false;
// 				}
// 				if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length && this.ReferralIndex == this.CurrentCoordinateIndex)
// 				{
// 					flag = false;
// 				}
// 				if (MakerAPI.InsideAndLoaded && !CharacterAccessory._cfgMakerMasterSwitch.Value)
// 				{
// 					flag = false;
// 				}
// 				if (flag)
// 				{
// 					base.ChaControl.StartCoroutine(this.OnCoordinateChangedCoroutine());
// 				}
// 			}
//
// 			// Token: 0x06000025 RID: 37 RVA: 0x000032EA File Offset: 0x000014EA
// 			internal IEnumerator OnCoordinateChangedCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[OnCoordinateChangedCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				if (CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(base.ChaControl, this.CurrentCoordinateIndex).Count > 0)
// 				{
// 					yield break;
// 				}
// 				this.TaskLock();
// 				if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length)
// 				{
// 					this.CopyPartsInfo();
// 				}
// 				else
// 				{
// 					this.RestorePartsInfo();
// 				}
// 				yield break;
// 			}
//
// 			// Token: 0x06000026 RID: 38 RVA: 0x000032FC File Offset: 0x000014FC
// 			internal void PrepareQueue()
// 			{
// 				this.QueueList = new List<CharacterAccessoryController.QueueItem>();
// 				if (this.ReferralIndex >= base.ChaControl.chaFile.coordinate.Length)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length && this.ReferralIndex == this.CurrentCoordinateIndex)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				int num = -1;
// 				if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length)
// 				{
// 					Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(base.ChaControl, this.ReferralIndex);
// 					num = ((dictionary.Count == 0) ? -1 : dictionary.Keys.Max());
// 				}
// 				else if (this.ReferralIndex == -1)
// 				{
// 					num = ((this.PartsInfo.Count == 0) ? -1 : this.PartsInfo.Keys.Max());
// 				}
// 				CharacterAccessory.DebugMsg(4, string.Format("[PrepareQueue][{0}][ReferralIndex: {1}][SrcLastNotEmpty: Slot{2:00}]", JetPack.Extensions.GetFullName(base.ChaControl), this.ReferralIndex, num + 1));
// 				if (num < 0)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				Dictionary<int, ChaFileAccessory.PartsInfo> dictionary2 = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(base.ChaControl, this.CurrentCoordinateIndex);
// 				if (dictionary2.Count == 0)
// 				{
// 					if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length)
// 					{
// 						this.CopyPartsInfo();
// 						return;
// 					}
// 					if (this.ReferralIndex == -1)
// 					{
// 						this.RestorePartsInfo();
// 					}
// 					return;
// 				}
// 				else
// 				{
// 					int num2 = 0;
// 					int num3 = 0;
// 					int num4 = dictionary2.Keys.Min();
// 					int num5 = dictionary2.Keys.Max();
// 					List<int> collection = dictionary2.Keys.ToList<int>();
// 					List<int> list = new List<int>();
// 					list.AddRange(collection);
// 					list.Reverse();
// 					CharacterAccessory.DebugMsg(4, string.Format("[PrepareQueue][{0}][CurrentCoordinateIndex: {1}][CurFirstNotEmpty: Slot{2:00}][CurLastNotEmpty: Slot{3:00}]", new object[]
// 					{
// 						JetPack.Extensions.GetFullName(base.ChaControl),
// 						this.CurrentCoordinateIndex,
// 						num4 + 1,
// 						num5 + 1
// 					}));
// 					if (num4 <= num)
// 					{
// 						num3 = num - num4 + 1;
// 						num2 = num5 + num3 - CharacterAccessory.MoreAccessoriesSupport.GetPartsCount(base.ChaControl, this.CurrentCoordinateIndex);
// 					}
// 					if (num3 > 0)
// 					{
// 						foreach (int num6 in list)
// 						{
// 							this.QueueList.Add(new CharacterAccessoryController.QueueItem(num6, num6 + num3));
// 						}
// 					}
// 					if (num2 > 0)
// 					{
// 						CharacterAccessory.MoreAccessoriesSupport.CheckAndPadPartInfo(base.ChaControl, this.CurrentCoordinateIndex, num5 + num3);
// 						base.StartCoroutine(this.TransferPartsInfoCoroutine());
// 						return;
// 					}
// 					if (this.QueueList.Count > 0)
// 					{
// 						this.TransferPartsInfo();
// 						return;
// 					}
// 					if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length)
// 					{
// 						this.CopyPartsInfo();
// 						return;
// 					}
// 					if (this.ReferralIndex == -1)
// 					{
// 						base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 						base.StartCoroutine(this.RestorePartsInfoCoroutine());
// 					}
// 					return;
// 				}
// 			}
//
// 			// Token: 0x06000027 RID: 39 RVA: 0x00003608 File Offset: 0x00001808
// 			internal IEnumerator TransferPartsInfoCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[TransferPartsInfoCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.TransferPartsInfo();
// 				yield break;
// 			}
//
// 			// Token: 0x06000028 RID: 40 RVA: 0x00003618 File Offset: 0x00001818
// 			internal void TransferPartsInfo()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[TransferPartsInfo][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				if (this.QueueList.Count == 0)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				for (int i = 0; i < this.QueueList.Count; i++)
// 				{
// 					int srcSlot = this.QueueList[i].SrcSlot;
// 					int dstSlot = this.QueueList[i].DstSlot;
// 					CharacterAccessory.DebugMsg(4, string.Format("[TransferPartsInfo][{0}][{1}][{2}]", JetPack.Extensions.GetFullName(base.ChaControl), srcSlot, dstSlot));
// 					AccessoryTransferEventArgs accessoryTransferEventArgs = new AccessoryTransferEventArgs(srcSlot, dstSlot);
// 					CharacterAccessory.MoreAccessoriesSupport.TransferPartsInfo(base.ChaControl, accessoryTransferEventArgs);
// 					CharacterAccessory.MoreAccessoriesSupport.RemovePartsInfo(base.ChaControl, this.CurrentCoordinateIndex, srcSlot);
// 					foreach (string text in CharacterAccessory._supportList)
// 					{
// 						Traverse.Create(this).Field(text).Method("TransferPartsInfo", new object[]
// 						{
// 							accessoryTransferEventArgs
// 						}).GetValue();
// 						Traverse.Create(this).Field(text).Method("RemovePartsInfo", new object[]
// 						{
// 							srcSlot
// 						}).GetValue();
// 					}
// 				}
// 				if (this.ReferralIndex > -1 && this.ReferralIndex < base.ChaControl.chaFile.coordinate.Length)
// 				{
// 					this.CopyPartsInfo();
// 					return;
// 				}
// 				if (this.ReferralIndex == -1)
// 				{
// 					base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 					base.ChaControl.StartCoroutine(this.RestorePartsInfoCoroutine());
// 				}
// 			}
//
// 			// Token: 0x06000029 RID: 41 RVA: 0x000037CC File Offset: 0x000019CC
// 			internal void CopyPartsInfo()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[CopyPartsInfo][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				if (!this.DuringLoading)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				if (!this.FunctionEnable)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				if (this.ReferralIndex == this.CurrentCoordinateIndex)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				List<ChaFileAccessory.PartsInfo> list = CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(base.ChaControl, this.ReferralIndex);
// 				List<int> list2 = new List<int>();
// 				for (int i = 0; i < list.Count; i++)
// 				{
// 					if (list[i].type > 120)
// 					{
// 						list2.Add(i);
// 					}
// 				}
// 				LogLevel logLevel = (LogLevel)4;
// 				string[] array = new string[5];
// 				array[0] = "[CopyPartsInfo][";
// 				array[1] = JetPack.Extensions.GetFullName(base.ChaControl);
// 				array[2] = "][Slots: ";
// 				array[3] = string.Join(",", (from Slot in list2
// 				select Slot.ToString()).ToArray<string>());
// 				array[4] = "]";
// 				CharacterAccessory.DebugMsg(logLevel, string.Concat(array));
// 				AccessoryCopyEventArgs accessoryCopyEventArgs = new AccessoryCopyEventArgs(list2, (ChaFileDefine.CoordinateType)this.ReferralIndex, (ChaFileDefine.CoordinateType)this.CurrentCoordinateIndex);
// 				CharacterAccessory.MoreAccessoriesSupport.CopyPartsInfo(base.ChaControl, accessoryCopyEventArgs);
// 				if (CharaStudio.Running)
// 				{
// 					base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 					base.ChaControl.StartCoroutine(this.CopyPluginSettingCoroutine(accessoryCopyEventArgs));
// 					return;
// 				}
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("CopyPartsInfo", new object[]
// 					{
// 						accessoryCopyEventArgs
// 					}).GetValue();
// 				}
// 				base.ChaControl.StartCoroutine(this.RefreshCoroutine());
// 			}
//
// 			// Token: 0x0600002A RID: 42 RVA: 0x0000399C File Offset: 0x00001B9C
// 			internal IEnumerator CopyPluginSettingCoroutine(AccessoryCopyEventArgs ev)
// 			{
// 				CharacterAccessory.DebugMsg(4, "[CopyPluginSettingCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.CopyPluginSetting(ev);
// 				yield break;
// 			}
//
// 			// Token: 0x0600002B RID: 43 RVA: 0x000039B4 File Offset: 0x00001BB4
// 			internal void CopyPluginSetting(AccessoryCopyEventArgs ev)
// 			{
// 				CharacterAccessory.DebugMsg(4, "[CopyPluginSetting][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("CopyPartsInfo", new object[]
// 					{
// 						ev
// 					}).GetValue();
// 				}
// 				base.ChaControl.StartCoroutine(this.RefreshCoroutine());
// 			}
//
// 			// Token: 0x0600002C RID: 44 RVA: 0x00003A58 File Offset: 0x00001C58
// 			internal void Backup()
// 			{
// 				int coordinateType = base.ChaControl.fileStatus.coordinateType;
// 				List<ChaFileAccessory.PartsInfo> list = CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(base.ChaControl, coordinateType);
// 				this.PartsInfo.Clear();
// 				this.PartsResolveInfo.Clear();
// 				for (int i = 0; i < list.Count; i++)
// 				{
// 					ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(base.ChaControl, coordinateType, i);
// 					if (partsInfo.type > 120)
// 					{
// 						byte[] array = MessagePackSerializer.Serialize<ChaFileAccessory.PartsInfo>(partsInfo);
// 						this.PartsInfo[i] = MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(array);
// 						this.PartsResolveInfo[i] = UniversalAutoResolver.TryGetResolutionInfo((ChaListDefine.CategoryNo)partsInfo.type, partsInfo.id);
// 					}
// 				}
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("Backup", Array.Empty<object>()).GetValue();
// 				}
// 			}
//
// 			// Token: 0x0600002D RID: 45 RVA: 0x00003B5C File Offset: 0x00001D5C
// 			internal static void MigrateData(ref ResolveInfo extResolve)
// 			{
// 				if (extResolve.GUID.IsNullOrWhiteSpace())
// 				{
// 					return;
// 				}
// 				List<MigrationInfo> migrationInfo = UniversalAutoResolver.GetMigrationInfo(extResolve.GUID);
// 				if (migrationInfo.Any((MigrationInfo x) => x.MigrationType == 2))
// 				{
// 					extResolve.GUID = "";
// 					return;
// 				}
// 				int slot = extResolve.Slot;
// 				ChaListDefine.CategoryNo categoryNo = extResolve.CategoryNo;
// 				IEnumerable<MigrationInfo> source = migrationInfo;
// 				Func<MigrationInfo, bool> <>9__1;
// 				Func<MigrationInfo, bool> predicate;
// 				if ((predicate = <>9__1) == null)
// 				{
// 					predicate = (<>9__1 = ((MigrationInfo x) => x.IDOld == slot && x.Category == categoryNo));
// 				}
// 				foreach (MigrationInfo migrationInfo2 in source.Where(predicate))
// 				{
// 					if (Sideloader.GetManifest(migrationInfo2.GUIDNew) != null)
// 					{
// 						extResolve.GUID = migrationInfo2.GUIDNew;
// 						extResolve.Slot = migrationInfo2.IDNew;
// 						return;
// 					}
// 				}
// 				foreach (MigrationInfo migrationInfo3 in from x in migrationInfo
// 				where x.MigrationType == 1
// 				select x)
// 				{
// 					if (Sideloader.GetManifest(migrationInfo3.GUIDNew) != null)
// 					{
// 						extResolve.GUID = migrationInfo3.GUIDNew;
// 					}
// 				}
// 			}
//
// 			// Token: 0x0600002E RID: 46 RVA: 0x00003CD0 File Offset: 0x00001ED0
// 			internal IEnumerator RestorePartsInfoCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RestoreCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.RestorePartsInfo();
// 				yield break;
// 			}
//
// 			// Token: 0x0600002F RID: 47 RVA: 0x00003CE0 File Offset: 0x00001EE0
// 			internal void Reset()
// 			{
// 				this.FunctionEnable = false;
// 				this.AutoCopyToBlank = false;
// 				this.ReferralIndex = -1;
// 				this.PartsInfo.Clear();
// 				this.PartsResolveInfo.Clear();
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("Reset", Array.Empty<object>()).GetValue();
// 				}
// 			}
//
// 			// Token: 0x06000030 RID: 48 RVA: 0x00003D78 File Offset: 0x00001F78
// 			internal void RestorePartsInfo()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RestorePartsInfo][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				if (!this.DuringLoading)
// 				{
// 					return;
// 				}
// 				if (!this.FunctionEnable)
// 				{
// 					this.TaskUnlock();
// 					return;
// 				}
// 				if (this.PartsInfo.Count == 0)
// 				{
// 					CharacterAccessory._logger.LogMessage("Nothing to restore");
// 					this.TaskUnlock();
// 					return;
// 				}
// 				int coordinateType = base.ChaControl.fileStatus.coordinateType;
// 				Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = CharacterAccessory.MoreAccessoriesSupport.ListUsedPartsInfo(base.ChaControl, coordinateType);
// 				if (dictionary.Count > 0 && dictionary.Keys.Min() <= this.PartsInfo.Keys.Max())
// 				{
// 					CharacterAccessory._logger.LogMessage(string.Format("Error: parts overlap [RefUsedPartsInfo.Keys.Min(): {0}][PartsInfo.Keys.Max(): {1}]", dictionary.Keys.Min(), this.PartsInfo.Keys.Max()));
// 					this.TaskUnlock();
// 					return;
// 				}
// 				LogLevel logLevel = 16;
// 				string[] array = new string[5];
// 				array[0] = "[RestorePartsInfo][";
// 				array[1] = JetPack.Extensions.GetFullName(base.ChaControl);
// 				array[2] = "][Slots: ";
// 				array[3] = string.Join(",", (from Slot in this.PartsInfo.Keys
// 				select Slot.ToString()).ToArray<string>());
// 				array[4] = "]";
// 				CharacterAccessory.DebugMsg(logLevel, string.Concat(array));
// 				foreach (KeyValuePair<int, ChaFileAccessory.PartsInfo> keyValuePair in this.PartsInfo)
// 				{
// 					CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(base.ChaControl, coordinateType, keyValuePair.Key, keyValuePair.Value);
// 				}
// 				if (CharaStudio.Running)
// 				{
// 					base.ChaControl.ChangeCoordinateTypeAndReload(false);
// 					base.StartCoroutine(this.RestorePluginSettingCoroutine());
// 					return;
// 				}
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("Restore", Array.Empty<object>()).GetValue();
// 				}
// 				base.StartCoroutine(this.RefreshCoroutine());
// 			}
//
// 			// Token: 0x06000031 RID: 49 RVA: 0x00003FC4 File Offset: 0x000021C4
// 			internal IEnumerator RestorePluginSettingCoroutine()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RestorePluginSettingCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.RestorePluginSetting();
// 				yield break;
// 			}
//
// 			// Token: 0x06000032 RID: 50 RVA: 0x00003FD4 File Offset: 0x000021D4
// 			internal void RestorePluginSetting()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[RestorePluginSetting][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				foreach (string text in CharacterAccessory._supportList)
// 				{
// 					Traverse.Create(this).Field(text).Method("Restore", Array.Empty<object>()).GetValue();
// 				}
// 				base.StartCoroutine(this.RefreshCoroutine());
// 			}
//
// 			// Token: 0x06000035 RID: 53 RVA: 0x000040A8 File Offset: 0x000022A8
// 			[CompilerGenerated]
// 			private IEnumerator <OnReload>g__OnReloadCoroutine|18_0()
// 			{
// 				CharacterAccessory.DebugMsg(4, "[OnReloadCoroutine][" + JetPack.Extensions.GetFullName(base.ChaControl) + "] fired");
// 				yield return Toolbox.WaitForEndOfFrame;
// 				yield return Toolbox.WaitForEndOfFrame;
// 				this.AutoCopyCheck();
// 				yield break;
// 			}
//
// 			// Token: 0x04000012 RID: 18
// 			internal CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag HairAccessoryCustomizer;
//
// 			// Token: 0x04000013 RID: 19
// 			internal CharacterAccessory.MaterialEditorSupport.UrineBag MaterialEditor;
//
// 			// Token: 0x04000014 RID: 20
// 			internal CharacterAccessory.MaterialRouterSupport.UrineBag MaterialRouter;
//
// 			// Token: 0x04000015 RID: 21
// 			internal CharacterAccessory.AccStateSyncSupport.UrineBag AccStateSync;
//
// 			// Token: 0x04000016 RID: 22
// 			internal CharacterAccessory.DynamicBoneEditorSupport.UrineBag DynamicBoneEditor;
//
// 			// Token: 0x04000017 RID: 23
// 			internal CharacterAccessory.AAAPKSupport.UrineBag AAAPK;
//
// 			// Token: 0x04000018 RID: 24
// 			internal CharacterAccessory.BendUrAccSupport.UrineBag BendUrAcc;
//
// 			// Token: 0x04000019 RID: 25
// 			internal Dictionary<int, ChaFileAccessory.PartsInfo> PartsInfo = new Dictionary<int, ChaFileAccessory.PartsInfo>();
//
// 			// Token: 0x0400001A RID: 26
// 			internal Dictionary<int, ResolveInfo> PartsResolveInfo = new Dictionary<int, ResolveInfo>();
//
// 			// Token: 0x0400001B RID: 27
// 			internal int ReferralIndex = -1;
//
// 			// Token: 0x0400001C RID: 28
// 			internal bool FunctionEnable;
//
// 			// Token: 0x0400001D RID: 29
// 			internal bool AutoCopyToBlank;
//
// 			// Token: 0x0400001E RID: 30
// 			internal bool DuringLoading;
//
// 			// Token: 0x0400001F RID: 31
// 			internal List<CharacterAccessoryController.QueueItem> QueueList = new List<CharacterAccessoryController.QueueItem>();
//
// 			// Token: 0x02000013 RID: 19
// 			internal class QueueItem
// 			{
// 				// Token: 0x17000006 RID: 6
// 				// (get) Token: 0x0600007E RID: 126 RVA: 0x0000579D File Offset: 0x0000399D
// 				// (set) Token: 0x0600007F RID: 127 RVA: 0x000057A5 File Offset: 0x000039A5
// 				public int SrcSlot { get; set; }
//
// 				// Token: 0x17000007 RID: 7
// 				// (get) Token: 0x06000080 RID: 128 RVA: 0x000057AE File Offset: 0x000039AE
// 				// (set) Token: 0x06000081 RID: 129 RVA: 0x000057B6 File Offset: 0x000039B6
// 				public int DstSlot { get; set; }
//
// 				// Token: 0x06000082 RID: 130 RVA: 0x000057BF File Offset: 0x000039BF
// 				public QueueItem(int _src, int _dst)
// 				{
// 					this.SrcSlot = _src;
// 					this.DstSlot = _dst;
// 				}
// 			}
// 		}
//
// 		// Token: 0x02000004 RID: 4
// 		internal class Hooks
// 		{
// 			// Token: 0x06000037 RID: 55 RVA: 0x000040CA File Offset: 0x000022CA
// 			internal static bool DuringLoading_Prefix(CharaCustomFunctionController __instance)
// 			{
// 				return !CharacterAccessory.GetController(__instance.ChaControl).DuringLoading;
// 			}
//
// 			// Token: 0x06000038 RID: 56 RVA: 0x000040E4 File Offset: 0x000022E4
// 			internal static bool DuringLoading_IEnumerator_Prefix(CharaCustomFunctionController __instance, ref IEnumerator __result)
// 			{
// 				if (CharacterAccessory.GetController(__instance.ChaControl).DuringLoading)
// 				{
// 					IEnumerator enumerator = __result;
// 					__result = new IEnumerator[]
// 					{
// 						enumerator,
// 						CharacterAccessory.Hooks.<DuringLoading_IEnumerator_Prefix>g__YieldBreak|1_0()
// 					}.GetEnumerator();
// 					return false;
// 				}
// 				return true;
// 			}
//
// 			// Token: 0x0600003A RID: 58 RVA: 0x0000412A File Offset: 0x0000232A
// 			[CompilerGenerated]
// 			internal static IEnumerator <DuringLoading_IEnumerator_Prefix>g__YieldBreak|1_0()
// 			{
// 				yield break;
// 			}
// 		}
//
// 		// Token: 0x02000005 RID: 5
// 		internal class HooksMaker
// 		{
// 			// Token: 0x0600003B RID: 59 RVA: 0x00004134 File Offset: 0x00002334
// 			internal static bool DuringLoading_Prefix()
// 			{
// 				CharacterAccessoryController controller = CharacterAccessory.GetController(Singleton<CustomBase>.Instance.chaCtrl);
// 				return controller == null || !controller.DuringLoading;
// 			}
// 		}
//
// 		// Token: 0x02000006 RID: 6
// 		internal static class AAAPKSupport
// 		{
// 			// Token: 0x0600003D RID: 61 RVA: 0x00004168 File Offset: 0x00002368
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("madevil.kk.AAAPK", out pluginInfo);
// 				CharacterAccessory.AAAPKSupport._instance = ((pluginInfo != null) ? pluginInfo.Instance : null);
// 				if (CharacterAccessory.AAAPKSupport._instance != null)
// 				{
// 					CharacterAccessory.AAAPKSupport._legacy = (pluginInfo.Metadata.Version.CompareTo(new Version("1.1.0.0")) < 0);
// 					if (CharacterAccessory.AAAPKSupport._legacy)
// 					{
// 						CharacterAccessory._logger.LogError(string.Format("AAAPK version {0} found, minimun version 1.1 is reqired", pluginInfo.Metadata.Version));
// 						return;
// 					}
// 					CharacterAccessory.AAAPKSupport._installed = true;
// 					CharacterAccessory._supportList.Add("AAAPK");
// 					Assembly assembly = CharacterAccessory.AAAPKSupport._instance.GetType().Assembly;
// 					CharacterAccessory.AAAPKSupport._types["AAAPKController"] = assembly.GetType("AAAPK.AAAPK+AAAPKController");
// 					CharacterAccessory.AAAPKSupport._types["ParentRule"] = assembly.GetType("AAAPK.AAAPK+ParentRule");
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.AAAPKSupport._types["AAAPKController"].GetMethod("ApplyParentRuleList", AccessTools.all, null, new Type[]
// 					{
// 						typeof(string)
// 					}, null), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x0600003E RID: 62 RVA: 0x000042AB File Offset: 0x000024AB
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				return Traverse.Create(CharacterAccessory.AAAPKSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x04000020 RID: 32
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000021 RID: 33
// 			internal static bool _installed = false;
//
// 			// Token: 0x04000022 RID: 34
// 			internal static bool _legacy = false;
//
// 			// Token: 0x04000023 RID: 35
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x02000021 RID: 33
// 			internal class UrineBag
// 			{
// 				// Token: 0x060000CD RID: 205 RVA: 0x00005FF8 File Offset: 0x000041F8
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.AAAPKSupport.GetController(this._chaCtrl);
// 					this._traverses["pluginCtrl"] = Traverse.Create(this._pluginCtrl);
// 				}
//
// 				// Token: 0x060000CE RID: 206 RVA: 0x0000605C File Offset: 0x0000425C
// 				internal object GetExtDataLink()
// 				{
// 					return this._traverses["pluginCtrl"].Field("ParentRuleList").GetValue();
// 				}
//
// 				// Token: 0x060000CF RID: 207 RVA: 0x0000607D File Offset: 0x0000427D
// 				internal void Reset()
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 				}
//
// 				// Token: 0x060000D0 RID: 208 RVA: 0x00006094 File Offset: 0x00004294
// 				internal List<string> Save()
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return null;
// 					}
// 					List<string> list = new List<string>();
// 					foreach (object obj in this._charaAccData)
// 					{
// 						list.Add(JSONSerializer.Serialize(CharacterAccessory.AAAPKSupport._types["ParentRule"], obj, false, null));
// 					}
// 					return list;
// 				}
//
// 				// Token: 0x060000D1 RID: 209 RVA: 0x00006110 File Offset: 0x00004310
// 				internal void Load(List<string> _json)
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					foreach (string text in _json)
// 					{
// 						this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.AAAPKSupport._types["ParentRule"], text, null, null));
// 					}
// 				}
//
// 				// Token: 0x060000D2 RID: 210 RVA: 0x00006190 File Offset: 0x00004390
// 				internal void Backup()
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
// 					List<int> list;
// 					if (partsInfo == null)
// 					{
// 						list = null;
// 					}
// 					else
// 					{
// 						Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
// 						list = ((keys != null) ? keys.ToList<int>() : null);
// 					}
// 					List<int> list2 = list;
// 					int count = (extDataLink as IList).Count;
// 					for (int i = 0; i < count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(extDataLink, i));
// 						Traverse traverse = Traverse.Create(obj);
// 						if (traverse.Property("Coordinate", null).GetValue<int>() == coordinateType && list2.Contains(traverse.Property("Slot", null).GetValue<int>()))
// 						{
// 							traverse.Property("Coordinate", null).SetValue(-1);
// 							this._charaAccData.Add(obj);
// 						}
// 					}
// 				}
//
// 				// Token: 0x060000D3 RID: 211 RVA: 0x00006280 File Offset: 0x00004480
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					for (int i = 0; i < this._charaAccData.Count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(this._charaAccData[i]);
// 						Traverse.Create(obj).Property("Coordinate", null).SetValue(coordinateType);
// 						(extDataLink as IList).Add(obj);
// 					}
// 				}
//
// 				// Token: 0x060000D4 RID: 212 RVA: 0x00006304 File Offset: 0x00004504
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					foreach (int num in _args.CopiedSlotIndexes)
// 					{
// 						this._traverses["pluginCtrl"].Method("CloneRule", new object[]
// 						{
// 							num,
// 							num,
// 							(int)_args.CopySource,
// 							(int)_args.CopyDestination
// 						}).GetValue();
// 					}
// 				}
//
// 				// Token: 0x060000D5 RID: 213 RVA: 0x000063A8 File Offset: 0x000045A8
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					this._traverses["pluginCtrl"].Method("MoveRule", new object[]
// 					{
// 						_args.SourceSlotIndex,
// 						_args.DestinationSlotIndex,
// 						coordinateType
// 					}).GetValue();
// 				}
//
// 				// Token: 0x060000D6 RID: 214 RVA: 0x00006419 File Offset: 0x00004619
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					if (!CharacterAccessory.AAAPKSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("RemoveRule", new object[]
// 					{
// 						_slotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x0400007E RID: 126
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x0400007F RID: 127
// 				private readonly CharaCustomFunctionController _pluginCtrl;
//
// 				// Token: 0x04000080 RID: 128
// 				private readonly List<object> _charaAccData = new List<object>();
//
// 				// Token: 0x04000081 RID: 129
// 				private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
// 			}
// 		}
//
// 		// Token: 0x02000007 RID: 7
// 		internal static class BendUrAccSupport
// 		{
// 			// Token: 0x06000040 RID: 64 RVA: 0x000042F0 File Offset: 0x000024F0
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("madevil.kk.BendUrAcc", out pluginInfo);
// 				CharacterAccessory.BendUrAccSupport._instance = ((pluginInfo != null) ? pluginInfo.Instance : null);
// 				if (CharacterAccessory.BendUrAccSupport._instance != null)
// 				{
// 					if (pluginInfo.Metadata.Version.CompareTo(new Version("1.0.5.0")) < 0)
// 					{
// 						CharacterAccessory._logger.LogError(string.Format("BendUrAcc version {0} found, minimun version 1.0.5.0 is reqired", pluginInfo.Metadata.Version));
// 						return;
// 					}
// 					CharacterAccessory.BendUrAccSupport._installed = true;
// 					CharacterAccessory._supportList.Add("BendUrAcc");
// 					Assembly assembly = CharacterAccessory.BendUrAccSupport._instance.GetType().Assembly;
// 					CharacterAccessory.BendUrAccSupport._types["BendUrAccController"] = assembly.GetType("BendUrAcc.BendUrAcc+BendUrAccController");
// 					CharacterAccessory.BendUrAccSupport._types["BendModifier"] = assembly.GetType("BendUrAcc.BendUrAcc+BendModifier");
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.BendUrAccSupport._types["BendUrAccController"].GetMethod("ApplyBendModifierList", AccessTools.all, null, new Type[]
// 					{
// 						typeof(string)
// 					}, null), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x06000041 RID: 65 RVA: 0x00004427 File Offset: 0x00002627
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				return Traverse.Create(CharacterAccessory.BendUrAccSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x04000024 RID: 36
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000025 RID: 37
// 			internal static bool _installed = false;
//
// 			// Token: 0x04000026 RID: 38
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x02000022 RID: 34
// 			internal class UrineBag
// 			{
// 				// Token: 0x060000D7 RID: 215 RVA: 0x00006454 File Offset: 0x00004654
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.BendUrAccSupport.GetController(this._chaCtrl);
// 					this._traverses["pluginCtrl"] = Traverse.Create(this._pluginCtrl);
// 				}
//
// 				// Token: 0x060000D8 RID: 216 RVA: 0x000064B8 File Offset: 0x000046B8
// 				internal object GetExtDataLink()
// 				{
// 					return this._traverses["pluginCtrl"].Field("BendModifierList").GetValue();
// 				}
//
// 				// Token: 0x060000D9 RID: 217 RVA: 0x000064D9 File Offset: 0x000046D9
// 				internal void Reset()
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 				}
//
// 				// Token: 0x060000DA RID: 218 RVA: 0x000064F0 File Offset: 0x000046F0
// 				internal List<string> Save()
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return null;
// 					}
// 					List<string> list = new List<string>();
// 					foreach (object obj in this._charaAccData)
// 					{
// 						list.Add(JSONSerializer.Serialize(CharacterAccessory.BendUrAccSupport._types["BendModifier"], obj, false, null));
// 					}
// 					return list;
// 				}
//
// 				// Token: 0x060000DB RID: 219 RVA: 0x0000656C File Offset: 0x0000476C
// 				internal void Load(List<string> _json)
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					foreach (string text in _json)
// 					{
// 						this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.BendUrAccSupport._types["BendModifier"], text, null, null));
// 					}
// 				}
//
// 				// Token: 0x060000DC RID: 220 RVA: 0x000065EC File Offset: 0x000047EC
// 				internal void Backup()
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
// 					List<int> list;
// 					if (partsInfo == null)
// 					{
// 						list = null;
// 					}
// 					else
// 					{
// 						Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
// 						list = ((keys != null) ? keys.ToList<int>() : null);
// 					}
// 					List<int> list2 = list;
// 					int count = (extDataLink as IList).Count;
// 					for (int i = 0; i < count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(extDataLink, i));
// 						Traverse traverse = Traverse.Create(obj);
// 						if (traverse.Property("Coordinate", null).GetValue<int>() == coordinateType && list2.Contains(traverse.Property("Slot", null).GetValue<int>()))
// 						{
// 							traverse.Property("Coordinate", null).SetValue(-1);
// 							this._charaAccData.Add(obj);
// 						}
// 					}
// 				}
//
// 				// Token: 0x060000DD RID: 221 RVA: 0x000066DC File Offset: 0x000048DC
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					for (int i = 0; i < this._charaAccData.Count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(this._charaAccData[i]);
// 						Traverse.Create(obj).Property("Coordinate", null).SetValue(coordinateType);
// 						(extDataLink as IList).Add(obj);
// 					}
// 				}
//
// 				// Token: 0x060000DE RID: 222 RVA: 0x00006760 File Offset: 0x00004960
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					foreach (int num in _args.CopiedSlotIndexes)
// 					{
// 						this._traverses["pluginCtrl"].Method("CloneModifier", new object[]
// 						{
// 							num,
// 							num,
// 							(int)_args.CopySource,
// 							(int)_args.CopyDestination
// 						}).GetValue();
// 					}
// 				}
//
// 				// Token: 0x060000DF RID: 223 RVA: 0x00006804 File Offset: 0x00004A04
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					this._traverses["pluginCtrl"].Method("CloneModifier", new object[]
// 					{
// 						_args.SourceSlotIndex,
// 						_args.DestinationSlotIndex,
// 						coordinateType,
// 						coordinateType
// 					}).GetValue();
// 					this._traverses["pluginCtrl"].Method("RemoveSlotModifier", new object[]
// 					{
// 						coordinateType,
// 						_args.SourceSlotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x060000E0 RID: 224 RVA: 0x000068BB File Offset: 0x00004ABB
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					if (!CharacterAccessory.BendUrAccSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("RemoveSlotModifier", new object[]
// 					{
// 						_slotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x04000082 RID: 130
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x04000083 RID: 131
// 				private readonly CharaCustomFunctionController _pluginCtrl;
//
// 				// Token: 0x04000084 RID: 132
// 				private readonly List<object> _charaAccData = new List<object>();
//
// 				// Token: 0x04000085 RID: 133
// 				private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
// 			}
// 		}
//
// 		// Token: 0x02000008 RID: 8
// 		internal static class BonerStateSync
// 		{
// 			// Token: 0x06000043 RID: 67 RVA: 0x00004464 File Offset: 0x00002664
// 			internal static void Init()
// 			{
// 				CharacterAccessory.BonerStateSync._instance = Toolbox.GetPluginInstance("BonerStateSync");
// 				if (CharacterAccessory.BonerStateSync._instance != null)
// 				{
// 					CharacterAccessory.BonerStateSync._installed = true;
// 				}
// 				if (!CharacterAccessory.BonerStateSync._installed)
// 				{
// 					CharacterAccessory.BonerStateSync._instance = Toolbox.GetPluginInstance("madevil.kk.BonerStateSync");
// 					if (CharacterAccessory.BonerStateSync._instance != null)
// 					{
// 						CharacterAccessory.BonerStateSync._installed = true;
// 					}
// 				}
// 				if (CharacterAccessory.BonerStateSync._installed)
// 				{
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.BonerStateSync._instance.GetType().Assembly.GetType("BonerStateSync.BonerStateSync+BonerStateSyncController").GetMethod("InitCurOutfitTriggerInfo", AccessTools.all, null, new Type[]
// 					{
// 						typeof(string)
// 					}, null), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x04000027 RID: 39
// 			internal static BaseUnityPlugin _instance;
//
// 			// Token: 0x04000028 RID: 40
// 			internal static bool _installed;
// 		}
//
// 		// Token: 0x02000009 RID: 9
// 		internal static class CumOnOverSupport
// 		{
// 			// Token: 0x06000044 RID: 68 RVA: 0x00004530 File Offset: 0x00002730
// 			internal static void Init()
// 			{
// 				CharacterAccessory.CumOnOverSupport._instance = Toolbox.GetPluginInstance("madevil.kk.CumOnOver");
// 				if (CharacterAccessory.CumOnOverSupport._instance != null)
// 				{
// 					CharacterAccessory.CumOnOverSupport._installed = true;
// 				}
// 				if (CharacterAccessory.CumOnOverSupport._installed)
// 				{
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.CumOnOverSupport._instance.GetType().Assembly.GetType("CumOnOver.CumOnOver+Hooks").GetMethod("ChaControl_UpdateClothesSiru", AccessTools.all), new HarmonyMethod(typeof(CharacterAccessory.CumOnOverSupport.Hooks), "ChaControl_UpdateClothesSiru_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x04000029 RID: 41
// 			internal static BaseUnityPlugin _instance;
//
// 			// Token: 0x0400002A RID: 42
// 			internal static bool _installed;
//
// 			// Token: 0x02000023 RID: 35
// 			internal static class Hooks
// 			{
// 				// Token: 0x060000E1 RID: 225 RVA: 0x000068F4 File Offset: 0x00004AF4
// 				internal static bool ChaControl_UpdateClothesSiru_Prefix(ChaControl __0)
// 				{
// 					bool flag = true;
// 					if (CharacterAccessory.GetController(__0).DuringLoading)
// 					{
// 						flag = false;
// 					}
// 					if (!flag)
// 					{
// 						CharacterAccessory.DebugMsg(4, "[ChaControl_UpdateClothesSiru_Prefix][" + JetPack.Extensions.GetFullName(__0) + "] await loading");
// 						return false;
// 					}
// 					return flag;
// 				}
// 			}
// 		}
//
// 		// Token: 0x0200000A RID: 10
// 		internal static class MoreAccessoriesSupport
// 		{
// 			// Token: 0x06000045 RID: 69 RVA: 0x000045BC File Offset: 0x000027BC
// 			internal static void Init()
// 			{
// 				CharacterAccessory.MoreAccessoriesSupport._installed = MoreAccessories.Installed;
// 				if (!CharacterAccessory.MoreAccessoriesSupport._installed)
// 				{
// 					return;
// 				}
// 				CharacterAccessory.MoreAccessoriesSupport._instance = MoreAccessories.Instance;
// 				Assembly assembly = CharacterAccessory.MoreAccessoriesSupport._instance.GetType().Assembly;
// 				CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg = MoreAccessories.BuggyBootleg;
// 				if (CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg)
// 				{
// 					return;
// 				}
// 				CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.MoreAccessoriesSupport._instance.GetType().Assembly.GetType("MoreAccessoriesKOI.ChaControl_UpdateVisible_Patches").GetMethod("Postfix", AccessTools.all, null, new Type[]
// 				{
// 					typeof(ChaControl)
// 				}, null), new HarmonyMethod(typeof(CharacterAccessory.MoreAccessoriesSupport.Hooks), "ChaControl_UpdateVisible_Patches_Prefix", null), null, null, null, null);
// 				if (CharaStudio.Running)
// 				{
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.MoreAccessoriesSupport._instance.GetType().GetMethod("UpdateStudioUI", AccessTools.all, null, new Type[0], null), new HarmonyMethod(typeof(CharacterAccessory.MoreAccessoriesSupport.Hooks), "MoreAccessories_UpdateStudioUI_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x06000046 RID: 70 RVA: 0x000046C8 File Offset: 0x000028C8
// 			internal static void UpdateStudioUI(ChaControl _chaCtrl)
// 			{
// 				if (CharacterAccessory.MoreAccessoriesSupport.BuggyBootleg)
// 				{
// 					return;
// 				}
// 				if (CharaStudio.CurOCIChar == null)
// 				{
// 					return;
// 				}
// 				if (CharaStudio.CurOCIChar.charInfo != _chaCtrl)
// 				{
// 					return;
// 				}
// 				AccessTools.Method(CharacterAccessory.MoreAccessoriesSupport._instance.GetType(), "UpdateUI", null, null).Invoke(CharacterAccessory.MoreAccessoriesSupport._instance, null);
// 			}
//
// 			// Token: 0x06000047 RID: 71 RVA: 0x0000471C File Offset: 0x0000291C
// 			internal static int GetPartsCount(ChaControl chaCtrl, int CoordinateIndex)
// 			{
// 				List<ChaFileAccessory.PartsInfo> list = CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(chaCtrl, CoordinateIndex);
// 				return ((list != null) ? new int?(list.Count) : null).Value + 20;
// 			}
//
// 			// Token: 0x06000048 RID: 72 RVA: 0x00004754 File Offset: 0x00002954
// 			internal static Dictionary<int, ChaFileAccessory.PartsInfo> ListUsedPartsInfo(ChaControl _chaCtrl, int _coordinateIndex)
// 			{
// 				Dictionary<int, ChaFileAccessory.PartsInfo> dictionary = new Dictionary<int, ChaFileAccessory.PartsInfo>();
// 				int num = 0;
// 				foreach (ChaFileAccessory.PartsInfo partsInfo in CharacterAccessory.MoreAccessoriesSupport.ListPartsInfo(_chaCtrl, _coordinateIndex))
// 				{
// 					if (partsInfo.type > 120)
// 					{
// 						dictionary[num] = partsInfo;
// 					}
// 					num++;
// 				}
// 				return dictionary;
// 			}
//
// 			// Token: 0x06000049 RID: 73 RVA: 0x000047C0 File Offset: 0x000029C0
// 			internal static ChaFileAccessory.PartsInfo GetPartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex)
// 			{
// 				return Accessory.GetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex);
// 			}
//
// 			// Token: 0x0600004A RID: 74 RVA: 0x000047CC File Offset: 0x000029CC
// 			internal static void SetPartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex, ChaFileAccessory.PartsInfo _part)
// 			{
// 				byte[] array = MessagePackSerializer.Serialize<ChaFileAccessory.PartsInfo>(_part);
// 				Accessory.SetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex, MessagePackSerializer.Deserialize<ChaFileAccessory.PartsInfo>(array));
// 			}
//
// 			// Token: 0x0600004B RID: 75 RVA: 0x000047EE File Offset: 0x000029EE
// 			internal static List<ChaFileAccessory.PartsInfo> ListPartsInfo(ChaControl _chaCtrl, int _coordinateIndex)
// 			{
// 				return Accessory.ListPartsInfo(_chaCtrl, _coordinateIndex);
// 			}
//
// 			// Token: 0x0600004C RID: 76 RVA: 0x000047F7 File Offset: 0x000029F7
// 			internal static void CheckAndPadPartInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex)
// 			{
// 				MoreAccessories.CheckAndPadPartInfo(_chaCtrl, _coordinateIndex, _slotIndex);
// 			}
//
// 			// Token: 0x0600004D RID: 77 RVA: 0x00004801 File Offset: 0x00002A01
// 			internal static ChaAccessoryComponent GetChaAccessoryComponent(ChaControl _chaCtrl, int _slotIndex)
// 			{
// 				return Accessory.GetChaAccessoryComponent(_chaCtrl, _slotIndex);
// 			}
//
// 			// Token: 0x0600004E RID: 78 RVA: 0x0000480A File Offset: 0x00002A0A
// 			internal static bool IsHairAccessory(ChaControl _chaCtrl, int _slotIndex)
// 			{
// 				return Accessory.IsHairAccessory(_chaCtrl, _slotIndex);
// 			}
//
// 			// Token: 0x0600004F RID: 79 RVA: 0x00004814 File Offset: 0x00002A14
// 			internal static void CopyPartsInfo(ChaControl _chaCtrl, AccessoryCopyEventArgs ev)
// 			{
// 				foreach (int slotIndex in ev.CopiedSlotIndexes)
// 				{
// 					ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(_chaCtrl, (int)ev.CopySource, slotIndex);
// 					CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, (int)ev.CopyDestination, slotIndex, partsInfo);
// 				}
// 			}
//
// 			// Token: 0x06000050 RID: 80 RVA: 0x00004878 File Offset: 0x00002A78
// 			internal static void TransferPartsInfo(ChaControl _chaCtrl, AccessoryTransferEventArgs ev)
// 			{
// 				int coordinateType = _chaCtrl.fileStatus.coordinateType;
// 				ChaFileAccessory.PartsInfo partsInfo = CharacterAccessory.MoreAccessoriesSupport.GetPartsInfo(_chaCtrl, coordinateType, ev.SourceSlotIndex);
// 				CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, coordinateType, ev.DestinationSlotIndex, partsInfo);
// 			}
//
// 			// Token: 0x06000051 RID: 81 RVA: 0x000048AD File Offset: 0x00002AAD
// 			internal static void RemovePartsInfo(ChaControl _chaCtrl, int _coordinateIndex, int _slotIndex)
// 			{
// 				CharacterAccessory.MoreAccessoriesSupport.SetPartsInfo(_chaCtrl, _coordinateIndex, _slotIndex, new ChaFileAccessory.PartsInfo());
// 			}
//
// 			// Token: 0x0400002B RID: 43
// 			internal static BaseUnityPlugin _instance;
//
// 			// Token: 0x0400002C RID: 44
// 			internal static bool _installed;
//
// 			// Token: 0x0400002D RID: 45
// 			internal static bool BuggyBootleg;
//
// 			// Token: 0x02000024 RID: 36
// 			internal static class Hooks
// 			{
// 				// Token: 0x060000E2 RID: 226 RVA: 0x00006934 File Offset: 0x00004B34
// 				internal static bool ChaControl_UpdateVisible_Patches_Prefix(ChaControl __0)
// 				{
// 					CharacterAccessoryController controller = CharacterAccessory.GetController(__0);
// 					return controller == null || !controller.DuringLoading;
// 				}
//
// 				// Token: 0x060000E3 RID: 227 RVA: 0x00006960 File Offset: 0x00004B60
// 				internal static bool MoreAccessories_UpdateStudioUI_Prefix(object __instance)
// 				{
// 					if (!CharacterAccessory._cfgMAHookUpdateStudioUI.Value)
// 					{
// 						return true;
// 					}
// 					bool flag = true;
// 					if (CharaStudio.CurOCIChar != null && CharacterAccessory.GetController(CharaStudio.CurOCIChar).DuringLoading)
// 					{
// 						flag = false;
// 					}
// 					return flag && flag;
// 				}
// 			}
// 		}
//
// 		// Token: 0x0200000B RID: 11
// 		internal static class MoreOutfitsSupport
// 		{
// 			// Token: 0x06000052 RID: 82 RVA: 0x000048BC File Offset: 0x00002ABC
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.moreoutfits", out pluginInfo);
// 				CharacterAccessory.MoreOutfitsSupport._instance = ((pluginInfo != null) ? pluginInfo.Instance : null);
// 				if (CharacterAccessory.MoreOutfitsSupport._instance != null)
// 				{
// 					CharacterAccessory.MoreOutfitsSupport._installed = true;
// 				}
// 			}
//
// 			// Token: 0x06000053 RID: 83 RVA: 0x000048FF File Offset: 0x00002AFF
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				return Traverse.Create(CharacterAccessory.MoreOutfitsSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x06000054 RID: 84 RVA: 0x00004924 File Offset: 0x00002B24
// 			internal static Dictionary<int, string> CoordinateNames(CharaCustomFunctionController _pluginCtrl)
// 			{
// 				return Traverse.Create(_pluginCtrl).Field("CoordinateNames").GetValue<Dictionary<int, string>>();
// 			}
//
// 			// Token: 0x06000055 RID: 85 RVA: 0x0000493B File Offset: 0x00002B3B
// 			internal static string GetCoodinateName(ChaControl _chaCtrl, int _coordinateIndex)
// 			{
// 				return MoreOutfits.GetCoodinateName(_chaCtrl, _coordinateIndex);
// 			}
//
// 			// Token: 0x06000056 RID: 86 RVA: 0x00004944 File Offset: 0x00002B44
// 			internal static string GetCoodinateName(CharaCustomFunctionController _pluginCtrl, int _coordinateIndex)
// 			{
// 				return Traverse.Create(_pluginCtrl).Method("GetCoodinateName", new object[]
// 				{
// 					_coordinateIndex
// 				}).GetValue<string>();
// 			}
//
// 			// Token: 0x06000057 RID: 87 RVA: 0x0000496C File Offset: 0x00002B6C
// 			internal static void MakerInit()
// 			{
// 				if (!CharacterAccessory.MoreOutfitsSupport._installed)
// 				{
// 					return;
// 				}
// 				CharacterAccessory.MoreOutfitsSupport._makerDropdownRef = null;
// 				CharacterAccessory._hooksInstance["Maker"].Patch(CharacterAccessory.MoreOutfitsSupport._instance.GetType().Assembly.GetType("KK_Plugins.MoreOutfits.MakerUI").GetMethod("UpdateMakerUI", AccessTools.all), null, new HarmonyMethod(typeof(CharacterAccessory.MoreOutfitsSupport.Hooks), "UpdateMakerUI_Postfix", null), null, null, null);
// 			}
//
// 			// Token: 0x06000058 RID: 88 RVA: 0x000049E0 File Offset: 0x00002BE0
// 			internal static void StudioInit()
// 			{
// 				if (!CharacterAccessory.MoreOutfitsSupport._installed)
// 				{
// 					return;
// 				}
// 				CharacterAccessory._hooksInstance["Studio"].Patch(CharacterAccessory.MoreOutfitsSupport._instance.GetType().Assembly.GetType("KK_Plugins.MoreOutfits.StudioUI").GetMethod("InitializeStudioUI", AccessTools.all), null, new HarmonyMethod(typeof(CharacterAccessory.MoreOutfitsSupport.Hooks), "InitializeStudioUI_Postfix", null), null, null, null);
// 			}
//
// 			// Token: 0x06000059 RID: 89 RVA: 0x00004A4C File Offset: 0x00002C4C
// 			internal static void BuildMakerDropdownRef()
// 			{
// 				ChaControl chaCtrl = Singleton<CustomBase>.Instance.chaCtrl;
// 				int? num;
// 				if (chaCtrl == null)
// 				{
// 					num = null;
// 				}
// 				else
// 				{
// 					GameObject gameObject = chaCtrl.gameObject;
// 					if (gameObject == null)
// 					{
// 						num = null;
// 					}
// 					else
// 					{
// 						CharacterAccessoryController component = gameObject.GetComponent<CharacterAccessoryController>();
// 						num = ((component != null) ? new int?(component.GetReferralIndex()) : null);
// 					}
// 				}
// 				int? num2 = num;
// 				int value = num2.Value;
// 				if (!CharacterAccessory.MoreOutfitsSupport._installed)
// 				{
// 					CharacterAccessory._makerDropdownReferral.SetValue(value);
// 					return;
// 				}
// 				if (CharacterAccessory.MoreOutfitsSupport._makerDropdownRef == null)
// 				{
// 					GameObject gameObject2 = GameObject.Find("tglCharaAcc");
// 					CharacterAccessory.MoreOutfitsSupport._makerDropdownRef = ((gameObject2 != null) ? gameObject2.GetComponentInChildren<TMP_Dropdown>(true) : null);
// 				}
// 				if (CharacterAccessory.MoreOutfitsSupport._makerDropdownRef == null)
// 				{
// 					CharacterAccessory._logger.LogError("[BuildDropdownRef] failed to get dropdown component");
// 					return;
// 				}
// 				List<string> list = CharacterAccessory._cordNames.ToList<string>();
// 				for (int i = list.Count; i < chaCtrl.chaFile.coordinate.Length; i++)
// 				{
// 					list.Add(CharacterAccessory.MoreOutfitsSupport.GetCoodinateName(chaCtrl, i));
// 				}
// 				list.Add("CharaAcc");
// 				CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.ClearOptions();
// 				CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.options.AddRange(from x in list
// 				select new TMP_Dropdown.OptionData(x));
// 				CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.value = value;
// 				CharacterAccessory.MoreOutfitsSupport._makerDropdownRef.RefreshShownValue();
// 			}
//
// 			// Token: 0x0600005A RID: 90 RVA: 0x00004BA0 File Offset: 0x00002DA0
// 			internal static void BuildStudioDropdownRef()
// 			{
// 				if (!CharacterAccessory.MoreOutfitsSupport._installed)
// 				{
// 					return;
// 				}
// 				if (CharacterAccessory.MoreOutfitsSupport._studioDropdownRef == null)
// 				{
// 					GameObject gameObject = GameObject.Find("StudioScene/Canvas Main Menu/02_Manipulate/00_Chara/01_State/Viewport/Content/CharaAcc_Items_SAPI/CustomDropdown Referral/Dropdown");
// 					CharacterAccessory.MoreOutfitsSupport._studioDropdownRef = ((gameObject != null) ? gameObject.GetComponent<Dropdown>() : null);
// 				}
// 				if (CharacterAccessory.MoreOutfitsSupport._studioDropdownRef == null)
// 				{
// 					CharacterAccessory._logger.LogError("[BuildDropdownRef] failed to get dropdown component");
// 					return;
// 				}
// 				List<Dropdown.OptionData> options = CharacterAccessory.MoreOutfitsSupport._studioDropdownRef.options;
// 				OCIChar curOCIChar = CharaStudio.CurOCIChar;
// 				ChaControl chaControl = (curOCIChar != null) ? curOCIChar.charInfo : null;
// 				if (chaControl == null)
// 				{
// 					options.RemoveRange(CharacterAccessory._cordNames.Count, options.Count - CharacterAccessory._cordNames.Count);
// 					options.Add(new Dropdown.OptionData("CharaAcc"));
// 					return;
// 				}
// 				options.RemoveRange(CharacterAccessory._cordNames.Count, options.Count - CharacterAccessory._cordNames.Count);
// 				for (int i = CharacterAccessory._cordNames.Count; i < chaControl.chaFile.coordinate.Length; i++)
// 				{
// 					if (i < CharacterAccessory._cordNames.Count)
// 					{
// 						options.Add(new Dropdown.OptionData(CharacterAccessory._cordNames[i]));
// 					}
// 					else
// 					{
// 						options.Add(new Dropdown.OptionData(CharacterAccessory.MoreOutfitsSupport.GetCoodinateName(chaControl, i)));
// 					}
// 				}
// 				options.Add(new Dropdown.OptionData("CharaAcc"));
// 			}
//
// 			// Token: 0x0400002E RID: 46
// 			private static BaseUnityPlugin _instance;
//
// 			// Token: 0x0400002F RID: 47
// 			private static bool _installed;
//
// 			// Token: 0x04000030 RID: 48
// 			internal static TMP_Dropdown _makerDropdownRef;
//
// 			// Token: 0x04000031 RID: 49
// 			internal static Dropdown _studioDropdownRef;
//
// 			// Token: 0x02000025 RID: 37
// 			internal static class Hooks
// 			{
// 				// Token: 0x060000E4 RID: 228 RVA: 0x0000699D File Offset: 0x00004B9D
// 				internal static void UpdateMakerUI_Postfix()
// 				{
// 					CharacterAccessory.MoreOutfitsSupport.BuildMakerDropdownRef();
// 				}
//
// 				// Token: 0x060000E5 RID: 229 RVA: 0x000069A4 File Offset: 0x00004BA4
// 				internal static void InitializeStudioUI_Postfix(MPCharCtrl __0)
// 				{
// 					CharacterAccessory.MoreOutfitsSupport.BuildStudioDropdownRef();
// 				}
// 			}
// 		}
//
// 		// Token: 0x0200000C RID: 12
// 		internal static class DynamicBoneEditorSupport
// 		{
// 			// Token: 0x0600005B RID: 91 RVA: 0x00004CDC File Offset: 0x00002EDC
// 			internal static void Init()
// 			{
// 				CharacterAccessory.DynamicBoneEditorSupport._instance = Toolbox.GetPluginInstance("com.deathweasel.bepinex.dynamicboneeditor");
// 				if (CharacterAccessory.DynamicBoneEditorSupport._instance != null)
// 				{
// 					CharacterAccessory.DynamicBoneEditorSupport._installed = true;
// 					CharacterAccessory._supportList.Add("DynamicBoneEditor");
// 					Assembly assembly = CharacterAccessory.DynamicBoneEditorSupport._instance.GetType().Assembly;
// 					CharacterAccessory.DynamicBoneEditorSupport._types["CharaController"] = assembly.GetType("KK_Plugins.DynamicBoneEditor.CharaController");
// 					CharacterAccessory.DynamicBoneEditorSupport._types["DynamicBoneData"] = assembly.GetType("KK_Plugins.DynamicBoneEditor.DynamicBoneData");
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.DynamicBoneEditorSupport._types["CharaController"].GetMethod("ApplyData", AccessTools.all), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_IEnumerator_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x0600005C RID: 92 RVA: 0x00004DAD File Offset: 0x00002FAD
// 			internal static CharaController GetController(ChaControl _chaCtrl)
// 			{
// 				return Plugin.GetCharaController(_chaCtrl);
// 			}
//
// 			// Token: 0x04000032 RID: 50
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000033 RID: 51
// 			internal static bool _installed = false;
//
// 			// Token: 0x04000034 RID: 52
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x02000027 RID: 39
// 			internal class UrineBag
// 			{
// 				// Token: 0x060000E9 RID: 233 RVA: 0x000069BF File Offset: 0x00004BBF
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.DynamicBoneEditorSupport.GetController(this._chaCtrl);
// 				}
//
// 				// Token: 0x060000EA RID: 234 RVA: 0x000069F2 File Offset: 0x00004BF2
// 				internal List<DynamicBoneData> GetExtDataLink()
// 				{
// 					return this._pluginCtrl.AccessoryDynamicBoneData;
// 				}
//
// 				// Token: 0x060000EB RID: 235 RVA: 0x000069FF File Offset: 0x00004BFF
// 				internal void Reset()
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 				}
//
// 				// Token: 0x060000EC RID: 236 RVA: 0x00006A14 File Offset: 0x00004C14
// 				internal List<string> Save()
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return null;
// 					}
// 					List<string> list = new List<string>();
// 					foreach (object obj in this._charaAccData)
// 					{
// 						list.Add(JSONSerializer.Serialize(typeof(DynamicBoneData), obj, false, null));
// 					}
// 					return list;
// 				}
//
// 				// Token: 0x060000ED RID: 237 RVA: 0x00006A88 File Offset: 0x00004C88
// 				internal void Load(List<string> _json)
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					foreach (string text in _json)
// 					{
// 						this._charaAccData.Add(JSONSerializer.Deserialize<DynamicBoneData>(text, null, null));
// 					}
// 				}
//
// 				// Token: 0x060000EE RID: 238 RVA: 0x00006AFC File Offset: 0x00004CFC
// 				internal void Backup()
// 				{
// 					CharacterAccessory.DynamicBoneEditorSupport.UrineBag.<>c__DisplayClass8_0 CS$<>8__locals1 = new CharacterAccessory.DynamicBoneEditorSupport.UrineBag.<>c__DisplayClass8_0();
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					List<DynamicBoneData> extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					CS$<>8__locals1._coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
// 					CharacterAccessoryController controller = CharacterAccessory.GetController(this._chaCtrl);
// 					CharacterAccessory.DynamicBoneEditorSupport.UrineBag.<>c__DisplayClass8_0 CS$<>8__locals2 = CS$<>8__locals1;
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = controller.PartsInfo;
// 					List<int> slots;
// 					if (partsInfo == null)
// 					{
// 						slots = null;
// 					}
// 					else
// 					{
// 						Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
// 						slots = ((keys != null) ? keys.ToList<int>() : null);
// 					}
// 					CS$<>8__locals2._slots = slots;
// 					this._charaAccData.AddRange(Toolbox.JsonClone<List<DynamicBoneData>>((from x in extDataLink
// 					where x.CoordinateIndex == CS$<>8__locals1._coordinateIndex && CS$<>8__locals1._slots.Contains(x.Slot)
// 					select x).ToList<DynamicBoneData>()));
// 					this._charaAccData.ForEach(delegate(DynamicBoneData x)
// 					{
// 						x.CoordinateIndex = -1;
// 					});
// 					int count = ((ICollection)extDataLink).Count;
// 					for (int i = 0; i < count; i++)
// 					{
// 						DynamicBoneData dynamicBoneData = Toolbox.JsonClone<DynamicBoneData>(extDataLink.ElementAtOrDefault(i));
// 						Traverse traverse = Traverse.Create(dynamicBoneData);
// 						if (traverse.Field("CoordinateIndex").GetValue<int>() == CS$<>8__locals1._coordinateIndex && CS$<>8__locals1._slots.IndexOf(traverse.Field("Slot").GetValue<int>()) >= 0)
// 						{
// 							traverse.Field("CoordinateIndex").SetValue(-1);
// 							this._charaAccData.Add(dynamicBoneData);
// 						}
// 					}
// 				}
//
// 				// Token: 0x060000EF RID: 239 RVA: 0x00006C50 File Offset: 0x00004E50
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					List<DynamicBoneData> extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int _coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
// 					List<DynamicBoneData> list = Toolbox.JsonClone<List<DynamicBoneData>>(this._charaAccData);
// 					list.ForEach(delegate(DynamicBoneData x)
// 					{
// 						x.CoordinateIndex = _coordinateIndex;
// 					});
// 					extDataLink.AddRange(list);
// 				}
//
// 				// Token: 0x060000F0 RID: 240 RVA: 0x00006CB1 File Offset: 0x00004EB1
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._pluginCtrl.AccessoriesCopiedEvent(null, _args);
// 				}
//
// 				// Token: 0x060000F1 RID: 241 RVA: 0x00006CC8 File Offset: 0x00004EC8
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					List<DynamicBoneData> extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					this.RemovePartsInfo(_args.DestinationSlotIndex);
// 					int _coordinateIndex = this._chaCtrl.fileStatus.coordinateType;
// 					List<DynamicBoneData> list = Toolbox.JsonClone<List<DynamicBoneData>>((from x in extDataLink
// 					where x.CoordinateIndex == _coordinateIndex && x.Slot == _args.SourceSlotIndex
// 					select x).ToList<DynamicBoneData>());
// 					list.ForEach(delegate(DynamicBoneData x)
// 					{
// 						x.Slot = _args.DestinationSlotIndex;
// 					});
// 					extDataLink.AddRange(list);
// 				}
//
// 				// Token: 0x060000F2 RID: 242 RVA: 0x00006D52 File Offset: 0x00004F52
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					if (!CharacterAccessory.DynamicBoneEditorSupport._installed)
// 					{
// 						return;
// 					}
// 					this._pluginCtrl.AccessoryKindChangeEvent(null, new AccessorySlotEventArgs(_slotIndex));
// 				}
//
// 				// Token: 0x04000088 RID: 136
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x04000089 RID: 137
// 				private readonly CharaController _pluginCtrl;
//
// 				// Token: 0x0400008A RID: 138
// 				private readonly List<DynamicBoneData> _charaAccData = new List<DynamicBoneData>();
// 			}
// 		}
//
// 		// Token: 0x0200000D RID: 13
// 		internal static class HairAccessoryCustomizerSupport
// 		{
// 			// Token: 0x0600005E RID: 94 RVA: 0x00004DD0 File Offset: 0x00002FD0
// 			internal static void Init()
// 			{
// 				CharacterAccessory.HairAccessoryCustomizerSupport._instance = Toolbox.GetPluginInstance("com.deathweasel.bepinex.hairaccessorycustomizer");
// 				if (CharacterAccessory.HairAccessoryCustomizerSupport._instance != null)
// 				{
// 					CharacterAccessory.HairAccessoryCustomizerSupport._installed = true;
// 					CharacterAccessory._supportList.Add("HairAccessoryCustomizer");
// 					Assembly assembly = CharacterAccessory.HairAccessoryCustomizerSupport._instance.GetType().Assembly;
// 					CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryController"] = assembly.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController");
// 					CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryInfo"] = assembly.GetType("KK_Plugins.HairAccessoryCustomizer+HairAccessoryController+HairAccessoryInfo");
// 					CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryController"].GetMethod("UpdateAccessories", AccessTools.all, null, new Type[]
// 					{
// 						typeof(bool)
// 					}, null), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_Prefix", null), null, null, null, null);
// 				}
// 			}
//
// 			// Token: 0x0600005F RID: 95 RVA: 0x00004EB6 File Offset: 0x000030B6
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 				{
// 					return null;
// 				}
// 				return Traverse.Create(CharacterAccessory.HairAccessoryCustomizerSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x04000035 RID: 53
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000036 RID: 54
// 			internal static bool _installed = false;
//
// 			// Token: 0x04000037 RID: 55
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x02000028 RID: 40
// 			internal class UrineBag
// 			{
// 				// Token: 0x060000F3 RID: 243 RVA: 0x00006D70 File Offset: 0x00004F70
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.HairAccessoryCustomizerSupport.GetController(this._chaCtrl);
// 					this._traverses["pluginCtrl"] = Traverse.Create(this._pluginCtrl);
// 				}
//
// 				// Token: 0x060000F4 RID: 244 RVA: 0x00006DD4 File Offset: 0x00004FD4
// 				internal object GetExtDataLink(int _coordinateIndex)
// 				{
// 					object value = this._traverses["pluginCtrl"].Field("HairAccessories").GetValue();
// 					if (value == null)
// 					{
// 						return null;
// 					}
// 					return JetPack.Extensions.RefTryGetValue(value, _coordinateIndex);
// 				}
//
// 				// Token: 0x060000F5 RID: 245 RVA: 0x00006E12 File Offset: 0x00005012
// 				internal void Reset()
// 				{
// 					this._charaAccData.Clear();
// 				}
//
// 				// Token: 0x060000F6 RID: 246 RVA: 0x00006E20 File Offset: 0x00005020
// 				internal Dictionary<int, string> Save()
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return null;
// 					}
// 					Dictionary<int, string> dictionary = new Dictionary<int, string>();
// 					foreach (KeyValuePair<int, object> keyValuePair in this._charaAccData)
// 					{
// 						CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo fakeHairAccessoryInfo = new CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo(keyValuePair.Value);
// 						dictionary[keyValuePair.Key] = JSONSerializer.Serialize(typeof(CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo), fakeHairAccessoryInfo, false, null);
// 					}
// 					return dictionary;
// 				}
//
// 				// Token: 0x060000F7 RID: 247 RVA: 0x00006EA8 File Offset: 0x000050A8
// 				internal void Load(Dictionary<int, string> _json)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					foreach (KeyValuePair<int, string> keyValuePair in _json)
// 					{
// 						CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo fakeHairAccessoryInfo = JSONSerializer.Deserialize<CharacterAccessory.HairAccessoryCustomizerSupport.UrineBag.FakeHairAccessoryInfo>(keyValuePair.Value, null, null);
// 						this._charaAccData[keyValuePair.Key] = fakeHairAccessoryInfo.Convert();
// 					}
// 				}
//
// 				// Token: 0x060000F8 RID: 248 RVA: 0x00006F30 File Offset: 0x00005130
// 				internal void Backup()
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					object extDataLink = this.GetExtDataLink(coordinateType);
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					foreach (int num in Traverse.Create(extDataLink).Property("Keys", null).GetValue<ICollection<int>>().ToList<int>())
// 					{
// 						if (CharacterAccessory.MoreAccessoriesSupport.IsHairAccessory(this._chaCtrl, num))
// 						{
// 							object obj = JetPack.Extensions.RefTryGetValue(extDataLink, num);
// 							if (obj != null)
// 							{
// 								this._charaAccData[num] = Toolbox.JsonClone(obj);
// 							}
// 						}
// 					}
// 				}
//
// 				// Token: 0x060000F9 RID: 249 RVA: 0x00006FF4 File Offset: 0x000051F4
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					object extDataLink = this.GetExtDataLink(coordinateType);
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					foreach (KeyValuePair<int, object> keyValuePair in this._charaAccData)
// 					{
// 						if (JetPack.Extensions.RefTryGetValue(extDataLink, keyValuePair.Key) != null)
// 						{
// 							CharacterAccessory.DebugMsg(4, string.Format("[HairAccessoryCustomizer][Restore][{0}][{1}] remove HairAccessoryInfo", JetPack.Extensions.GetFullName(this._chaCtrl), keyValuePair.Key));
// 							(extDataLink as IDictionary).Remove(keyValuePair.Key);
// 						}
// 						(extDataLink as IDictionary).Add(keyValuePair.Key, Toolbox.JsonClone(keyValuePair.Value));
// 					}
// 				}
//
// 				// Token: 0x060000FA RID: 250 RVA: 0x000070E0 File Offset: 0x000052E0
// 				internal void UpdateAccessories(bool _updateHairInfo = true)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("UpdateAccessories", new object[]
// 					{
// 						_updateHairInfo
// 					}).GetValue();
// 				}
//
// 				// Token: 0x060000FB RID: 251 RVA: 0x00007119 File Offset: 0x00005319
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("CopyAccessoriesHandler", new object[]
// 					{
// 						_args
// 					}).GetValue();
// 				}
//
// 				// Token: 0x060000FC RID: 252 RVA: 0x0000714D File Offset: 0x0000534D
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("TransferAccessoriesHandler", new object[]
// 					{
// 						_args
// 					}).GetValue();
// 				}
//
// 				// Token: 0x060000FD RID: 253 RVA: 0x00007181 File Offset: 0x00005381
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					if (!CharacterAccessory.HairAccessoryCustomizerSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("RemoveHairAccessoryInfo", new object[]
// 					{
// 						_slotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x0400008B RID: 139
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x0400008C RID: 140
// 				private readonly CharaCustomFunctionController _pluginCtrl;
//
// 				// Token: 0x0400008D RID: 141
// 				private readonly Dictionary<int, object> _charaAccData = new Dictionary<int, object>();
//
// 				// Token: 0x0400008E RID: 142
// 				private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
//
// 				// Token: 0x02000031 RID: 49
// 				internal class FakeHairAccessoryInfo
// 				{
// 					// Token: 0x0600013C RID: 316 RVA: 0x00008FFC File Offset: 0x000071FC
// 					public FakeHairAccessoryInfo(object _info)
// 					{
// 						Traverse traverse = Traverse.Create(_info);
// 						this.HairGloss = traverse.Field("HairGloss").GetValue<bool>();
// 						this.ColorMatch = traverse.Field("ColorMatch").GetValue<bool>();
// 						this.OutlineColor = traverse.Field("OutlineColor").GetValue<Color>();
// 						this.AccessoryColor = traverse.Field("AccessoryColor").GetValue<Color>();
// 						this.HairLength = traverse.Field("HairLength").GetValue<float>();
// 					}
//
// 					// Token: 0x0600013D RID: 317 RVA: 0x0000909C File Offset: 0x0000729C
// 					public object Convert()
// 					{
// 						object obj = Activator.CreateInstance(CharacterAccessory.HairAccessoryCustomizerSupport._types["HairAccessoryInfo"]);
// 						Traverse traverse = Traverse.Create(obj);
// 						traverse.Field<bool>("HairGloss").Value = this.HairGloss;
// 						traverse.Field<bool>("ColorMatch").Value = this.ColorMatch;
// 						traverse.Field<Color>("OutlineColor").Value = this.OutlineColor;
// 						traverse.Field<Color>("AccessoryColor").Value = this.AccessoryColor;
// 						traverse.Field<float>("HairLength").Value = this.HairLength;
// 						return obj;
// 					}
//
// 					// Token: 0x040000A7 RID: 167
// 					public bool HairGloss;
//
// 					// Token: 0x040000A8 RID: 168
// 					public bool ColorMatch;
//
// 					// Token: 0x040000A9 RID: 169
// 					public Color OutlineColor = Color.white;
//
// 					// Token: 0x040000AA RID: 170
// 					public Color AccessoryColor = Color.white;
//
// 					// Token: 0x040000AB RID: 171
// 					public float HairLength;
// 				}
// 			}
// 		}
//
// 		// Token: 0x0200000E RID: 14
// 		internal static class MaterialEditorSupport
// 		{
// 			// Token: 0x06000061 RID: 97 RVA: 0x00004EFC File Offset: 0x000030FC
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("com.deathweasel.bepinex.materialeditor", out pluginInfo);
// 				CharacterAccessory.MaterialEditorSupport._instance = pluginInfo.Instance;
// 				CharacterAccessory._supportList.Add("MaterialEditor");
// 				Assembly assembly = CharacterAccessory.MaterialEditorSupport._instance.GetType().Assembly;
// 				CharacterAccessory.MaterialEditorSupport._types["MaterialAPI"] = assembly.GetType("MaterialEditorAPI.MaterialAPI");
// 				CharacterAccessory.MaterialEditorSupport._types["MaterialEditorCharaController"] = assembly.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController");
// 				CharacterAccessory.MaterialEditorSupport._types["ObjectType"] = assembly.GetType("KK_Plugins.MaterialEditor.MaterialEditorCharaController+ObjectType");
// 				CharacterAccessory.MaterialEditorSupport._legacy = (pluginInfo.Metadata.Version.CompareTo(new Version("3.0")) < 0);
// 				if (CharacterAccessory.MaterialEditorSupport._legacy)
// 				{
// 					CharacterAccessory._logger.LogWarning(string.Format("Material Editor version {0} found, running in legacy mode", pluginInfo.Metadata.Version));
// 				}
// 				else
// 				{
// 					CharacterAccessory.MaterialEditorSupport._containerKeys.Add("MaterialCopyList");
// 				}
// 				CharacterAccessory._hooksInstance["General"].Patch(CharacterAccessory.MaterialEditorSupport._types["MaterialEditorCharaController"].GetMethod("LoadData", AccessTools.all, null, new Type[]
// 				{
// 					typeof(bool),
// 					typeof(bool),
// 					typeof(bool)
// 				}, null), new HarmonyMethod(typeof(CharacterAccessory.Hooks), "DuringLoading_IEnumerator_Prefix", null), null, null, null, null);
// 			}
//
// 			// Token: 0x06000062 RID: 98 RVA: 0x00005067 File Offset: 0x00003267
// 			internal static MaterialEditorCharaController GetController(ChaControl _chaCtrl)
// 			{
// 				if (_chaCtrl == null)
// 				{
// 					return null;
// 				}
// 				GameObject gameObject = _chaCtrl.gameObject;
// 				if (gameObject == null)
// 				{
// 					return null;
// 				}
// 				return gameObject.GetComponent<MaterialEditorCharaController>();
// 			}
//
// 			// Token: 0x04000038 RID: 56
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000039 RID: 57
// 			internal static bool _legacy = false;
//
// 			// Token: 0x0400003A RID: 58
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x0400003B RID: 59
// 			private static readonly List<string> _containerKeys = new List<string>
// 			{
// 				"RendererPropertyList",
// 				"MaterialShaderList",
// 				"MaterialFloatPropertyList",
// 				"MaterialColorPropertyList",
// 				"MaterialTexturePropertyList"
// 			};
//
// 			// Token: 0x02000029 RID: 41
// 			internal class UrineBag
// 			{
// 				// Token: 0x060000FE RID: 254 RVA: 0x000071BC File Offset: 0x000053BC
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.MaterialEditorSupport.GetController(this._chaCtrl);
// 					foreach (string text in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						string name = "KK_Plugins.MaterialEditor.MaterialEditorCharaController+" + text.Replace("List", "");
// 						Type type = CharacterAccessory.MaterialEditorSupport._instance.GetType().Assembly.GetType(name);
// 						Type type2 = typeof(List<>).MakeGenericType(new Type[]
// 						{
// 							type
// 						});
// 						this._charaAccData[text] = Activator.CreateInstance(type2);
// 					}
// 				}
//
// 				// Token: 0x060000FF RID: 255 RVA: 0x000072A8 File Offset: 0x000054A8
// 				internal void Reset()
// 				{
// 					foreach (string text in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						this._extdataLink[text] = Traverse.Create(this._pluginCtrl).Field(text).GetValue();
// 						Traverse.Create(this._charaAccData[text]).Method("Clear", Array.Empty<object>()).GetValue();
// 					}
// 					this._texData.Clear();
// 				}
//
// 				// Token: 0x06000100 RID: 256 RVA: 0x00007348 File Offset: 0x00005548
// 				internal Dictionary<string, string> Save()
// 				{
// 					Dictionary<string, string> dictionary = new Dictionary<string, string>();
// 					foreach (string key in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						dictionary[key] = JSONSerializer.Serialize(this._charaAccData[key].GetType(), this._charaAccData[key], false, null);
// 					}
// 					return dictionary;
// 				}
//
// 				// Token: 0x06000101 RID: 257 RVA: 0x000073C8 File Offset: 0x000055C8
// 				internal void Load(Dictionary<string, string> _json)
// 				{
// 					this.Reset();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					foreach (string key in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						if (_json.ContainsKey(key) && _json[key] != null)
// 						{
// 							this._charaAccData[key] = JSONSerializer.Deserialize(this._charaAccData[key].GetType(), _json[key], null, null);
// 						}
// 					}
// 				}
//
// 				// Token: 0x06000102 RID: 258 RVA: 0x0000745C File Offset: 0x0000565C
// 				internal void Backup()
// 				{
// 					this.Reset();
// 					CharacterAccessoryController controller = CharacterAccessory.GetController(this._chaCtrl);
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					List<int> list = controller.PartsInfo.Keys.ToList<int>();
// 					foreach (string text in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						int value = Traverse.Create(this._extdataLink[text]).Property("Count", null).GetValue<int>();
// 						CharacterAccessory.DebugMsg(4, string.Format("[MaterialEditor][Backup][{0}][_extdataLink[{1}] count: {2}]", JetPack.Extensions.GetFullName(this._chaCtrl), text, value));
// 						for (int i = 0; i < value; i++)
// 						{
// 							object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(this._extdataLink[text], i));
// 							Traverse traverse = Traverse.Create(obj);
// 							if (!(traverse.Field("ObjectType").Method("ToString", Array.Empty<object>()).GetValue<string>() != "Accessory") && traverse.Field("CoordinateIndex").GetValue<int>() == coordinateType && list.IndexOf(traverse.Field("Slot").GetValue<int>()) >= 0)
// 							{
// 								traverse.Field("CoordinateIndex").SetValue(-1);
// 								(this._charaAccData[text] as IList).Add(obj);
// 							}
// 						}
// 					}
// 					foreach (MaterialEditorCharaController.MaterialTextureProperty materialTextureProperty in (this._charaAccData["MaterialTexturePropertyList"] as List<MaterialEditorCharaController.MaterialTextureProperty>))
// 					{
// 						if (materialTextureProperty.TexID != null)
// 						{
// 							int value2 = materialTextureProperty.TexID.Value;
// 							if (!this._texData.ContainsKey(value2))
// 							{
// 								TextureContainer textureContainer;
// 								this._pluginCtrl.TextureDictionary.TryGetValue(value2, out textureContainer);
// 								if (textureContainer != null)
// 								{
// 									this._texData[value2] = textureContainer.Data;
// 									CharacterAccessory.DebugMsg(4, string.Format("[TexID: {0}][Length: {1}]", value2, this._texData[value2].Length));
// 								}
// 							}
// 						}
// 					}
// 				}
//
// 				// Token: 0x06000103 RID: 259 RVA: 0x000076D8 File Offset: 0x000058D8
// 				internal void Restore()
// 				{
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					Dictionary<int, int> dictionary = new Dictionary<int, int>();
// 					foreach (KeyValuePair<int, byte[]> keyValuePair in this._texData)
// 					{
// 						dictionary[keyValuePair.Key] = this._pluginCtrl.SetAndGetTextureID(keyValuePair.Value);
// 					}
// 					foreach (string text in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						int value = Traverse.Create(this._charaAccData[text]).Property("Count", null).GetValue<int>();
// 						for (int i = 0; i < value; i++)
// 						{
// 							object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(this._charaAccData[text], i));
// 							Traverse traverse = Traverse.Create(obj);
// 							traverse.Field("CoordinateIndex").SetValue(coordinateType);
// 							if (text == "MaterialTexturePropertyList")
// 							{
// 								int? value2 = traverse.Field("TexID").GetValue<int?>();
// 								if (value2 != null)
// 								{
// 									traverse.Field("TexID").SetValue(dictionary[value2.Value]);
// 								}
// 							}
// 							(this._extdataLink[text] as IList).Add(obj);
// 						}
// 					}
// 				}
//
// 				// Token: 0x06000104 RID: 260 RVA: 0x0000787C File Offset: 0x00005A7C
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					this._pluginCtrl.AccessoriesCopiedEvent(null, _args);
// 				}
//
// 				// Token: 0x06000105 RID: 261 RVA: 0x0000788C File Offset: 0x00005A8C
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					this.RemovePartsInfo(_args.DestinationSlotIndex);
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					foreach (string key in CharacterAccessory.MaterialEditorSupport._containerKeys)
// 					{
// 						int value = Traverse.Create(this._extdataLink[key]).Property("Count", null).GetValue<int>();
// 						for (int i = 0; i < value; i++)
// 						{
// 							object obj = this.MoveSlot(Toolbox.JsonClone(JetPack.Extensions.RefElementAt(this._extdataLink[key], i)), coordinateType, _args.SourceSlotIndex, _args.DestinationSlotIndex);
// 							if (obj != null)
// 							{
// 								(this._extdataLink[key] as IList).Add(obj);
// 							}
// 						}
// 					}
// 				}
//
// 				// Token: 0x06000106 RID: 262 RVA: 0x00007978 File Offset: 0x00005B78
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					this._pluginCtrl.AccessoryKindChangeEvent(null, new AccessorySlotEventArgs(_slotIndex));
// 				}
//
// 				// Token: 0x1700001E RID: 30
// 				// (get) Token: 0x06000107 RID: 263 RVA: 0x0000798C File Offset: 0x00005B8C
// 				// (set) Token: 0x06000108 RID: 264 RVA: 0x00007994 File Offset: 0x00005B94
// 				internal Dictionary<int, byte[]> TexContainer
// 				{
// 					get
// 					{
// 						return this._texData;
// 					}
// 					set
// 					{
// 						this._texData = value;
// 					}
// 				}
//
// 				// Token: 0x06000109 RID: 265 RVA: 0x000079A0 File Offset: 0x00005BA0
// 				private object MoveSlot(object _obj, int _coordinateIndex, int _srcSlotIndex, int _dstSlotIndex)
// 				{
// 					if (_obj == null)
// 					{
// 						return null;
// 					}
// 					Traverse traverse = Traverse.Create(_obj);
// 					if (traverse.Field("ObjectType").Method("ToString", Array.Empty<object>()).GetValue<string>() != "Accessory")
// 					{
// 						return null;
// 					}
// 					if (traverse.Field("CoordinateIndex").GetValue<int>() != _coordinateIndex)
// 					{
// 						return null;
// 					}
// 					if (traverse.Field("Slot").GetValue<int>() != _srcSlotIndex)
// 					{
// 						return null;
// 					}
// 					traverse.Field("Slot").SetValue(_dstSlotIndex);
// 					return _obj;
// 				}
//
// 				// Token: 0x0400008F RID: 143
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x04000090 RID: 144
// 				private readonly MaterialEditorCharaController _pluginCtrl;
//
// 				// Token: 0x04000091 RID: 145
// 				private readonly Dictionary<string, object> _extdataLink = new Dictionary<string, object>();
//
// 				// Token: 0x04000092 RID: 146
// 				private readonly Dictionary<string, object> _charaAccData = new Dictionary<string, object>();
//
// 				// Token: 0x04000093 RID: 147
// 				private Dictionary<int, byte[]> _texData = new Dictionary<int, byte[]>();
// 			}
// 		}
//
// 		// Token: 0x0200000F RID: 15
// 		internal static class MaterialRouterSupport
// 		{
// 			// Token: 0x06000064 RID: 100 RVA: 0x000050E4 File Offset: 0x000032E4
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("madevil.kk.mr", out pluginInfo);
// 				CharacterAccessory.MaterialRouterSupport._instance = ((pluginInfo != null) ? pluginInfo.Instance : null);
// 				if (CharacterAccessory.MaterialRouterSupport._instance != null)
// 				{
// 					if (pluginInfo.Metadata.Version.CompareTo(new Version("2.0.0.0")) < 0)
// 					{
// 						CharacterAccessory._logger.LogError(string.Format("Material Router version {0} found, minimun version 2 is reqired", pluginInfo.Metadata.Version));
// 						return;
// 					}
// 					CharacterAccessory.MaterialRouterSupport._installed = true;
// 					CharacterAccessory._supportList.Add("MaterialRouter");
// 					Assembly assembly = CharacterAccessory.MaterialRouterSupport._instance.GetType().Assembly;
// 					CharacterAccessory.MaterialRouterSupport._types["MaterialRouterController"] = assembly.GetType("MaterialRouter.MaterialRouter+MaterialRouterController");
// 					CharacterAccessory.MaterialRouterSupport._types["RouteRule"] = assembly.GetType("MaterialRouter.MaterialRouter+RouteRule");
// 					CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"] = assembly.GetType("MaterialRouter.MaterialRouter+RouteRuleV1");
// 				}
// 			}
//
// 			// Token: 0x06000065 RID: 101 RVA: 0x000051D4 File Offset: 0x000033D4
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				if (!CharacterAccessory.MaterialRouterSupport._installed)
// 				{
// 					return null;
// 				}
// 				return Traverse.Create(CharacterAccessory.MaterialRouterSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x0400003C RID: 60
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x0400003D RID: 61
// 			internal static bool _installed = false;
//
// 			// Token: 0x0400003E RID: 62
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x0200002A RID: 42
// 			internal class UrineBag
// 			{
// 				// Token: 0x0600010A RID: 266 RVA: 0x00007A2C File Offset: 0x00005C2C
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.MaterialRouterSupport.GetController(this._chaCtrl);
// 					this._traverses["pluginCtrl"] = Traverse.Create(this._pluginCtrl);
// 				}
//
// 				// Token: 0x0600010B RID: 267 RVA: 0x00007A90 File Offset: 0x00005C90
// 				internal object GetExtDataLink()
// 				{
// 					return this._traverses["pluginCtrl"].Field("RouteRuleList").GetValue();
// 				}
//
// 				// Token: 0x0600010C RID: 268 RVA: 0x00007AB1 File Offset: 0x00005CB1
// 				internal void Reset()
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 				}
//
// 				// Token: 0x0600010D RID: 269 RVA: 0x00007AC8 File Offset: 0x00005CC8
// 				internal List<string> Save()
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return null;
// 					}
// 					List<string> list = new List<string>();
// 					foreach (object obj in this._charaAccData)
// 					{
// 						list.Add(JSONSerializer.Serialize(CharacterAccessory.MaterialRouterSupport._types["RouteRule"], obj, false, null));
// 					}
// 					return list;
// 				}
//
// 				// Token: 0x0600010E RID: 270 RVA: 0x00007B44 File Offset: 0x00005D44
// 				internal void Load(List<string> _json)
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					List<object> charaAccData = this._charaAccData;
// 					if (charaAccData != null)
// 					{
// 						charaAccData.Clear();
// 					}
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					bool flag = false;
// 					Type type = typeof(List<>).MakeGenericType(new Type[]
// 					{
// 						CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"]
// 					});
// 					object obj = Activator.CreateInstance(type);
// 					foreach (string text in _json)
// 					{
// 						if (text.IndexOf("GameObjectPath") > -1)
// 						{
// 							flag = true;
// 							(obj as IList).Add(JSONSerializer.Deserialize(CharacterAccessory.MaterialRouterSupport._types["RouteRuleV1"], text, null, null));
// 						}
// 						else
// 						{
// 							this._charaAccData.Add(JSONSerializer.Deserialize(CharacterAccessory.MaterialRouterSupport._types["RouteRule"], text, null, null));
// 						}
// 					}
// 					if (flag)
// 					{
// 						CharacterAccessory.DebugMsg(4, "[MaterialRouterSupport][Migration]");
// 						foreach (object item in (Traverse.Create(CharacterAccessory.MaterialRouterSupport._instance).Method("MigrationV1", new Type[]
// 						{
// 							type
// 						}, new object[]
// 						{
// 							obj
// 						}).GetValue() as IList))
// 						{
// 							this._charaAccData.Add(item);
// 						}
// 					}
// 				}
//
// 				// Token: 0x0600010F RID: 271 RVA: 0x00007CC0 File Offset: 0x00005EC0
// 				internal void Backup()
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					this._charaAccData.Clear();
// 					CharacterAccessoryController controller = CharacterAccessory.GetController(this._chaCtrl);
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = controller.PartsInfo;
// 					List<int> list;
// 					if (partsInfo == null)
// 					{
// 						list = null;
// 					}
// 					else
// 					{
// 						Dictionary<int, ChaFileAccessory.PartsInfo>.KeyCollection keys = partsInfo.Keys;
// 						list = ((keys != null) ? keys.ToList<int>() : null);
// 					}
// 					List<int> list2 = list;
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					int count = (extDataLink as IList).Count;
// 					for (int i = 0; i < count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(extDataLink, i));
// 						Traverse traverse = Traverse.Create(obj);
// 						if (!(traverse.Property("ObjectType", null).Method("ToString", Array.Empty<object>()).GetValue<string>() != "Accessory") && traverse.Property("Coordinate", null).GetValue<int>() == coordinateType)
// 						{
// 							int item = int.Parse(traverse.Property("GameObjectName", null).GetValue<string>().Replace("ca_slot", ""));
// 							if (list2.Contains(item))
// 							{
// 								traverse.Property("Coordinate", null).SetValue(-1);
// 								((IList)this._charaAccData).Add(obj);
// 							}
// 						}
// 					}
// 				}
//
// 				// Token: 0x06000110 RID: 272 RVA: 0x00007DFC File Offset: 0x00005FFC
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					object extDataLink = this.GetExtDataLink();
// 					if (extDataLink == null)
// 					{
// 						return;
// 					}
// 					for (int i = 0; i < this._charaAccData.Count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(this._charaAccData[i]);
// 						Traverse.Create(obj).Property("Coordinate", null).SetValue(this._chaCtrl.fileStatus.coordinateType);
// 						(extDataLink as IList).Add(obj);
// 					}
// 				}
//
// 				// Token: 0x06000111 RID: 273 RVA: 0x00007E8E File Offset: 0x0000608E
// 				internal string Report()
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return "";
// 					}
// 					return JSONSerializer.Serialize(this._charaAccData.GetType(), this._charaAccData, true, null);
// 				}
//
// 				// Token: 0x06000112 RID: 274 RVA: 0x00007EB5 File Offset: 0x000060B5
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("AccessoryCopyEvent", new object[]
// 					{
// 						_args
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000113 RID: 275 RVA: 0x00007EEC File Offset: 0x000060EC
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					this._traverses["pluginCtrl"].Method("TransferAccSlotInfo", new object[]
// 					{
// 						coordinateType,
// 						_args
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000114 RID: 276 RVA: 0x00007F48 File Offset: 0x00006148
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					if (!CharacterAccessory.MaterialRouterSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					this._traverses["pluginCtrl"].Method("RemoveAccSlotInfo", new object[]
// 					{
// 						coordinateType,
// 						_slotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x04000094 RID: 148
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x04000095 RID: 149
// 				private readonly CharaCustomFunctionController _pluginCtrl;
//
// 				// Token: 0x04000096 RID: 150
// 				private readonly List<object> _charaAccData = new List<object>();
//
// 				// Token: 0x04000097 RID: 151
// 				private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
// 			}
// 		}
//
// 		// Token: 0x02000010 RID: 16
// 		internal static class AccStateSyncSupport
// 		{
// 			// Token: 0x06000067 RID: 103 RVA: 0x0000521C File Offset: 0x0000341C
// 			internal static void Init()
// 			{
// 				PluginInfo pluginInfo;
// 				Chainloader.PluginInfos.TryGetValue("madevil.kk.ass", out pluginInfo);
// 				CharacterAccessory.AccStateSyncSupport._instance = ((pluginInfo != null) ? pluginInfo.Instance : null);
// 				if (CharacterAccessory.AccStateSyncSupport._instance != null)
// 				{
// 					CharacterAccessory.AccStateSyncSupport._legacy = (pluginInfo.Metadata.Version.CompareTo(new Version("4.0.0.0")) < 0);
// 					if (CharacterAccessory.AccStateSyncSupport._legacy)
// 					{
// 						CharacterAccessory._logger.LogError(string.Format("AccStateSync version {0} found, minimun version 4 is reqired", pluginInfo.Metadata.Version));
// 						return;
// 					}
// 					CharacterAccessory.AccStateSyncSupport._installed = true;
// 					CharacterAccessory._supportList.Add("AccStateSync");
// 					Assembly assembly = CharacterAccessory.AccStateSyncSupport._instance.GetType().Assembly;
// 					CharacterAccessory.AccStateSyncSupport._types["AccStateSyncController"] = assembly.GetType("AccStateSync.AccStateSync+AccStateSyncController");
// 					CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"] = assembly.GetType("AccStateSync.AccStateSync+TriggerProperty");
// 					CharacterAccessory.AccStateSyncSupport._types["TriggerGroup"] = assembly.GetType("AccStateSync.AccStateSync+TriggerGroup");
// 					foreach (object obj in Enum.GetValues(typeof(ChaAccessoryDefine.AccessoryParentKey)))
// 					{
// 						CharacterAccessory.AccStateSyncSupport._accParentNames[obj.ToString()] = ChaAccessoryDefine.dictAccessoryParent[(int)obj];
// 					}
// 				}
// 			}
//
// 			// Token: 0x06000068 RID: 104 RVA: 0x00005384 File Offset: 0x00003584
// 			internal static CharaCustomFunctionController GetController(ChaControl _chaCtrl)
// 			{
// 				if (!CharacterAccessory.AccStateSyncSupport._installed)
// 				{
// 					return null;
// 				}
// 				return Traverse.Create(CharacterAccessory.AccStateSyncSupport._instance).Method("GetController", new object[]
// 				{
// 					_chaCtrl
// 				}).GetValue<CharaCustomFunctionController>();
// 			}
//
// 			// Token: 0x0400003F RID: 63
// 			internal static BaseUnityPlugin _instance = null;
//
// 			// Token: 0x04000040 RID: 64
// 			internal static bool _installed = false;
//
// 			// Token: 0x04000041 RID: 65
// 			internal static bool _legacy = false;
//
// 			// Token: 0x04000042 RID: 66
// 			internal static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
//
// 			// Token: 0x04000043 RID: 67
// 			internal static readonly List<string> _containerKeys = new List<string>
// 			{
// 				"TriggerPropertyList",
// 				"TriggerGroupList"
// 			};
//
// 			// Token: 0x04000044 RID: 68
// 			internal static readonly Dictionary<string, string> _accParentNames = new Dictionary<string, string>();
//
// 			// Token: 0x04000045 RID: 69
// 			internal static readonly Dictionary<string, int> _guidMapping = new Dictionary<string, int>();
//
// 			// Token: 0x0200002B RID: 43
// 			internal class UrineBag
// 			{
// 				// Token: 0x06000115 RID: 277 RVA: 0x00007FA8 File Offset: 0x000061A8
// 				internal UrineBag(ChaControl ChaControl)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._chaCtrl = ChaControl;
// 					this._pluginCtrl = CharacterAccessory.AccStateSyncSupport.GetController(this._chaCtrl);
// 					this._traverses["pluginCtrl"] = Traverse.Create(this._pluginCtrl);
// 					foreach (string text in CharacterAccessory.AccStateSyncSupport._containerKeys)
// 					{
// 						Type type = CharacterAccessory.AccStateSyncSupport._types[text.Replace("List", "")];
// 						Type type2 = typeof(List<>).MakeGenericType(new Type[]
// 						{
// 							type
// 						});
// 						this._charaAccData[text] = Activator.CreateInstance(type2);
// 						CharacterAccessory.AccStateSyncSupport._types[text] = type2;
// 					}
// 				}
//
// 				// Token: 0x06000116 RID: 278 RVA: 0x000080A0 File Offset: 0x000062A0
// 				internal object GetTriggerPropertyList()
// 				{
// 					return this._traverses["pluginCtrl"].Field("TriggerPropertyList").GetValue();
// 				}
//
// 				// Token: 0x06000117 RID: 279 RVA: 0x000080C1 File Offset: 0x000062C1
// 				internal object GetTriggerGroupList()
// 				{
// 					return this._traverses["pluginCtrl"].Field("TriggerGroupList").GetValue();
// 				}
//
// 				// Token: 0x06000118 RID: 280 RVA: 0x000080E4 File Offset: 0x000062E4
// 				internal void Reset()
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					(this._charaAccData["TriggerPropertyList"] as IList).Clear();
// 					(this._charaAccData["TriggerGroupList"] as IList).Clear();
// 					CharacterAccessory.AccStateSyncSupport._guidMapping.Clear();
// 				}
//
// 				// Token: 0x06000119 RID: 281 RVA: 0x00008138 File Offset: 0x00006338
// 				internal Dictionary<string, string> Save()
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return null;
// 					}
// 					Dictionary<string, string> dictionary = new Dictionary<string, string>();
// 					foreach (string key in CharacterAccessory.AccStateSyncSupport._containerKeys)
// 					{
// 						dictionary[key] = JSONSerializer.Serialize(CharacterAccessory.AccStateSyncSupport._types[key], this._charaAccData[key], false, null);
// 					}
// 					return dictionary;
// 				}
//
// 				// Token: 0x0600011A RID: 282 RVA: 0x000081B8 File Offset: 0x000063B8
// 				internal void Migrate(Dictionary<int, string> _json)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this.Reset();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					List<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo> list = new List<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo>();
// 					int num = -1;
// 					int num2 = 9;
// 					Dictionary<string, int> dictionary = new Dictionary<string, int>();
// 					foreach (string text in _json.Values)
// 					{
// 						CharacterAccessory.AccStateSyncSupport.AccTriggerInfo accTriggerInfo = JSONSerializer.Deserialize<CharacterAccessory.AccStateSyncSupport.AccTriggerInfo>(text, null, null);
// 						list.Add(accTriggerInfo);
// 						if (accTriggerInfo.Kind >= 9)
// 						{
// 							dictionary[accTriggerInfo.Group] = accTriggerInfo.Kind;
// 						}
// 					}
// 					dictionary = (from x in dictionary
// 					orderby x.Value, x.Key
// 					select x).ToDictionary((KeyValuePair<string, int> x) => x.Key, (KeyValuePair<string, int> x) => x.Value);
// 					Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
// 					foreach (string key in dictionary.Keys)
// 					{
// 						dictionary2[key] = num2;
// 						string text2 = CharacterAccessory.AccStateSyncSupport._accParentNames.ContainsKey(key) ? CharacterAccessory.AccStateSyncSupport._accParentNames[key] : "";
// 						object value = Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerGroup"], new object[]
// 						{
// 							num,
// 							num2,
// 							text2
// 						});
// 						(this._charaAccData["TriggerGroupList"] as IList).Add(value);
// 						num2++;
// 					}
// 					foreach (CharacterAccessory.AccStateSyncSupport.AccTriggerInfo accTriggerInfo2 in list)
// 					{
// 						if (accTriggerInfo2.Kind >= 9)
// 						{
// 							accTriggerInfo2.Kind = dictionary2[accTriggerInfo2.Group];
// 							object value2 = Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], new object[]
// 							{
// 								num,
// 								accTriggerInfo2.Slot,
// 								accTriggerInfo2.Kind,
// 								0,
// 								accTriggerInfo2.State[0],
// 								0
// 							});
// 							(this._charaAccData["TriggerPropertyList"] as IList).Add(value2);
// 							object value3 = Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], new object[]
// 							{
// 								num,
// 								accTriggerInfo2.Slot,
// 								accTriggerInfo2.Kind,
// 								1,
// 								accTriggerInfo2.State[3],
// 								0
// 							});
// 							(this._charaAccData["TriggerPropertyList"] as IList).Add(value3);
// 						}
// 						else
// 						{
// 							for (int i = 0; i <= 3; i++)
// 							{
// 								object value4 = Activator.CreateInstance(CharacterAccessory.AccStateSyncSupport._types["TriggerProperty"], new object[]
// 								{
// 									num,
// 									accTriggerInfo2.Slot,
// 									accTriggerInfo2.Kind,
// 									i,
// 									accTriggerInfo2.State[i],
// 									0
// 								});
// 								(this._charaAccData["TriggerPropertyList"] as IList).Add(value4);
// 							}
// 						}
// 					}
// 					for (int j = 0; j < (this._charaAccData["TriggerGroupList"] as IList).Count; j++)
// 					{
// 						Traverse traverse = Traverse.Create(JetPack.Extensions.RefElementAt(this._charaAccData["TriggerGroupList"], j));
// 						int value5 = traverse.Property("Kind", null).GetValue<int>();
// 						string value6 = traverse.Property("GUID", null).GetValue<string>();
// 						CharacterAccessory.AccStateSyncSupport._guidMapping[value6] = value5;
// 					}
// 				}
//
// 				// Token: 0x0600011B RID: 283 RVA: 0x00008660 File Offset: 0x00006860
// 				internal void Load(Dictionary<string, string> _json)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this.Reset();
// 					if (_json == null)
// 					{
// 						return;
// 					}
// 					if (!_json.ContainsKey("TriggerPropertyList"))
// 					{
// 						return;
// 					}
// 					this._charaAccData["TriggerPropertyList"] = JSONSerializer.Deserialize(this._charaAccData["TriggerPropertyList"].GetType(), _json["TriggerPropertyList"], null, null);
// 					this._charaAccData["TriggerGroupList"] = JSONSerializer.Deserialize(this._charaAccData["TriggerGroupList"].GetType(), _json["TriggerGroupList"], null, null);
// 					for (int i = 0; i < (this._charaAccData["TriggerGroupList"] as IList).Count; i++)
// 					{
// 						Traverse traverse = Traverse.Create(JetPack.Extensions.RefElementAt(this._charaAccData["TriggerGroupList"], i));
// 						int value = traverse.Property("Kind", null).GetValue<int>();
// 						string value2 = traverse.Property("GUID", null).GetValue<string>();
// 						CharacterAccessory.AccStateSyncSupport._guidMapping[value2] = value;
// 					}
// 				}
//
// 				// Token: 0x0600011C RID: 284 RVA: 0x0000876C File Offset: 0x0000696C
// 				internal void Backup()
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this.Reset();
// 					this.RefreshCache();
// 					object value = this._traverses["pluginCtrl"].Field("_cachedCoordinatePropertyList").GetValue();
// 					object value2 = this._traverses["pluginCtrl"].Field("_cachedCoordinateGroupList").GetValue();
// 					if (value == null)
// 					{
// 						return;
// 					}
// 					Dictionary<int, ChaFileAccessory.PartsInfo> partsInfo = CharacterAccessory.GetController(this._chaCtrl).PartsInfo;
// 					HashSet<int> hashSet = new HashSet<int>((partsInfo != null) ? partsInfo.Keys : null);
// 					for (int i = 0; i < (value as IList).Count; i++)
// 					{
// 						object obj = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(value, i));
// 						Traverse traverse = Traverse.Create(obj);
// 						if (hashSet.Contains(traverse.Property("Slot", null).GetValue<int>()))
// 						{
// 							traverse.Property("Coordinate", null).SetValue(-1);
// 							(this._charaAccData["TriggerPropertyList"] as IList).Add(obj);
// 						}
// 					}
// 					for (int j = 0; j < (value2 as IList).Count; j++)
// 					{
// 						object obj2 = Toolbox.JsonClone(JetPack.Extensions.RefElementAt(value2, j));
// 						Traverse traverse2 = Traverse.Create(obj2);
// 						traverse2.Property("Coordinate", null).SetValue(-1);
// 						(this._charaAccData["TriggerGroupList"] as IList).Add(obj2);
// 						int value3 = traverse2.Property("Kind", null).GetValue<int>();
// 						string value4 = traverse2.Property("GUID", null).GetValue<string>();
// 						CharacterAccessory.AccStateSyncSupport._guidMapping[value4] = value3;
// 					}
// 				}
//
// 				// Token: 0x0600011D RID: 285 RVA: 0x00008910 File Offset: 0x00006B10
// 				internal void Restore()
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					object value = this._traverses["pluginCtrl"].Field("TriggerPropertyList").GetValue();
// 					object value2 = this._traverses["pluginCtrl"].Field("TriggerGroupList").GetValue();
// 					if (value == null)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					Dictionary<int, int> dictionary = new Dictionary<int, int>();
// 					foreach (string text in CharacterAccessory.AccStateSyncSupport._guidMapping.Keys)
// 					{
// 						object obj = null;
// 						foreach (object obj2 in (this._charaAccData["TriggerGroupList"] as IList))
// 						{
// 							if (!(Traverse.Create(obj2).Property("GUID", null).GetValue<string>() != text))
// 							{
// 								obj = Toolbox.JsonClone(obj2);
// 							}
// 						}
// 						if (obj == null)
// 						{
// 							CharacterAccessory.DebugMsg((LogLevel)2, "[Restore] cannot find group setting for " + text);
// 						}
// 						else
// 						{
// 							Traverse traverse = Traverse.Create(obj);
// 							traverse.Property("Coordinate", null).SetValue(coordinateType);
// 							bool triggerGroupByGUID = this.GetTriggerGroupByGUID(coordinateType, text) != null;
// 							int key = CharacterAccessory.AccStateSyncSupport._guidMapping[text];
// 							if (!triggerGroupByGUID)
// 							{
// 								int nextGroupID = this.GetNextGroupID(coordinateType);
// 								dictionary[key] = nextGroupID;
// 								traverse.Property("Kind", null).SetValue(nextGroupID);
// 							}
// 							(value2 as IList).Add(obj);
// 						}
// 					}
// 					foreach (object obj3 in (this._charaAccData["TriggerPropertyList"] as IList))
// 					{
// 						object obj4 = Toolbox.JsonClone(obj3);
// 						Traverse traverse2 = Traverse.Create(obj4);
// 						traverse2.Property("Coordinate", null).SetValue(coordinateType);
// 						int value3 = traverse2.Property("RefKind", null).GetValue<int>();
// 						if (dictionary.ContainsKey(value3))
// 						{
// 							traverse2.Property("RefKind", null).SetValue(dictionary[value3]);
// 						}
// 						(value as IList).Add(obj4);
// 					}
// 					this.RefreshCache();
// 				}
//
// 				// Token: 0x0600011E RID: 286 RVA: 0x00008BCC File Offset: 0x00006DCC
// 				internal void CopyPartsInfo(AccessoryCopyEventArgs _args)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					foreach (int num in _args.CopiedSlotIndexes)
// 					{
// 						this._traverses["pluginCtrl"].Method("CloneSlotTriggerProperty", new object[]
// 						{
// 							num,
// 							num,
// 							(int)_args.CopySource,
// 							(int)_args.CopyDestination
// 						}).GetValue();
// 					}
// 				}
//
// 				// Token: 0x0600011F RID: 287 RVA: 0x00008C70 File Offset: 0x00006E70
// 				internal void TransferPartsInfo(AccessoryTransferEventArgs _args)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					int coordinateType = this._chaCtrl.fileStatus.coordinateType;
// 					this._traverses["pluginCtrl"].Method("CloneSlotTriggerProperty", new object[]
// 					{
// 						_args.SourceSlotIndex,
// 						_args.DestinationSlotIndex,
// 						coordinateType,
// 						coordinateType
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000120 RID: 288 RVA: 0x00008CEA File Offset: 0x00006EEA
// 				internal void RemovePartsInfo(int _slotIndex)
// 				{
// 					this.RemovePartsInfo(this._chaCtrl.fileStatus.coordinateType, _slotIndex);
// 				}
//
// 				// Token: 0x06000121 RID: 289 RVA: 0x00008D04 File Offset: 0x00006F04
// 				internal void RemovePartsInfo(int _coordinateIndex, int _slotIndex)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("RemoveSlotTriggerProperty", new object[]
// 					{
// 						_coordinateIndex,
// 						_slotIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000122 RID: 290 RVA: 0x00008D54 File Offset: 0x00006F54
// 				internal object GetTriggerGroupByGUID(int _coordinateIndex, string _guid)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return -1;
// 					}
// 					return this._traverses["pluginCtrl"].Method("GetTriggerGroupByGUID", new object[]
// 					{
// 						_coordinateIndex,
// 						_guid
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000123 RID: 291 RVA: 0x00008DA1 File Offset: 0x00006FA1
// 				internal int GetNextGroupID(int _coordinateIndex)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return -1;
// 					}
// 					return this._traverses["pluginCtrl"].Method("GetNextGroupID", new object[]
// 					{
// 						_coordinateIndex
// 					}).GetValue<int>();
// 				}
//
// 				// Token: 0x06000124 RID: 292 RVA: 0x00008DDA File Offset: 0x00006FDA
// 				internal void PackGroupID(int _coordinateIndex)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("PackGroupID", new object[]
// 					{
// 						_coordinateIndex
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000125 RID: 293 RVA: 0x00008E13 File Offset: 0x00007013
// 				internal void RefreshCache()
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("RefreshCache", Array.Empty<object>()).GetValue();
// 				}
//
// 				// Token: 0x06000126 RID: 294 RVA: 0x00008E42 File Offset: 0x00007042
// 				internal void InitCurOutfitTriggerInfo(string _caller)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("InitCurOutfitTriggerInfo", new object[]
// 					{
// 						_caller
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000127 RID: 295 RVA: 0x00008E76 File Offset: 0x00007076
// 				internal void SetAccessoryStateAll(bool _show = true)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("SetAccessoryStateAll", new object[]
// 					{
// 						_show
// 					}).GetValue();
// 				}
//
// 				// Token: 0x06000128 RID: 296 RVA: 0x00008EAF File Offset: 0x000070AF
// 				internal void SyncAllAccToggle(string _caller)
// 				{
// 					if (!CharacterAccessory.AccStateSyncSupport._installed)
// 					{
// 						return;
// 					}
// 					this._traverses["pluginCtrl"].Method("SyncAllAccToggle", new object[]
// 					{
// 						_caller
// 					}).GetValue();
// 				}
//
// 				// Token: 0x04000098 RID: 152
// 				private readonly ChaControl _chaCtrl;
//
// 				// Token: 0x04000099 RID: 153
// 				private readonly CharaCustomFunctionController _pluginCtrl;
//
// 				// Token: 0x0400009A RID: 154
// 				private readonly Dictionary<string, object> _charaAccData = new Dictionary<string, object>();
//
// 				// Token: 0x0400009B RID: 155
// 				private readonly Dictionary<string, Traverse> _traverses = new Dictionary<string, Traverse>();
// 			}
//
// 			// Token: 0x0200002C RID: 44
// 			public class AccTriggerInfo
// 			{
// 				// Token: 0x1700001F RID: 31
// 				// (get) Token: 0x06000129 RID: 297 RVA: 0x00008EE3 File Offset: 0x000070E3
// 				// (set) Token: 0x0600012A RID: 298 RVA: 0x00008EEB File Offset: 0x000070EB
// 				public int Slot { get; set; }
//
// 				// Token: 0x17000020 RID: 32
// 				// (get) Token: 0x0600012B RID: 299 RVA: 0x00008EF4 File Offset: 0x000070F4
// 				// (set) Token: 0x0600012C RID: 300 RVA: 0x00008EFC File Offset: 0x000070FC
// 				public int Kind { get; set; } = -1;
//
// 				// Token: 0x17000021 RID: 33
// 				// (get) Token: 0x0600012D RID: 301 RVA: 0x00008F05 File Offset: 0x00007105
// 				// (set) Token: 0x0600012E RID: 302 RVA: 0x00008F0D File Offset: 0x0000710D
// 				public string Group { get; set; } = "";
//
// 				// Token: 0x17000022 RID: 34
// 				// (get) Token: 0x0600012F RID: 303 RVA: 0x00008F16 File Offset: 0x00007116
// 				// (set) Token: 0x06000130 RID: 304 RVA: 0x00008F1E File Offset: 0x0000711E
// 				public List<bool> State { get; set; } = new List<bool>
// 				{
// 					true,
// 					false,
// 					false,
// 					false
// 				};
//
// 				// Token: 0x06000131 RID: 305 RVA: 0x00008F28 File Offset: 0x00007128
// 				public AccTriggerInfo(int slot)
// 				{
// 					this.Slot = slot;
// 				}
// 			}
// 		}
// 	}
// }
