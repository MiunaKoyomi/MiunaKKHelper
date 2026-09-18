using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using KoikatsuPipeline.PngLoading;

namespace MiunaKKHelper.CardFavorites;

public struct CardPreference { public int Rating; public bool Favorite; }

/// <summary>Original file dataID -> personal metadata. Never writes card files.</summary>
public sealed class CardPreferenceStore
{
    readonly string filePath, gameRoot;
    Dictionary<string, CardPreference> entries = new(StringComparer.OrdinalIgnoreCase);
    Dictionary<string, CardPreference> legacy = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, IdentityCache> cache = new(StringComparer.OrdinalIgnoreCase);
    sealed class IdentityCache { public long Size, Ticks; public string Id; }
    public int Revision { get; private set; }

    public CardPreferenceStore(string filePath, string gameRoot)
    {
        this.filePath = Path.GetFullPath(filePath);
        this.gameRoot = Path.GetFullPath(gameRoot);
        if (!File.Exists(this.filePath)) return;
        var document = new XmlDocument { XmlResolver = null };
        var settings = new XmlReaderSettings { XmlResolver = null };
#if KKS
        settings.DtdProcessing = DtdProcessing.Prohibit;
#else
        settings.ProhibitDtd = true;
#endif
        using (var reader = XmlReader.Create(this.filePath, settings)) document.Load(reader);
        var root = document.DocumentElement;
        if (root == null || root.Name != "cardPreferences"
            || (root.GetAttribute("version") != "1" && root.GetAttribute("version") != "2"))
            throw new IOException("Unsupported card preference file.");
        foreach (XmlNode node in root.ChildNodes)
        {
            var el = node as XmlElement;
            if (el == null || el.Name != "card") continue;
            int rating; bool favorite;
            if (!int.TryParse(el.GetAttribute("rating"), out rating) || !ValidRating(rating)
                || !bool.TryParse(el.GetAttribute("favorite"), out favorite))
                throw new IOException("Invalid card preference record.");
            var value = new CardPreference { Rating = rating, Favorite = favorite };
            string id = CardPngIdentity.Normalize(el.GetAttribute("dataId"));
            if (root.GetAttribute("version") == "2" && el.HasAttribute("dataId") && id == null)
                throw new IOException("Invalid card dataID.");
            if (id != null) entries[id] = value;
            else if (el.HasAttribute("path")) legacy[FullPath(el.GetAttribute("path"))] = value;
            else throw new IOException("Missing card identity.");
        }
        bool migrated = false;
        var remaining = new Dictionary<string, CardPreference>(legacy, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in legacy)
        {
            string id;
            try { id = GetDataId(pair.Key); }
            catch (IOException) { continue; }
            catch (UnauthorizedAccessException) { continue; }
            CardPreference old;
            if (entries.TryGetValue(id, out old))
                entries[id] = new CardPreference { Rating = Math.Max(old.Rating, pair.Value.Rating), Favorite = old.Favorite || pair.Value.Favorite };
            else entries[id] = pair.Value;
            remaining.Remove(pair.Key);
            migrated = true;
        }
        if (migrated) Persist(entries, remaining);
    }

    static bool ValidRating(int rating) => rating == 0 || rating == 4 || rating == 5;
    string FullPath(string path)
    {
        if (string.IsNullOrEmpty(path)) throw new ArgumentException("Card path is empty.");
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(gameRoot, path));
    }

    public string GetDataId(string cardPath)
    {
        string full = FullPath(cardPath);
        var info = new FileInfo(full);
        if (!info.Exists) throw new FileNotFoundException("Card file not found.", full);
        IdentityCache cached;
        if (cache.TryGetValue(full, out cached) && cached.Size == info.Length && cached.Ticks == info.LastWriteTimeUtc.Ticks)
            return cached.Id;
        string id = CardPngIdentity.ComputeDataId(full);
        cache[full] = new IdentityCache { Id = id, Size = info.Length, Ticks = info.LastWriteTimeUtc.Ticks };
        return id;
    }

    public CardPreference Get(string cardPath)
    {
        CardPreference value;
        string full = FullPath(cardPath);
        string id = GetDataId(full);
        if (entries.TryGetValue(id, out value)) return value;
        if (!legacy.TryGetValue(full, out value)) return default;
        // A legacy file may reappear after startup. Migrate before it can be moved again.
        Set(full, value.Rating, value.Favorite);
        return value;
    }

    public void Set(string cardPath, int rating, bool favorite)
    {
        if (!ValidRating(rating)) throw new ArgumentOutOfRangeException(nameof(rating));
        string id = GetDataId(cardPath);
        var next = new Dictionary<string, CardPreference>(entries, StringComparer.OrdinalIgnoreCase);
        var remaining = new Dictionary<string, CardPreference>(legacy, StringComparer.OrdinalIgnoreCase);
        remaining.Remove(FullPath(cardPath));
        if (rating == 0 && !favorite) next.Remove(id);
        else next[id] = new CardPreference { Rating = rating, Favorite = favorite };
        Persist(next, remaining);
    }

    void Persist(Dictionary<string, CardPreference> next, Dictionary<string, CardPreference> remaining)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        string temporary = filePath + ".tmp";
        try
        {
            using (var writer = XmlWriter.Create(temporary, new XmlWriterSettings { Indent = true }))
            {
                writer.WriteStartElement("cardPreferences");
                writer.WriteAttributeString("version", "2");
                WriteRecords(writer, next, "dataId");
                WriteRecords(writer, remaining, "path");
                writer.WriteEndElement();
            }
            if (File.Exists(filePath)) File.Replace(temporary, filePath, filePath + ".bak");
            else File.Move(temporary, filePath);
            entries = next;
            legacy = remaining;
            Revision++;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    void WriteRecords(XmlWriter writer, Dictionary<string, CardPreference> values, string attribute)
    {
        var keys = new List<string>(values.Keys);
        keys.Sort(StringComparer.OrdinalIgnoreCase);
        foreach (string key in keys)
        {
            string stored = key;
            string prefix = gameRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (attribute == "path" && stored.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                stored = stored.Substring(prefix.Length);
            writer.WriteStartElement("card");
            writer.WriteAttributeString(attribute, stored);
            writer.WriteAttributeString("rating", values[key].Rating.ToString());
            writer.WriteAttributeString("favorite", values[key].Favorite.ToString());
            writer.WriteEndElement();
        }
    }
}
