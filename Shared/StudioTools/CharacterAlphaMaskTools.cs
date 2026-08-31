using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MiunaKKHelper.StudioTools;

/// <summary>
/// MaterialEditor 把 <c>alpha_a</c> / <c>alpha_b</c> 拉黑（CheckBlacklist），Studio 改不了。
/// 游戏只在上衣穿脱时通过 <see cref="ChaControl.ChangeAlphaMask"/> 写这两项：
/// 穿=1,1；半=0,1；脱=0,0。这里给选中角色手动滑，并可锁定以免被穿脱盖掉。
/// </summary>
public static class CharacterAlphaMaskTools
{
    static readonly Dictionary<ChaControl, LockedValues> Locked = new Dictionary<ChaControl, LockedValues>();

    struct LockedValues
    {
        public float AlphaA;
        public float AlphaB;
    }

    public static bool TryRead(ChaControl cha, out float alphaA, out float alphaB)
    {
        alphaA = 0f;
        alphaB = 0f;
        Material mat = cha != null ? cha.customMatBody : null;
        if (mat == null || !mat.HasProperty("_alpha_a"))
            return false;

        alphaA = mat.GetFloat(ChaShader._alpha_a);
        alphaB = mat.HasProperty("_alpha_b") ? mat.GetFloat(ChaShader._alpha_b) : 0f;
        return true;
    }

    public static bool IsLocked(ChaControl cha)
    {
        return cha != null && Locked.ContainsKey(cha);
    }

    public static void SetLocked(ChaControl cha, bool locked, float alphaA, float alphaB)
    {
        if (cha == null)
            return;

        if (!locked)
        {
            Locked.Remove(cha);
            return;
        }

        Locked[cha] = new LockedValues { AlphaA = alphaA, AlphaB = alphaB };
        Apply(cha, alphaA, alphaB);
    }

    public static void Apply(ChaControl cha, float alphaA, float alphaB)
    {
        if (cha == null)
            return;

        if (Locked.ContainsKey(cha))
            Locked[cha] = new LockedValues { AlphaA = alphaA, AlphaB = alphaB };

        ApplyToMaterial(cha.customMatBody, alphaA, alphaB);
        CopyBodyExtras(cha, alphaA, alphaB);
        CopyRendererArray(cha.rendBra, alphaA, alphaB);
        CopyRendererArray(cha.rendInner, alphaA, alphaB);
    }

    public static void ReapplyIfLocked(ChaControl cha)
    {
        if (cha == null || !Locked.TryGetValue(cha, out LockedValues values))
            return;
        ApplyToMaterial(cha.customMatBody, values.AlphaA, values.AlphaB);
        CopyBodyExtras(cha, values.AlphaA, values.AlphaB);
        CopyRendererArray(cha.rendBra, values.AlphaA, values.AlphaB);
        CopyRendererArray(cha.rendInner, values.AlphaA, values.AlphaB);
    }

    public static string GetDisplayName(ChaControl cha)
    {
        if (cha == null)
            return "—";

        ChaFileParameter p = cha.chaFile != null ? cha.chaFile.parameter : null;
        if (p != null)
        {
            if (!string.IsNullOrEmpty(p.fullname))
                return p.fullname.Trim();
            string combined = ((p.lastname ?? "") + " " + (p.firstname ?? "")).Trim();
            if (combined.Length > 0)
                return combined;
        }

        return cha.name;
    }

    static void CopyBodyExtras(ChaControl cha, float alphaA, float alphaB)
    {
        Renderer rendBody = cha.rendBody;
        if (rendBody == null || rendBody.sharedMaterials == null || rendBody.sharedMaterials.Length <= 1)
            return;

        for (int i = 0; i < rendBody.sharedMaterials.Length; i++)
            ApplyToMaterial(rendBody.sharedMaterials[i], alphaA, alphaB);
    }

    static void CopyRendererArray(Renderer[] renderers, float alphaA, float alphaB)
    {
        if (renderers == null)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend == null)
                continue;

            Material mat = rend.material;
            ApplyToMaterial(mat, alphaA, alphaB);
            if (rend.materials == null || rend.materials.Length <= 1)
                continue;

            for (int j = 0; j < rend.materials.Length; j++)
                ApplyToMaterial(rend.materials[j], alphaA, alphaB);
        }
    }

    static void ApplyToMaterial(Material mat, float alphaA, float alphaB)
    {
        if (mat == null || !mat.HasProperty("_alpha_a"))
            return;
        mat.SetFloat(ChaShader._alpha_a, alphaA);
        if (mat.HasProperty("_alpha_b"))
            mat.SetFloat(ChaShader._alpha_b, alphaB);
    }

    /// <summary>上衣穿脱会再调 ChangeAlphaMask；锁定时把滑条值写回去。</summary>
    [HarmonyPatch(typeof(ChaControl), nameof(ChaControl.ChangeAlphaMask))]
    static class ChangeAlphaMaskLockPatch
    {
        static void Postfix(ChaControl __instance)
        {
            ReapplyIfLocked(__instance);
        }
    }
}
