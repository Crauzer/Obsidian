using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WAD;
using log4net;

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

            foreach (BINFileEntry entry in bin.Entries)
            {
                strings.AddRange(ProcessBINEntry(entry));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINEntry(BINFileEntry entry)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in entry.Values)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINValue(BINFileValue value)
        {
            List<string> strings = new List<string>();

            if (value.Type == BINFileValueType.String)
            {
                string valueString = value.Value as string;
                strings.Add(valueString);

                if ((valueString.StartsWith("ASSETS/", true, null) || valueString.StartsWith("LEVELS/", true, null)) && Path.GetExtension(valueString) == ".dds")
                {
                    int index = valueString.LastIndexOf('/');
                    strings.Add(valueString.Insert(index + 1, "2x_"));
                    strings.Add(valueString.Insert(index + 1, "4x_"));
                }
            }
            else if (value.Type == BINFileValueType.AdditionalOptionalData)
            {
                strings.AddRange(ProcessBINAdditionalData(value.Value as BINFileAdditionalData));
            }
            else if (value.Type == BINFileValueType.Container)
            {
                strings.AddRange(ProcessBINContainer(value.Value as BINFileContainer));
            }
            else if (value.Type == BINFileValueType.Embedded || value.Type == BINFileValueType.Structure)
            {
                strings.AddRange(ProcessBINStructure(value.Value as BINFileStructure));
            }
            else if (value.Type == BINFileValueType.Map)
            {
                strings.AddRange(ProcessBINMap(value.Value as BINFileMap));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINAdditionalData(BINFileAdditionalData additionalData)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in additionalData.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINContainer(BINFileContainer container)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in container.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINStructure(BINFileStructure structure)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in structure.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINMap(BINFileMap map)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValuePair valuePair in map.Entries)
            {
                strings.AddRange(ProcessBINValue(valuePair.Pair.Key));
                strings.AddRange(ProcessBINValue(valuePair.Pair.Value));
            }

            return strings;
        }

        private static IEnumerable<string> ProcessBINLinkedFiles(IEnumerable<string> linkedFiles)
        {
            List<string> strings = new List<string>();

            foreach (string fetchedString in linkedFiles)
            {
                strings.Add(fetchedString);
                if (fetchedString.Contains("_Skins_Skin"))
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
                if (splitStringComponent != "Skins" && splitStringComponent != "Tiers")
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
    }
}
