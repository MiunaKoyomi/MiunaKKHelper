# Outfit personal names

Helper 1.2.4 exposes `CardOutfitNames.Get/Set(dataId, coordinateIndex, name)`.
Storage is `Dictionary<string, Dictionary<int, string>>`, saved atomically to
`BepInEx/config/org.miuna.plugins.KKHelper.outfit-names.xml` (KKS uses KKSHelper).
The original loaded card dataId is the key, never a highlighted list row or file path.
Ratings remain in their existing file and are not changed by naming.

KK exporter 1.3.8 / KKS exporter 1.2.11 use this API for the Outfit text fields
and exported cloth JSON / card manifest. Priority: personal name, existing coordinateName,
localized coordinate type, then numbered outfit. Empty personal input removes that override.
Neither editing nor exporting names modifies the original card or Maker coordinateName.
The existing Unity importer maps coordinateName into Cloth SO displayName and labels.

Builds: KK/KKS Helper and exporters pass. CardPreferences test executable passes 27 checks,
including eight new checks for name persistence, Unicode/XML escaping, card/slot separation,
dataId normalization, clearing, invalid identity rejection, and failed-write rollback.
Game UI/restart and full Unity import were not exercised during this change.
