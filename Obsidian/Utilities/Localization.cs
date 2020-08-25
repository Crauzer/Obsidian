using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Obsidian.Utilities
{
    public static class Localization
    {
        public const string LOCALIZATION_FOLDER = "Localization";
        private const string DEFAULT_LOCALIZATION = "English";

        private static Dictionary<string, string> _localization;

        public static Dictionary<string, string> Load()
        {
            string localization = Config.Get<string>("Localization");
            List<string> availableLocalizations = GetAvailableLocalizations();

            //Check if the localization from Config exists
            if(!availableLocalizations.Any(x => x == localization))
            {
                //Localization couldn't be found so we load default english
                _localization = ReadDefaultLocalization();

                Config.Set("Localization", DEFAULT_LOCALIZATION);
            }
            else
            {
                //Load localization
                _localization = ReadLocalization(File.OpenRead(Path.Combine(LOCALIZATION_FOLDER, localization + ".locale.json")));

                //Read default localization and check if there are any missing strings
                //if there are then we add the default ones to current locale
                Dictionary<string, string> defaultLocalization = ReadDefaultLocalization();
                foreach(var defaultLocalizationEntry in defaultLocalization)
                {
                    //If current localization doesn't have this entry then we add it from the default localization
                    if(!_localization.ContainsKey(defaultLocalizationEntry.Key))
                    {
                        _localization.Add(defaultLocalizationEntry.Key, defaultLocalizationEntry.Value);
                    }
                }
            }

            return _localization;
        }

        private static Dictionary<string, string> ReadLocalization(Stream localizationStream)
        {
            byte[] defaultLocalizationBuffer = new byte[localizationStream.Length];
            localizationStream.Read(defaultLocalizationBuffer, 0, defaultLocalizationBuffer.Length);

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(Encoding.UTF8.GetString(defaultLocalizationBuffer));
        }
        private static Dictionary<string, string> ReadDefaultLocalization()
        {
            //Get Stream to Default Localization
            Stream defaultLocalizationStream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"Obsidian.Resources.{DEFAULT_LOCALIZATION}.locale.json");
            if (defaultLocalizationStream == null)
            {
                throw new Exception("Failed to load default localization");
            }

            return ReadLocalization(defaultLocalizationStream);
        }

        public static List<string> GetAvailableLocalizations(bool includeDefault = false)
        {
            if(Directory.Exists(LOCALIZATION_FOLDER))
            {
                List<string> localizations = Directory.EnumerateFiles(LOCALIZATION_FOLDER, "*.locale.json")
                    .Select(x => Path.GetFileNameWithoutExtension(x))
                    .Select(x => Path.GetFileNameWithoutExtension(x))
                    .ToList();

                if (includeDefault && !localizations.Contains(DEFAULT_LOCALIZATION))
                {
                    localizations.Add(DEFAULT_LOCALIZATION);
                }

                return localizations;
            }
            else
            {
                Directory.CreateDirectory(LOCALIZATION_FOLDER);

                return new List<string>();
            }
        }

        public static string Get(string entry)
        {
            if(_localization is null || !_localization.ContainsKey(entry))
            {
                return entry;
            }
            else
            {
                return _localization[entry];
            }
        }
    }
}
