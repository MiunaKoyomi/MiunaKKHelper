using System;
using System.Reflection;
using UnityEngine;

namespace MiunaKKHelper;

/// <summary>
/// 挡住 XUnity.AutoTranslator 翻本插件 IMGUI。
/// BepInEx 下没有 ___XUnityAutoTranslator 这个 GO，README 的 Find+SendMessage 无效。
/// 改走 AutoTranslationPlugin.Current 反射 Disable/Enable，并用 \u180e 前缀兜底
/// （对应 AutoTranslatorConfig IgnoreTextStartingWith）。
/// </summary>
static class XuaImguiGuard
{
    const char IgnorePrefix = '\u180e';

    static bool _resolved;
    static object _plugin;
    static MethodInfo _disable;
    static MethodInfo _enable;

    static void Resolve()
    {
        if (_resolved)
            return;
        _resolved = true;

        try
        {
            Type type = Type.GetType(
                "XUnity.AutoTranslator.Plugin.Core.AutoTranslationPlugin, XUnity.AutoTranslator.Plugin.Core");
            if (type == null)
                return;

            PropertyInfo current = type.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
            _plugin = current != null ? current.GetValue(null, null) : null;
            if (_plugin == null)
                return;

            _disable = type.GetMethod("DisableAutoTranslator", BindingFlags.Public | BindingFlags.Instance);
            _enable = type.GetMethod("EnableAutoTranslator", BindingFlags.Public | BindingFlags.Instance);
        }
        catch
        {
            _plugin = null;
            _disable = null;
            _enable = null;
        }
    }

    public static string Plain(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        if (text[0] == IgnorePrefix)
            return text;
        return IgnorePrefix + text;
    }

    public static void Run(Action draw)
    {
        Resolve();
        try
        {
            if (_disable != null)
                _disable.Invoke(_plugin, null);
            draw();
        }
        finally
        {
            if (_enable != null)
                _enable.Invoke(_plugin, null);
        }
    }

    public static bool Button(string text, params GUILayoutOption[] options)
    {
        return GUILayout.Button(Plain(text), options);
    }

    public static void Label(string text, params GUILayoutOption[] options)
    {
        GUILayout.Label(Plain(text), options);
    }

    public static void Label(string text, GUIStyle style, params GUILayoutOption[] options)
    {
        GUILayout.Label(Plain(text), style, options);
    }

    public static bool Toggle(bool value, string text, params GUILayoutOption[] options)
    {
        return GUILayout.Toggle(value, Plain(text), options);
    }
}
