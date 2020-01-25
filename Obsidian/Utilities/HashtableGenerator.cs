using Fantome.Libraries.League.Helpers;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WAD;
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
        public static Dictionary<ulong, string> Generate(WADFile wad)
        {
            Dictionary<ulong, string> hashtable = new Dictionary<ulong, string>();
            List<string> strings = new List<string>();

            foreach (WADEntry entry in wad.Entries.Where(x => x.Type != EntryType.FileRedirection))
            {
                byte[] entryContent = entry.GetContent(true);
                LeagueFileType fileType = LeagueUtilities.GetExtension(entryContent);

                if (fileType == LeagueFileType.BIN)
                {
                    BINFile bin = null;
                    try
                    {
                        bin = new BINFile(new MemoryStream(entryContent));
                    }
                    catch (Exception)
                    {

                    }

                    if (bin != null)
                    {
                        strings.AddRange(ProcessBINLinkedFiles(bin.LinkedFiles));
                        strings.AddRange(ProcessBINFile(bin));
                    }
                }
                
                //This is a file in DATA.wad.client that contains paths of all entries in the WAD
                if(entry.XXHash == 2155072684501898278)
                {
                    using (BinaryReader br = new BinaryReader(new MemoryStream(entry.GetContent(true))))
                    {
                        uint pathCount = br.ReadUInt32();

                        for(int i = 0; i < pathCount; i++)
                        {
                            strings.Add(Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32())));
                        }
                    }
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

            static IEnumerable<string> ProcessBINFile(BINFile bin)
            {
                List<string> strings = new List<string>();

                foreach (BINEntry entry in bin.Entries)
                {
                    strings.AddRange(ProcessBINEntry(entry));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINEntry(BINEntry entry)
            {
                List<string> strings = new List<string>();

                foreach (BINValue value in entry.Values)
                {
                    strings.AddRange(ProcessBINValue(value));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINValue(BINValue value)
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
            static IEnumerable<string> ProcessBINAdditionalData(BINOptional additionalData)
            {
                List<string> strings = new List<string>();

                if (additionalData.Value != null)
                {
                    strings.AddRange(ProcessBINValue(additionalData.Value));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINContainer(BINContainer container)
            {
                List<string> strings = new List<string>();

                foreach (BINValue value in container.Values)
                {
                    strings.AddRange(ProcessBINValue(value));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINStructure(BINStructure structure)
            {
                List<string> strings = new List<string>();

                foreach (BINValue value in structure.Values)
                {
                    strings.AddRange(ProcessBINValue(value));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINMap(BINMap map)
            {
                List<string> strings = new List<string>();

                foreach (KeyValuePair<BINValue, BINValue> valuePair in map.Values)
                {
                    strings.AddRange(ProcessBINValue(valuePair.Key));
                    strings.AddRange(ProcessBINValue(valuePair.Value));
                }

                return strings;
            }
            static IEnumerable<string> ProcessBINLinkedFiles(IEnumerable<string> linkedFiles)
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
            static IEnumerable<string> ProcessBINPackedLinkedFile(string linkedString)
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
            static bool DetectPackedKeyword(string packed)
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
}
