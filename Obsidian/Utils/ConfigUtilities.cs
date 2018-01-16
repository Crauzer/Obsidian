using log4net.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Obsidian.Utils
{
    public static class ConfigUtilities
    {
        public static readonly Dictionary<string, object> DefaultConfig = new Dictionary<string, object>()
        {
            { "LoggingPattern", "[%utcdate{ABSOLUTE}] | [%-5level] | [%-20logger: (%-4line)] | %message%newline" },
            { "LogLevel", Level.Info },
            { "WadSaveMajorVersion", 3L },
            { "WadSaveMinorVersion", 0L },
            { "GenerateWadDictionary", true },
            { "ParallelExtraction", true }
        };

        public static Dictionary<string, object> ReadConfig()
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText("config.json"));
        }

        public static void WriteDefaultConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(DefaultConfig, Formatting.Indented));
        }
    }
}
