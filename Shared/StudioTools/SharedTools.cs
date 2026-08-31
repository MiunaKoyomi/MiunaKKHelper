using System.Collections.Generic;
using KKAPI;
using KKAPI.Maker;
using KKAPI.Studio;
using Studio;
using UnityEngine;

namespace MiunaKKHelper.StudioTools;

public static class SharedTools
{
    public static Transform GetSelectionTransform()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            return null;
        var tree = Studio.Studio.Instance.treeNodeCtrl;
        if (tree == null || tree.selectObjectCtrl == null || tree.selectObjectCtrl.Length == 0) return null;
        var selection = tree.selectObjectCtrl[0].guideObject.transformTarget;
        return selection;
    }

    public static OCIChar GetSelectedOciChar()
    {
        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            return null;

        foreach (OCIChar oci in StudioAPI.GetSelectedCharacters())
        {
            if (oci != null && oci.charInfo != null)
                return oci;
        }

        return null;
    }

    /// <summary>
    /// Maker：当前捏人角色。Studio：Workspace 选中的角色（可多选）。
    /// </summary>
    public static List<ChaControl> GetTargetChaControls()
    {
        var list = new List<ChaControl>();
        if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
        {
            ChaControl makerCha = MakerAPI.GetCharacterControl();
            if (makerCha != null)
                list.Add(makerCha);
            return list;
        }

        if (KoikatuAPI.GetCurrentGameMode() != GameMode.Studio)
            return list;

        foreach (OCIChar oci in StudioAPI.GetSelectedCharacters())
        {
            if (oci == null || oci.charInfo == null)
                continue;
            if (!list.Contains(oci.charInfo))
                list.Add(oci.charInfo);
        }

        if (list.Count == 0)
        {
            ChaControl fromTree = FindChaControl(GetSelectionTransform());
            if (fromTree != null)
                list.Add(fromTree);
        }

        return list;
    }

    public static ChaControl FindChaControl(Transform t)
    {
        if (t == null)
            return null;
        return t.GetComponent<ChaControl>()
               ?? t.GetComponentInParent<ChaControl>()
               ?? t.GetComponentInChildren<ChaControl>(true);
    }

    /// <summary>
    /// Maker：当前捏人角色。Studio：对象树选中项，否则选中 OCIChar。
    /// </summary>
    public static Transform GetCharacterRoot()
    {
        if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            return ResolveChaRoot(MakerAPI.GetCharacterControl());

        Transform selected = GetSelectionTransform();
        if (selected != null)
            return selected;

        return ResolveChaRoot(GetSelectedOciChar()?.charInfo);
    }

    static Transform ResolveChaRoot(ChaControl cha)
    {
        if (cha == null)
            return null;
        if (cha.objTop != null)
            return cha.objTop.transform;
        return cha.transform;
    }
}