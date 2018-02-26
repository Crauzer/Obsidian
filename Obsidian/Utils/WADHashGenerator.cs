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
            List<string> strings = new List<string>();

            foreach (WADEntry entry in wad.Entries.Where(x => x.Type != EntryType.FileRedirection))
            {
                byte[] entryContent = entry.GetContent(true);
                LeagueFileType fileType = Utilities.GetLeagueFileExtensionType(entryContent);

                if (fileType == LeagueFileType.BIN)
                {
                    strings.AddRange(ProcessBINFile(new BINFile(new MemoryStream(entryContent))));
                }
            }

            using (XXHash64 xxHash = XXHash64.Create())
            {
                for (int i = 0; i < strings.Count; i++)
                {
                    string fetchedString = strings[i];
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

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINEntry(BINFileEntry entry)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in entry.Values)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINValue(BINFileValue value)
        {
            List<string> strings = new List<string>();

            if (value.Type == BINFileValueType.String)
            {
                strings.Add(value.Value as string);
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

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINAdditionalData(BINFileAdditionalData additionalData)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in additionalData.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINContainer(BINFileContainer container)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in container.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINStructure(BINFileStructure structure)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValue value in structure.Entries)
            {
                strings.AddRange(ProcessBINValue(value));
            }

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINMap(BINFileMap map)
        {
            List<string> strings = new List<string>();

            foreach (BINFileValuePair valuePair in map.Entries)
            {
                strings.AddRange(ProcessBINValue(valuePair.Pair.Key));
                strings.AddRange(ProcessBINValue(valuePair.Pair.Value));
            }

            return strings.AsEnumerable();
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

                string[] strings = linkedFile.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
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
    }
}
