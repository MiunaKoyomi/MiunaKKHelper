using System;
using System.IO;
using MiunaKKHelper.CardFavorites;
using KoikatsuPipeline.PngLoading;
public static class CardPreferenceTests
{
    static int checks;
    public static int Main() { try { Console.WriteLine(Run()); return 0; } catch(Exception e) { Console.WriteLine(e); return 1; } }
    public static string Run()
    {
        checks = 0;
        string temp = Path.Combine(Path.GetTempPath(), "MiunaCards-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string file = Path.Combine(temp, "preferences.xml"), card = Path.Combine(temp, "card.png"), copy = Path.Combine(temp, "copy.png");
            string namesPath = Path.Combine(temp, "outfit-names.xml");
            string idA = "90015098-3cd2-4fb0-d696-3f7d28e17f72", idB = "11111111-2222-3333-4444-555555555555";
            var names = new CardOutfitNameStore(namesPath);
            Check(names.Get(idA, 0) == null, "name default absent");
            names.Set(idA, 0, "校服 & <测试>"); names.Set(idA, 4, "演出服"); names.Set(idB, 0, "睡衣");
            names = new CardOutfitNameStore(namesPath);
            Check(names.Get(idA, 0) == "校服 & <测试>", "outfit Unicode XML reload");
            Check(names.Get(idA, 4) == "演出服", "independent slot");
            Check(names.Get(idB, 0) == "睡衣", "independent card");
            Check(names.Get(idA.ToUpperInvariant(), 4) == "演出服", "normalized dataId");
            names.Set(idA, 0, "");
            names = new CardOutfitNameStore(namesPath);
            Check(names.Get(idA, 0) == null && names.Get(idA, 4) == "演出服", "clear restores fallback only this slot");
            Check(names.GetMetadata(idA, "card") == null, "legacy outfit store has no metadata override");
            names.SetMetadata(idA, "card", "作者 & <虚子>");
            names.SetMetadata(idA, "cardName", "莫娜");
            names.SetMetadata(idB, "card", "");
            names.Set(idA, 4, "礼服");
            names = new CardOutfitNameStore(namesPath);
            Check(names.GetMetadata(idA, "card") == "作者 & <虚子>" && names.GetMetadata(idA, "cardName") == "莫娜", "metadata persists alongside outfit edits");
            Check(names.GetMetadata(idB, "card") == "" && names.GetMetadata(idB, "cardName") == null, "empty and absent metadata are distinct and card isolated");
            names.SetMetadata(idA, "cardName", null);
            names.Set(idA, 4, null);
            names = new CardOutfitNameStore(namesPath);
            Check(names.GetMetadata(idA, "cardName") == null && names.GetMetadata(idA, "card") == "作者 & <虚子>", "reset field and delete last outfit preserve other metadata");
            string metadataBefore = File.ReadAllText(namesPath);
            try { names.SetMetadata(idA, "card", "bad\u0001"); } catch (ArgumentException) { }
            Check(File.ReadAllText(namesPath) == metadataBefore && names.GetMetadata(idA, "card") == "作者 & <虚子>", "failed metadata write preserves disk and memory");
            bool invalidRejected = false;
            try { names.Set("invalid", 0, "bad"); } catch (ArgumentException) { invalidRejected = true; }
            Check(invalidRejected, "no path or invalid identity keys");
            string saved = File.ReadAllText(namesPath);
            try { names.Set(idA, 1, "bad\u0001"); } catch (ArgumentException) { }
            Check(File.ReadAllText(namesPath) == saved && names.Get(idA, 1) == null, "failed write leaves file and dictionary unchanged");
            File.WriteAllText(card, "abc");
            var store = new CardPreferenceStore(file, temp);
            Check(store.Get(card).Rating == 0, "unmarked");
            Check(store.GetDataId(card) == "90015098-3cd2-4fb0-d696-3f7d28e17f72", "MD5 text GUID byte order");
            Check(store.GetDataId(card) == CardPngIdentity.ComputeDataId(card), "exporter parity");
            store.Set(card, 4, false);
            Check(store.Get(card).Rating == 4 && !store.Get(card).Favorite, "blue");
            store.Set(card, 5, true);
            store = new CardPreferenceStore(file, temp);
            Check(store.Get(card).Rating == 5 && store.Get(card).Favorite, "reload");
            File.Copy(card, copy);
            Check(store.Get(copy).Rating == 5, "copy");
            File.Move(copy, copy+"renamed");
            Check(new CardPreferenceStore(file, temp).Get(copy+"renamed").Favorite, "rename");
            File.WriteAllText(copy+"renamed", "changed bytes");
            Check(store.Get(copy+"renamed").Rating == 0, "changed content");
            store.Set(card, 0, true);
            Check(store.Get(card).Rating == 0 && store.Get(card).Favorite, "independent favorite");
            store.Set(card, 4, false);
            Check(!store.Get(card).Favorite, "remove favorite");
            bool rejected = false;
            try { store.Set(card, 3, true); } catch(ArgumentOutOfRangeException) { rejected = true; }
            Check(rejected && store.Get(card).Rating == 4, "invalid rating");
            string previous = File.ReadAllText(file);
            Directory.CreateDirectory(file+".tmp");
            rejected = false;
            try { store.Set(card, 5, true); } catch(Exception) { rejected = true; }
            Check(rejected && store.Get(card).Rating == 4 && File.ReadAllText(file) == previous, "atomic failure");
            Directory.Delete(file+".tmp");
            Check(File.Exists(file+".bak"), "backup");
            store.Set(card, 0, false);
            Check(new CardPreferenceStore(file, temp).Get(card).Rating == 0 && !File.ReadAllText(file).Contains("<card "), "clear");
            File.WriteAllText(file, "<cardPreferences version='1'><card path='card.png' rating='4' favorite='False'/><card path='missing.png' rating='5' favorite='True'/></cardPreferences>");
            store = new CardPreferenceStore(file, temp);
            Check(store.Get(card).Rating == 4 && File.ReadAllText(file).Contains("dataId="), "migration");
            Check(File.ReadAllText(file).Contains("missing.png") && File.ReadAllText(file+".bak").Contains("version='1'"), "preserve unresolved and original backup");
            File.WriteAllText(Path.Combine(temp, "missing.png"), "late card");
            store = new CardPreferenceStore(file, temp);
            Check(store.Get(Path.Combine(temp, "missing.png")).Favorite, "late migration");
            File.WriteAllText(file, "<cardPreferences version='1'><card path='card.png' rating='4' favorite='True'/><card path='same.png' rating='5' favorite='False'/></cardPreferences>");
            File.Copy(card, Path.Combine(temp, "same.png"));
            store = new CardPreferenceStore(file, temp);
            Check(store.Get(card).Rating == 5 && store.Get(card).Favorite, "duplicate merge");
            File.WriteAllText(file, "broken XML");
            rejected = false;
            try { new CardPreferenceStore(file, temp); } catch(Exception) { rejected = true; }
            Check(rejected && File.ReadAllText(file) == "broken XML", "corrupt file protected");
            var four = new CardPreference { Rating = 4 };
            var five = new CardPreference { Rating = 5, Favorite = true };
            var liked = new CardPreference { Favorite = true };
            Check(!CardPreferenceFilter.Active(false, false, false), "filter off");
            Check(CardPreferenceFilter.Matches(default(CardPreference), false, false, false), "no filter keeps unmarked");
            Check(CardPreferenceFilter.Matches(four, true, false, false), "4 only");
            Check(!CardPreferenceFilter.Matches(five, true, false, false), "5 excluded from 4");
            Check(CardPreferenceFilter.Matches(five, true, true, false), "4 or 5");
            Check(CardPreferenceFilter.Matches(liked, true, false, true), "favorite unions with 4");
            Check(!CardPreferenceFilter.Matches(default(CardPreference), true, false, false), "unmarked hidden");
            Check(CardPreferenceFilter.Matches(four, false, false, true) == false, "4 without like excluded");
            Check(CardPreferenceFilter.Matches(liked, false, false, true), "favorite only");
            return "PASS: "+checks+" identity / migration / persistence checks";
        }
        finally { Directory.Delete(temp, true); }
    }
    static void Check(bool ok, string name) { if(!ok) throw new Exception(name); checks++; }
}
