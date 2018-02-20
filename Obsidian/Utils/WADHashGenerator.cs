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
