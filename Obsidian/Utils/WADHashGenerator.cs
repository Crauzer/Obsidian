using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.Helpers.Utilities;
using Fantome.Libraries.League.IO.BIN;
using Fantome.Libraries.League.IO.WAD;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Obsidian.Utils
{
    public static class WADHashGenerator
    {
        public static void GenerateWADStrings(ILog logger, WADFile wad, Dictionary<ulong, string> stringDictionary)
        {
            foreach (WADEntry wadEntry in wad.Entries.Where(x => x.Type == EntryType.Compressed || x.Type == EntryType.ZStandardCompressed))
            {
                byte[] entryData = wadEntry.GetContent(true);
                if (Utilities.GetLeagueFileExtensionType(entryData) == LeagueFileType.BIN)
                {
                    List<string> wadEntryStrings = new List<string>();
                    BINFile bin = null;

                    try
                    {
                        bin = new BINFile(new MemoryStream(entryData));
                        
                    }
                    catch
                    {
                        logger.Warn("Loading Dictionary Strings from WAD Entry: " 
                            +  BitConverter.ToString(BitConverter.GetBytes(wadEntry.XXHash)).Replace("-", "") 
                            + " failed");
                        continue;
                    }

                    wadEntryStrings.AddRange(GetLinkedFileStrings(bin.LinkedFiles).Distinct());

                    foreach (BINFileEntry binEntry in bin.Entries)
                    {
                        foreach (BINFileValue binValue in binEntry.Values.Where(x =>
                        x.Type == BINFileValueType.String || x.Value.GetType() == typeof(BINFileValueList)))
                        {
                            switch (binValue.Type)
                            {
                                case BINFileValueType.String:
                                    wadEntryStrings.Add(binValue.Value as string);
                                    break;
                                case BINFileValueType.PairList:
                                case BINFileValueType.LargeStaticTypeList:
                                case BINFileValueType.List:
                                case BINFileValueType.List2:
                                case BINFileValueType.SmallStaticTypeList:
                                    wadEntryStrings.AddRange(GetValueStrings(binValue));
                                    break;
                            }
                        }
                    }

                    using (XXHash64 xxHash = XXHash64.Create())
                    {
                        wadEntryStrings.ForEach(x =>
                        {
                            if (x != "")
                            {
                                string loweredName = x.ToLower();
                                ulong hash = BitConverter.ToUInt64(xxHash.ComputeHash(Encoding.ASCII.GetBytes(loweredName)), 0);
                                if (!stringDictionary.ContainsKey(hash))
                                {
                                    stringDictionary.Add(hash, x);
                                }
                            }
                        });
                    }
                }
            }
        }

        private static IEnumerable<string> GetLinkedFileStrings(List<string> linkedFiles)
        {
            List<string> characterNames = new List<string>();

            foreach (string linkedFile in linkedFiles)
            {
                if (linkedFile.StartsWith("DATA/Characters"))
                {
                    yield return linkedFile;
                    string withoutStart = linkedFile.Remove(0, 16);
                    characterNames.Add(withoutStart.Substring(0, withoutStart.IndexOf('/')));
                    continue;
                }

                string[] strings = linkedFile.Split(new char[] {'_'}, StringSplitOptions.RemoveEmptyEntries);
                characterNames.Add(strings[0].Remove(0, 5));

                string extension = strings[strings.Length - 1].Substring(strings[strings.Length - 1].IndexOf(".", StringComparison.Ordinal));
                strings[strings.Length - 1] = strings[strings.Length - 1].Replace(extension, "");

                characterNames = characterNames.Distinct().ToList();

                for (int i = 0; i < characterNames.Count; i++)
                {
                    for (int j = 1; j < strings.Length; j += 2)
                    {
                        yield return string.Format("DATA/Characters/{0}/{1}/{2}{3}", characterNames[i], strings[j], strings[j + 1], extension);
                    }

                    yield return string.Format("DATA/Characters/{0}/Skins/Root{1}", characterNames[i], extension);
                }
            }
        }

        private static IEnumerable<string> GetValueStrings(BINFileValue value)
        {
            List<string> strings = new List<string>();
            if (value.Type == BINFileValueType.String)
            {
                strings.Add(value.Value as string);
            }
            else
            {
                if (value.Value is BINFileValueList valueList)
                {
                    foreach (BINFileValue binValue in valueList.Entries.Where(x => x is BINFileValue))
                    {
                        switch (binValue.Type)
                        {
                            case BINFileValueType.String:
                                strings.Add(binValue.Value as string);
                                break;
                            case BINFileValueType.PairList:
                            case BINFileValueType.LargeStaticTypeList:
                            case BINFileValueType.List:
                            case BINFileValueType.List2:
                            case BINFileValueType.SmallStaticTypeList:
                                foreach (BINFileValue binValue2 in ((BINFileValueList)binValue.Value).Entries.Where(x =>
                                    x is BINFileValue &&
                                    ((BINFileValue)x).Type == BINFileValueType.String))
                                {
                                    strings.AddRange(GetValueStrings(binValue2));
                                }
                                break;
                        }
                    }
                }
            }

            return strings.AsEnumerable();
        }
    }
}
