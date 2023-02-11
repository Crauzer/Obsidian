using LeagueToolkit.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommunityToolkit.HighPerformance.Buffers;
using LeagueToolkit.Core.Meta;
using LeagueToolkit.Core.Meta.Properties;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Hashing;
using XXHash3NET;
using LeagueUtilities = LeagueToolkit.Helpers.Utilities;

namespace Obsidian.Utilities
{
    public static class HashtableGenerator
    {
        private static readonly List<string> LegacyDirLists = new List<string>();

        public static void Initialize()
        {
            LegacyDirLists.Add("data/final/data.wad.legacydirlistinfo");

            for (int i = 0; i < 100; i++)
            {
                LegacyDirLists.Add(string.Format("data/final/maps/shipping/map{0}levels.wad.legacydirlistinfo", i));
            }
        }

        public static Dictionary<ulong, string> Generate(WadFile wad)
        {
            Dictionary<ulong, string> hashtable = new Dictionary<ulong, string>();
            List<string> strings = new List<string>();

            foreach (WadChunk entry in wad.Chunks.Values.Where(x => x.Compression != WadChunkCompression.Satellite))
            {
                using MemoryOwner<byte> entryData = wad.LoadChunkDecompressed(entry);
                LeagueFileType fileType = LeagueUtilities.GetExtensionType(entryData.Span);

                if (fileType == LeagueFileType.PropertyBin)
                {
                    BinTree bin = null;
                    try
                    {
                        bin = new BinTree(new MemoryStream(entryData.Span.ToArray()));
                    }
                    catch (Exception)
                    {

                    }

                    if (bin != null)
                    {
                        strings.AddRange(ProcessBinDependencies(bin.Dependencies));
                        strings.AddRange(ProcessBinTree(bin));
                    }
                }
                else if (IsLegacyDirList(entry.PathHash))
                {
                    strings.AddRange(ProcessLegacyDirList(wad, entry));
                }
            }

            foreach (string fetchedString in strings.Distinct())
            {
                if (!string.IsNullOrEmpty(fetchedString))
                {
                    ulong hash = XXHash64.Compute(fetchedString.ToLower());

                    if (!hashtable.ContainsKey(hash))
                    {
                        hashtable.Add(hash, fetchedString);
                    }
                }
            }

            return hashtable;
        }

        private static IEnumerable<string> ProcessBinTree(BinTree bin)
        {
            List<string> strings = new List<string>();

            foreach (BinTreeObject treeObject in bin.Objects.Values)
            {
                strings.AddRange(ProcessBinTreeObject(treeObject));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeObject(BinTreeObject treeObject)
        {
            List<string> strings = new List<string>();

            foreach (BinTreeProperty treeProperty in treeObject.Properties.Values)
            {
                strings.AddRange(ProcessBinTreeProperty(treeProperty));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeProperty(BinTreeProperty treeProperty)
        {
            return treeProperty switch
            {
                BinTreeString property => ProcessBinTreeString(property),
                BinTreeOptional property => ProcessBinTreeOptional(property),
                BinTreeContainer property => ProcessBinTreeContainer(property),
                BinTreeStruct property => ProcessBinTreeStructure(property),
                BinTreeMap property => ProcessBinTreeMap(property),
                _ => Enumerable.Empty<string>()
            };
        }
        private static IEnumerable<string> ProcessBinTreeString(BinTreeString treeString)
        {
            List<string> strings = new List<string>();
            string value = treeString.Value;

            strings.Add(value);

            if ((value.StartsWith("ASSETS/", true, null) || value.StartsWith("LEVELS/", true, null)) && Path.GetExtension(value) == ".dds")
            {
                int index = value.LastIndexOf('/');
                strings.Add(value.Insert(index + 1, "2x_"));
                strings.Add(value.Insert(index + 1, "4x_"));
            }

            if (treeString.NameHash == Fnv1a.HashLower("mapPath"))
            {
                strings.Add("DATA/" + value + ".materials.bin");
                strings.Add("DATA/" + value + ".mapgeo");
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeOptional(BinTreeOptional optional)
        {
            List<string> strings = new List<string>();

            if (optional.Value is not null)
            {
                strings.AddRange(ProcessBinTreeProperty(optional.Value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeContainer(BinTreeContainer container)
        {
            List<string> strings = new List<string>();

            foreach (BinTreeProperty property in container.Elements)
            {
                strings.AddRange(ProcessBinTreeProperty(property));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeStructure(BinTreeStruct structure)
        {
            List<string> strings = new List<string>();

            foreach (BinTreeProperty property in structure.Properties.Values)
            {
                strings.AddRange(ProcessBinTreeProperty(property));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinTreeMap(BinTreeMap map)
        {
            List<string> strings = new List<string>();

            foreach ((BinTreeProperty key, BinTreeProperty value) in map)
            {
                strings.AddRange(ProcessBinTreeProperty(key));
                strings.AddRange(ProcessBinTreeProperty(value));
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinDependencies(IEnumerable<string> dependencies)
        {
            List<string> strings = new List<string>();

            foreach (string dependency in dependencies)
            {
                strings.Add(dependency);

                bool containsKeyword = false;
                string[] packedKeywords = Config.Get<string[]>("BINPackedKeywords");
                for (int i = 0; i < packedKeywords.Length; i++)
                {
                    if (dependency.Contains("_" + packedKeywords[i] + "_", StringComparison.OrdinalIgnoreCase))
                    {
                        containsKeyword = true;
                        break;
                    }
                }


                if (containsKeyword)
                {
                    strings.AddRange(ProcessBinPackedDependency(dependency));
                }
            }

            return strings;
        }
        private static IEnumerable<string> ProcessBinPackedDependency(string dependency)
        {
            List<string> strings = new List<string>();
            string extension = Path.GetExtension(dependency);
            string[] unpacked = Path.GetFileNameWithoutExtension(dependency).Split('_');

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
        private static IEnumerable<string> ProcessLegacyDirList(WadFile wad, WadChunk entry)
        {
            using Stream entryStream = wad.OpenChunk(entry);
            using BinaryReader br = new BinaryReader(entryStream);
            uint pathCount = br.ReadUInt32();

            for (int i = 0; i < pathCount; i++)
            {
                yield return Encoding.ASCII.GetString(br.ReadBytes(br.ReadInt32()));
            }
        }

        private static bool IsLegacyDirList(ulong hash)
        {
            foreach (string legacyDirList in LegacyDirLists)
            {
                if (hash == XXHash64.Compute(legacyDirList))
                {
                    return true;
                }
            }

            return false;
        }
        private static bool DetectPackedKeyword(string packed)
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
