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
                        strings = strings.Distinct().ToList();

                        strings.AddRange(ProcessBINFile(bin));
                    }
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
                string valueString = value.Value as string;
                strings.Add(valueString);

                if (valueString.Contains('/') && Path.GetExtension(valueString) == ".dds")
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

        private static IEnumerable<string> ProcessBINLinkedFiles(List<string> linkedFiles)
        {
            List<string> strings = new List<string>();

            for (int i = 0; i < linkedFiles.Count; i++)
            {
                string fetchedString = linkedFiles[i];

                strings.Add(fetchedString);
                strings.AddRange(ProcessBINPackedLinkedFile(fetchedString));
            }

            return strings.AsEnumerable();
        }

        private static IEnumerable<string> ProcessBINPackedLinkedFile(string linkedString)
        {
            List<string> strings = new List<string>();
            string stringToProcess = linkedString;

            try
            {
                string characterName = linkedString.Substring(5, linkedString.IndexOf('_') - 5);
                string extension = linkedString.Substring(linkedString.LastIndexOf('.'), linkedString.Length - linkedString.LastIndexOf('.'));

                stringToProcess = stringToProcess.Remove(0, 5 + characterName.Length + 1);
                stringToProcess = stringToProcess.Remove(stringToProcess.Length - extension.Length);

                while (stringToProcess.Length != 0)
                {
                    string directoryName = stringToProcess.Substring(0, stringToProcess.IndexOf('_'));
                    stringToProcess = stringToProcess.Remove(0, directoryName.Length + 1);

                    string skin = "";
                    int indexOfUnderscore = stringToProcess.IndexOf('_');
                    if (indexOfUnderscore != -1)
                    {
                        skin = stringToProcess.Substring(0, stringToProcess.IndexOf('_'));
                        stringToProcess = stringToProcess.Remove(0, skin.Length + 1);
                    }
                    else
                    {
                        skin = stringToProcess;
                        stringToProcess = stringToProcess.Remove(0);
                    }

                    strings.Add(string.Format("DATA/Characters/{0}/{1}/{2}{3}", characterName, directoryName, skin, extension));
                }
            }
            catch (Exception excp)
            {
                strings.Add(linkedString);
            }

            return strings.AsEnumerable();
        }
    }
}