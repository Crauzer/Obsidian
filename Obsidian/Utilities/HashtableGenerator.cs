﻿using Fantome.Libraries.League.Helpers;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WadFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeagueUtilities = Fantome.Libraries.League.Helpers.Utilities;

namespace Obsidian.Utilities
{
    public static class HashtableGenerator
    {
        private static List<string> _legacyDirLists = new List<string>();

        public static void Initialize()
        {
            _legacyDirLists.Add("data/final/data.wad.legacydirlistinfo");

            for (int i = 0; i < 100; i++)
            {
                _legacyDirLists.Add(string.Format("data/final/maps/shipping/map{0}levels.wad.legacydirlistinfo", i));
            }
        }

        public static Dictionary<ulong, string> Generate(Wad wad)
        {
            Dictionary<ulong, string> hashtable = new Dictionary<ulong, string>();
            List<string> strings = new List<string>();

            foreach (WadEntry entry in wad.Entries.Values.Where(x => x.Type != WadEntryType.FileRedirection))
            {
                using Stream entryStream = entry.GetDataHandle().GetDecompressedStream();
                LeagueFileType fileType = LeagueUtilities.GetExtensionType(entryStream);

                if (fileType == LeagueFileType.BIN)
                {
                    BINFile bin = null;
                    try
                    {
                        bin = new BINFile(entryStream);
                    }
                    catch (Exception)
                    {

                    }

                    if (bin != null)
                    {
                        strings.AddRange(ProcessBINDependencies(bin.Dependencies));
                        strings.AddRange(ProcessBINFile(bin));
                    }
                }
                else if (IsLegacyDirList(entry.XXHash))
                {
                    strings.AddRange(ProcessLegacyDirList(entry));
                }
            }

            using (XXHash64 xxHash = XXHash64.Create())
            {
                foreach (string fetchedString in strings.Distinct())
                {
                    if (!string.IsNullOrEmpty(fetchedString))
                    {
                        ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(fetchedString.ToLower())), 0);

                        if (!hashtable.ContainsKey(hash))
                        {
                            hashtable.Add(hash, fetchedString);
                        }
                    }
                }
            }

            return hashtable;
        }

        private static IEnumerable<string> ProcessBINFile(BINFile bin)
        {
            List<string> strings = new List<string>();

            foreach (BINEntry entry in bin.Entries)
            {
                strings.AddRange(ProcessBINEntry(entry));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINEntry(BINEntry entry)
        {
            List<string> strings = new List<string>();

            foreach (BINValue value in entry.Values)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINValue(BINValue value)
        {
            List<string> strings = new List<string>();

            if (value.Type == BINValueType.String)
            {
                string valueString = value.Value as string;
                strings.Add(valueString);

                if ((valueString.StartsWith("ASSETS/", true, null) || valueString.StartsWith("LEVELS/", true, null)) && Path.GetExtension(valueString) == ".dds")
                {
                    int index = valueString.LastIndexOf('/');
                    strings.Add(valueString.Insert(index + 1, "2x_"));
                    strings.Add(valueString.Insert(index + 1, "4x_"));
                }

                if (value.Property == Cryptography.FNV32Hash("mapPath"))
                {
                    strings.Add("DATA/" + valueString + ".materials.bin");
                    strings.Add("DATA/" + valueString + ".mapgeo");
                }
            }
            else if (value.Type == BINValueType.Optional)
            {
                strings.AddRange(ProcessBINAdditionalData(value.Value as BINOptional));
            }
            else if (value.Type == BINValueType.Container)
            {
                strings.AddRange(ProcessBINContainer(value.Value as BINContainer));
            }
            else if (value.Type == BINValueType.Embedded || value.Type == BINValueType.Structure)
            {
                strings.AddRange(ProcessBINStructure(value.Value as BINStructure));
            }
            else if (value.Type == BINValueType.Map)
            {
                strings.AddRange(ProcessBINMap(value.Value as BINMap));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINAdditionalData(BINOptional additionalData)
        {
            List<string> strings = new List<string>();

            if (additionalData.Value != null)
            {
                strings.AddRange(ProcessBINValue(additionalData.Value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINContainer(BINContainer container)
        {
            List<string> strings = new List<string>();

            foreach (BINValue value in container.Values)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINStructure(BINStructure structure)
        {
            List<string> strings = new List<string>();

            foreach (BINValue value in structure.Values)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINMap(BINMap map)
        {
            List<string> strings = new List<string>();

            foreach (KeyValuePair<BINValue, BINValue> valuePair in map.Values)
            {
                strings.AddRange(ProcessBINValue(valuePair.Key));
                strings.AddRange(ProcessBINValue(valuePair.Value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINDependencies(IEnumerable<string> linkedFiles)
        {
            List<string> strings = new List<string>();

            foreach (string fetchedString in linkedFiles)
            {
                strings.Add(fetchedString);

                bool containsKeyword = false;
                string[] packedKeywords = Config.Get<string[]>("BINPackedKeywords");
                for (int i = 0; i < packedKeywords.Length; i++)
                {
                    if (fetchedString.Contains("_" + packedKeywords[i] + "_", StringComparison.OrdinalIgnoreCase))
                    {
                        containsKeyword = true;
                        break;
                    }
                }


                if (containsKeyword)
                {
                    strings.AddRange(ProcessBINPackedLinkedFile(fetchedString));
                }
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBINPackedLinkedFile(string linkedString)
        {
            List<string> strings = new List<string>();
            string extension = Path.GetExtension(linkedString);
            string[] unpacked = Path.GetFileNameWithoutExtension(linkedString).Split('_');

            string characterName = "";
            int startIndexSkinData = 0;
            for (int i = 0; i < unpacked.Length; i++)
            {
                string splitStringComponent = unpacked[i];
                if (!DetectPackedKeyword(splitStringComponent))
                {
                    characterName += splitStringComponent + "_";
                }
                else
                {
                    characterName = characterName.Remove(characterName.Length - 1, 1);
                    startIndexSkinData = i;
                    break;
                }
            }

            for (int i = startIndexSkinData; i < unpacked.Length; i += 2)
            {
                strings.Add(string.Format("DATA/Characters/{0}/{1}/{2}{3}", characterName, unpacked[i], unpacked[i + 1], extension));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessLegacyDirList(WadEntry entry)
        {
            using Stream entryStream = entry.GetDataHandle().GetDecompressedStream();
            using (BinaryReader br = new BinaryReader(entryStream))
            {
                uint pathCount = br.ReadUInt32();

                for (int i = 0; i < pathCount; i++)
                {
                    yield return Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
                }
            }
        }

        private static bool IsLegacyDirList(ulong hash)
        {
            foreach (string legacyDirList in _legacyDirLists)
            {
                if (hash == XXHash.XXH64(Encoding.ASCII.GetBytes(legacyDirList)))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool DetectPackedKeyword(string packed)
        {
            string[] keywords = Config.Get<string[]>("BINPackedKeywords");

            if (keywords.Contains(packed, StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
