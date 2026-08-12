using KKAPI;
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
}