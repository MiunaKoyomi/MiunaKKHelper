using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using KoikatsuPipeline.PngLoading;

namespace MiunaKKHelper.CardFavorites;

/// <summary>Personal outfit names keyed by original card dataId and coordinate index.</summary>
public sealed class CardOutfitNameStore
{
    readonly string path;
    Dictionary<string, Dictionary<int, string>> entries = new(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, string[]> metadata = new(StringComparer.OrdinalIgnoreCase);

    public CardOutfitNameStore(string path)
    {
        this.path = Path.GetFullPath(path);
        if (!File.Exists(this.path)) return;
        var doc = new XmlDocument { XmlResolver = null };
        var settings = new XmlReaderSettings { XmlResolver = null };
#if KKS
        settings.DtdProcessing = DtdProcessing.Prohibit;
#else
        settings.ProhibitDtd = true;
#endif
        using (var reader = XmlReader.Create(this.path, settings)) doc.Load(reader);
        if (doc.DocumentElement?.Name != "outfitNames" || doc.DocumentElement.GetAttribute("version") != "1")
            throw new IOException("Invalid outfit name store.");
        foreach (XmlNode node in doc.DocumentElement.ChildNodes)
        {
            var card = node as XmlElement;
            if (card == null || card.Name != "card") continue;
            string id = Normalize(card.GetAttribute("dataId"));
            if (card.HasAttribute("card") || card.HasAttribute("cardName") || card.HasAttribute("coordinateName"))
                metadata[id] = new[] { card.HasAttribute("card") ? card.GetAttribute("card") : null,
                    card.HasAttribute("cardName") ? card.GetAttribute("cardName") : null,
                    card.HasAttribute("coordinateName") ? card.GetAttribute("coordinateName") : null };
            var names = new Dictionary<int, string>();
            foreach (XmlNode child in card.ChildNodes)
            {
                var outfit = child as XmlElement;
                if (outfit == null || outfit.Name != "outfit") continue;
                int index;
                if (!int.TryParse(outfit.GetAttribute("index"), out index) || index < 0)
                    throw new IOException("Invalid outfit slot.");
                names[index] = outfit.GetAttribute("name");
            }
            entries[id] = names;
        }
    }

    static string Normalize(string id) => CardPngIdentity.Normalize(id)
        ?? throw new ArgumentException("Missing or invalid loaded card dataId.");

    public string Get(string dataId, int index)
    {
        Dictionary<int, string> names;
        string name;
        return entries.TryGetValue(Normalize(dataId), out names) && names.TryGetValue(index, out name) ? name : null;
    }

    public void Set(string dataId, int index, string name)
    {
        string id = Normalize(dataId);
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        if (name != null && name.Length > 80) throw new ArgumentException("Outfit name is too long.");
        if (Get(id, index) == name) return;
        var next = new Dictionary<string, Dictionary<int, string>>(entries, StringComparer.OrdinalIgnoreCase);
        Dictionary<int, string> old;
        var names = entries.TryGetValue(id, out old) ? new Dictionary<int, string>(old) : new Dictionary<int, string>();
        if (string.IsNullOrEmpty(name)) names.Remove(index);
        else names[index] = name;
        if (names.Count == 0) next.Remove(id); else next[id] = names;
        Save(next, metadata);
        entries = next;
    }

    public string GetMetadata(string dataId, string field)
    {
        int slot = MetadataSlot(field);
        string[] values;
        return metadata.TryGetValue(Normalize(dataId), out values) ? values[slot] : null;
    }

    public void SetMetadata(string dataId, string field, string value)
    {
        string id = Normalize(dataId);
        int slot = MetadataSlot(field);
        if (value != null && value.Length > 256) throw new ArgumentException("Card metadata is too long.");
        if (GetMetadata(id, field) == value) return;
        var next = new Dictionary<string, string[]>(metadata, StringComparer.OrdinalIgnoreCase);
        string[] old;
        var values = metadata.TryGetValue(id, out old) ? (string[])old.Clone() : new string[3];
        // Empty is an explicit override; null removes the override and restores original-card lookup.
        values[slot] = value;
        if (values[0] == null && values[1] == null && values[2] == null) next.Remove(id); else next[id] = values;
        Save(entries, next);
        metadata = next;
    }

    static int MetadataSlot(string field)
    {
        if (field == "card") return 0;
        if (field == "cardName") return 1;
        if (field == "coordinateName") return 2;
        throw new ArgumentException("Unknown card metadata field.");
    }

    void Save(Dictionary<string, Dictionary<int, string>> next, Dictionary<string, string[]> nextMetadata)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temp = path + ".tmp";
        try
        {
            using (var writer = XmlWriter.Create(temp, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement("outfitNames"); writer.WriteAttributeString("version", "1");
                var ids = new List<string>(next.Keys);
                foreach (var id in nextMetadata.Keys) if (!next.ContainsKey(id)) ids.Add(id);
                foreach (var id in ids)
                {
                    writer.WriteStartElement("card"); writer.WriteAttributeString("dataId", id);
                    string[] values;
                    if (nextMetadata.TryGetValue(id, out values))
                    {
                        if (values[0] != null) writer.WriteAttributeString("card", values[0]);
                        if (values[1] != null) writer.WriteAttributeString("cardName", values[1]);
                        if (values[2] != null) writer.WriteAttributeString("coordinateName", values[2]);
                    }
                    Dictionary<int, string> outfits;
                    if (next.TryGetValue(id, out outfits)) foreach (var outfit in outfits)
                    {
                        writer.WriteStartElement("outfit"); writer.WriteAttributeString("index", outfit.Key.ToString());
                        writer.WriteAttributeString("name", outfit.Value); writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            if (File.Exists(path)) File.Replace(temp, path, path + ".bak"); else File.Move(temp, path);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
