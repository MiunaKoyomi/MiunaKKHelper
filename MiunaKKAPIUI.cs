using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using KKAPI;
using KKAPI.Maker;
using UnityEngine;

namespace KK_API_DOC;
public class MiunaKKAPIUI : MonoBehaviour
{
    
    internal static Rect MainWindowRect = new (500, 40, 240, 400);
    
    public static bool UIActive;
    
    //下拉框
    private bool _isDropdownOpen = false;
    private List<int> _slots = null;
    private string[] _slotOptions = null; // 初始为空
    private int _selectedIndex = -1;      // -1 表示未选中
    private int _selectedSlot = -1;
    private Vector2 _scrollPos;
    
    void Update()
    {
        if (MiunaKKAPI.hotkey.Value.IsDown() && (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker 
                                                 || KoikatuAPI.GetCurrentGameMode() == GameMode.Studio))
        {
            UIActive = !UIActive;
            return;
        }
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio && KoikatuAPI.GetCurrentGameMode() != GameMode.Maker)
            UIActive = false;
    }
    
    void OnGUI()
    {
        if (!(KoikatuAPI.GetCurrentGameMode() == GameMode.Maker || KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)) return;
        if (UIActive)
        {
            MainWindowRect = GUILayout.Window(33361, MainWindowRect, WindowFunction, "Miuna KK Helper " + MiunaKKAPI.Version);
            KKAPI.Utilities.IMGUIUtils.EatInputInRect(MainWindowRect);
        }
    }
    
    private void WindowFunction(int windowID)
    {
        GUILayout.BeginVertical();
        
        // 标题
        GUILayout.Label("MiunaTest", GUILayout.Height(30));
        
        // --- 下拉框区域 ---
        // 显示当前选中的值
        string buttonText = (_selectedIndex >= 0 && _slotOptions != null && _selectedIndex < _slotOptions.Length) 
            ? _slotOptions[_selectedIndex] 
            : "Select Slot...";
        
        if (GUILayout.Button(buttonText, GUILayout.Height(25), GUILayout.Width(200)))
        {
            _isDropdownOpen = !_isDropdownOpen;
        }
        
        // 下拉列表展开
        if (_isDropdownOpen && _slotOptions != null && _slotOptions.Length > 0)
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(100), GUILayout.Width(200));
            for (int i = 0; i < _slotOptions.Length; i++)
            {
                if (GUILayout.Button(_slotOptions[i], GUILayout.Height(25)))
                {
                    _selectedIndex = i;
                    _isDropdownOpen = false;
                }
            }
            GUILayout.EndScrollView();
        }
        
        GUILayout.Space(5);
        
        // --- Apply Option 按钮 ---
        GUI.enabled = (_selectedIndex != -1);
        if (GUILayout.Button("Apply Option", GUILayout.Height(30), GUILayout.Width(200)))
        {
            _selectedSlot = _slots[_selectedIndex];
            MiunaKKAPI.Logger.LogWarning($"Applying: {_selectedSlot}");
        }
        GUI.enabled = true;
        
        GUILayout.Space(5);
        
        // --- Clear Log 按钮 ---
        if (GUILayout.Button("Clear Log", GUILayout.Height(30), GUILayout.Width(200)))
        {
            ClearConsole();
        }
        
        GUILayout.Space(5);
        
        // --- Test CharAcc 按钮 ---
        if (GUILayout.Button("Test CharAcc", GUILayout.Height(30), GUILayout.Width(200)))
        {
            var targetController = MakerAPI.GetCharacterControl()
                .gameObject.GetComponentInChildren(ForceGetControllerType());
            if (targetController != null)
            {
                object backupEnabled = AccessTools.Field(targetController.GetType(), "FunctionEnable")
                    .GetValue(targetController);
                MiunaKKAPI.Logger.LogWarning((bool)backupEnabled);
                object dictObj = AccessTools.Field(targetController.GetType(), "PartsInfo")
                    .GetValue(targetController);
                if (dictObj is IDictionary dict)
                {
                    bool hasElements = dict.Count > 0;
                    if (hasElements)
                    {
                        int maxIndex = -1;
                        foreach (DictionaryEntry entry in dict)
                        {
                            int slotIndex = (int)entry.Key;
                            ChaFileAccessory.PartsInfo partsInfo = (ChaFileAccessory.PartsInfo)entry.Value;
                            MiunaKKAPI.Logger.LogWarning($"正在处理 Slot: {slotIndex} info : {partsInfo.type}");
                            maxIndex = Mathf.Max(maxIndex, slotIndex);
                        }
                        MiunaKKAPI.Logger.LogWarning($"PartsInfo 字典中有 {dict.Count} 个元素");
                    }
                }
            }
        }
        
        GUILayout.EndVertical();
        
        // 可拖拽区域（放在最后，覆盖整个窗口）
        GUI.DragWindow(new Rect(0, 0, MainWindowRect.width, 20));
    }
    
    public static void ClearConsole()
    {
        try
        {
            Console.Clear();
        }
        catch (System.IO.IOException)
        {
            // 缓冲
        }
    }
    
    public static Type ForceGetControllerType()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "KKS_CharacterAccessory");

        if (assembly == null) return null;

        return assembly.GetType("CharacterAccessory+CharacterAccessoryController");
    }
}

public struct UIButton
{
    public string Text;
    public Action Action;
}