using Fantome.Libraries.League.Helpers;
using Fantome.Libraries.League.Helpers.Cryptography;
using Fantome.Libraries.League.IO.WAD;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LeagueUtilities = Fantome.Libraries.League.Helpers.Utilities;

namespace Obsidian.Utilities
{
    public static class Hashtable
    {
        public const string GAME_HASHTABLE_FILE = "GAME_HASHTABLE.txt";
        public const string LCU_HASHTABLE_FILE = "LCU_HASHTABLE.txt";

        private static Dictionary<ulong, string> _hashtable = new Dictionary<ulong, string>();

        public static string Get(ulong key)
        {
            if (_hashtable.ContainsKey(key))
            {
                return _hashtable[key];
            }
            else
            {
                return key.ToString("x16");
            }
        }
        public static string Get(WADEntry entry)
        {
            if (_hashtable.ContainsKey(entry.XXHash))
            {
                return _hashtable[entry.XXHash];
            }
            else
            {
                string extension = LeagueUtilities.GetExtension(entry.GetContent(true));

                return string.Format("{0}.{1}", entry.XXHash.ToString("x16"), extension);
            }
        }

        public static void Add(Dictionary<ulong, string> hashtable)
        {
            foreach (KeyValuePair<ulong, string> hashPair in hashtable)
            {
                if (!_hashtable.ContainsKey(hashPair.Key))
                {
                    _hashtable.Add(hashPair.Key, hashPair.Value);
                }
            }
        }

        public static void Load()
        {
            Load(GAME_HASHTABLE_FILE);
            Load(LCU_HASHTABLE_FILE);
        }
        public static void Load(string location)
        {
            //Obsidian will support 2 types of hashtables:
            //{hashHex} {string}
            //{string}

            foreach (string line in File.ReadAllLines(location))
            {
                string[] lineSplit = line.Split(' ');
                ulong hash;
                string name = string.Empty;

                if(lineSplit.Length == 1)
                {
                    hash = XXHash.XXH64(Encoding.ASCII.GetBytes(lineSplit[0].ToLower()));
                    name = lineSplit[0];
                }
                else
                {
                    for (int i = 1; i < lineSplit.Length; i++)
                    {
                        name += lineSplit[i];

                        if (i + 1 != lineSplit.Length)
                        {
                            name += ' ';
                        }
                    }

                    hash = ulong.Parse(lineSplit[0], NumberStyles.HexNumber);
                }

                if (!_hashtable.ContainsKey(hash))
                {
                    _hashtable.Add(hash, name);
                }
            }
        }

        public static void Write(string location, Dictionary<ulong, string> hashtable)
        {
            using (StreamWriter sw = new StreamWriter(File.Create(location)))
            {
                foreach (KeyValuePair<ulong, string> hashPair in hashtable)
                {
                    sw.WriteLine(string.Format("{0} {1}", hashPair.Key.ToString("X16"), hashPair.Value));
                }
            }
        }
    }
}