using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fantome.Libraries.League.Helpers;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WAD;
using log4net;
using Newtonsoft.Json.Linq;

namespace Obsidian.Utils
{
    public static class WADHashGenerator
    {
        public static void GenerateWADStrings(ILog logger, WADFile wad, Dictionary<ulong, string> stringDictionary)
        {
            List<string> strings = new List<string>();

            foreach (WADEntry entry in wad.Entries.Where(x => x.Type != EntryType.FileRedirection))
            {
                byte[] entryContent = entry.GetContent(true);
                LeagueFileType fileType = Utilities.GetLeagueFileExtensionType(entryContent);

                if (fileType == LeagueFileType.BIN)
                {
                    BINFile bin = null;
                    try
                    {
                        bin = new BINFile(new MemoryStream(entryContent));
                    }
                    catch (Exception excp)
                    {
                        Logging.LogException(logger, "There was an error while reading a BIN file", excp);
                    }

                    if (bin != null)
                    {
                        strings.AddRange(ProcessBINLinkedFiles(bin.LinkedFiles));
                        strings.AddRange(ProcessBINFile(bin));
                    }
                }
            }

            strings = strings.Distinct().ToList();

            using (XXHash64 xxHash = XXHash64.Create())
            {
                foreach (string fetchedString in strings)
                {
                    if (!string.IsNullOrEmpty(fetchedString))
                    {
                        ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(fetchedString.ToLower())), 0);

                        if (!stringDictionary.ContainsKey(hash))
                        {
                            stringDictionary.Add(hash, fetchedString);
                        }
                    }
                }
            }
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

                if(value.Property == Cryptography.FNV32Hash("mapPath"))
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

            if(additionalData.Value != null)
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

        private static IEnumerable<string> ProcessBINLinkedFiles(IEnumerable<string> linkedFiles)
        {
            List<string> strings = new List<string>();

            foreach (string fetchedString in linkedFiles)
            {
                strings.Add(fetchedString);

                bool containsKeyword = false;
                string[] packedKeywords = (Config.Get("BinPackedKeywords") as JArray).ToObject<string[]>();
                for (int i = 0; i < packedKeywords.Length; i++)
                {
                    if(fetchedString.Contains("_" + packedKeywords[i] + "_"))
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

        private static bool DetectPackedKeyword(string packed)
        {
            string[] keywords = (Config.Get("BinPackedKeywords") as JArray).ToObject<string[]>();

            if (keywords.Contains(packed))
            {
                return true;
            }

            return false;
        }
    }
}
