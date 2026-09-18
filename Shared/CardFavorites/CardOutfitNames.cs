using System.IO;
using BepInEx;

namespace MiunaKKHelper.CardFavorites;

/// <summary>Public bridge for the exporter; loading failures must not overwrite personal data.</summary>
public static class CardOutfitNames
{
    static CardOutfitNameStore store;
    static CardOutfitNameStore Store => store ?? (store = new CardOutfitNameStore(
        Path.Combine(Paths.ConfigPath, MiunaHelperHost.GUID + ".outfit-names.xml")));

    public static string Get(string dataId, int index) => Store.Get(dataId, index);
    public static void Set(string dataId, int index, string name) => Store.Set(dataId, index, name);
    public static string GetMetadata(string dataId, string field) => Store.GetMetadata(dataId, field);
    public static void SetMetadata(string dataId, string field, string value) => Store.SetMetadata(dataId, field, value);
}
