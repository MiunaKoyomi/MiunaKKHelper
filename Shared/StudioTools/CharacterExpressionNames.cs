using BepInEx.Bootstrap;

namespace MiunaKKHelper.StudioTools;

internal static class CharacterExpressionNames
{
    // 内置编号与 MindControlGame 的 KoikatsuGame*EmoEnum 对齐。
    static readonly string[] Eyebrows =
    {
        "默认", "愤怒", "委屈", "无聊", "疑问（左）", "疑问（右）",
        "思虑（左）", "思虑（右）", "愤怒 2（左）", "愤怒 2（右）",
        "认真", "不安", "惊讶", "沮丧", "得意", "眨单眼（左）", "眨单眼（右）"
    };

    static readonly string[] EyePatterns =
    {
        "默认", "闭上双眼", "笑脸", "闭眼笑", "微笑", "眨单眼（左）", "眨单眼（右）",
        "难过", "害羞", "愤怒", "认真", "无聊", "痛苦", "厌恶", "思虑",
        "悲伤", "哭泣", "焦躁", "沮丧", "为难", "得意"
    };

    static readonly string[] MouthPatterns =
    {
        "默认", "笑脸", "开心", "开心（小）", "开心（微）", "心动", "心动（小）", "心动（微）",
        "愤怒", "愤怒 2", "认真", "认真 2", "厌恶", "寂寞", "焦躁", "不满",
        "吃惊", "惊讶", "惊讶（小）", "得意", "吐舌", "舔舐", "吸吮", "亲吻", "张嘴吐舌",
        "啊（小）", "啊（大）", "咿（小）", "咿（大）", "呜（小）", "呜（大）", "哎（小）",
        "哎（大）", "哦（小）", "哦（大）", "嗯（小）", "嗯（大）", "猫口（闭上）", "三角", "变形微笑"
    };

    // SexFaces 1.3 的扩展从 43 开始，40–42 未命名。
    const int ExtendedMouthStart = 43;
    static readonly string[] ExtendedMouthPatterns =
    {
        "吐舌笑", "吐舌皱嘴", "吐舌大笑", "撇嘴（左）", "撇嘴（右）",
        "歪嘴笑（左）", "歪嘴笑（右）", "歪嘴笑 2（左）", "歪嘴笑 2（右）", "得意露齿笑", "猫口露齿笑"
    };

    public static string Eyebrow(int pattern) => Lookup(Eyebrows, pattern) ?? Unknown(pattern);

    public static string Eyes(ChaControl character)
    {
        int pattern = character.GetEyesPtn();
        string name = Lookup(EyePatterns, pattern);
        if (name != null)
            return name;

        var eyeSets = character.lstCtrl?.GetCategoryInfo(ChaListDefine.CategoryNo.cha_eyeset);
        if (eyeSets != null && eyeSets.TryGetValue(pattern, out var eyeSet))
        {
            string listName = eyeSet?.Name;
            if (!string.IsNullOrEmpty(listName) && listName != "0")
                return listName;
        }

        return Unknown(pattern);
    }

    public static string Mouth(int pattern)
    {
        string name = Lookup(MouthPatterns, pattern);
        if (name == null && pattern >= ExtendedMouthStart && Chainloader.PluginInfos.ContainsKey("Sauceke.SexFaces"))
            name = Lookup(ExtendedMouthPatterns, pattern - ExtendedMouthStart);
        return name ?? Unknown(pattern);
    }

    static string Lookup(string[] names, int pattern) =>
        pattern >= 0 && pattern < names.Length ? names[pattern] : null;

    static string Unknown(int pattern) => "未命名（#" + pattern + "）";
}
