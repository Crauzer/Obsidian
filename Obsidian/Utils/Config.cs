using System.Collections.Generic;
using System.IO;

namespace Obsidian.Utils
{
    public static class Config
    {
        private static Dictionary<string, object> _config;

        public static void SetConfig(Dictionary<string, object> config)
        {
            _config = config;
        }

        public static object Get(string key)
        {
            return _config[key];
        }
    }
}
