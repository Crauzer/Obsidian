using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;

namespace Obsidian.Utilities
{
    public static class Hashtable
    {
        public const string GAME_HASHTABLE_FILE = "GAME_HASHTABLE.txt";
        public const string LCU_HASHTABLE_FILE = "LCU_HASHTABLE.txt";

        private static Dictionary<ulong, string> _hashtable = new Dictionary<ulong, string>();

        public static string Get(ulong key)
        {
            if(_hashtable.ContainsKey(key))
            {
                return _hashtable[key];
            }
            else 
            {
                return key.ToString("x16");
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

            foreach(string line in File.ReadAllLines(location))
            {
                string[] lineSplit = line.Split(' ');
                
                ulong hash = ulong.Parse(lineSplit[0], NumberStyles.HexNumber);
                
                //Since names can have spaces in them
                string name = "";
                for(int i = 1; i < lineSplit.Length; i++)
                {
                    name += lineSplit[i];
                }

                _hashtable.Add(hash, name);
            }
        }
    }
}
