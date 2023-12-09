using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Utils;
using Obsidian.Data;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using FileMode = System.IO.FileMode;

namespace Obsidian.Services;

public class HashtableService {
    public Config Config { get; }

    public Dictionary<ulong, string> Hashes { get; private set; } = new();

    public Dictionary<uint, string> BinClasses { get; private set; } = new();
    public Dictionary<uint, string> BinProperties { get; private set; } = new();
    public Dictionary<uint, string> BinHashes { get; private set; } = new();
    public Dictionary<uint, string> BinObjects { get; private set; } = new();

    private const string HASHES_BASE_URL = "https://raw.communitydragon.org/data/hashes/lol/";

    private const string HASHES_DIRECTORY = "hashes";
    private const string GAME_HASHES_FILENAME = "hashes.game.txt";
    private const string LCU_HASHES_FILENAME = "hashes.lcu.txt";
    private const string GAME_HASHES_PATH = $"{HASHES_DIRECTORY}/hashes.game.txt";
    private const string LCU_HASHES_PATH = $"{HASHES_DIRECTORY}/hashes.lcu.txt";

    private const string BIN_FIELDS_FILENAME = "hashes.binfields.txt";
    private const string BIN_CLASSES_FILENAME = "hashes.bintypes.txt";
    private const string BIN_HASHES_FILENAME = "hashes.binhashes.txt";
    private const string BIN_OBJECTS_FILENAME = "hashes.binentries.txt";
    private const string BIN_FIELDS_PATH = $"{HASHES_DIRECTORY}/hashes.binfields.txt";
    private const string BIN_CLASSES_PATH = $"{HASHES_DIRECTORY}/hashes.bintypes.txt";
    private const string BIN_HASHES_PATH = $"{HASHES_DIRECTORY}/hashes.binhashes.txt";
    private const string BIN_OBJECTS_PATH = $"{HASHES_DIRECTORY}/hashes.binentries.txt";

    public HashtableService(Config config) {
        this.Config = config;
    }

    public async Task Initialize() {
        using HttpClient client = new();

        Directory.CreateDirectory(HASHES_DIRECTORY);

        if (this.Config.SyncHashtables) {
            string hashFilesHtml = await client.GetStringAsync(HASHES_BASE_URL);
            await SyncHashtables(client, hashFilesHtml);
            await SyncBinHashtables(client, hashFilesHtml);
        }

        this.InitializeHashtables();
        this.InitializeBinHashtables();
    }

    private void InitializeHashtables() {
        Log.Information("Initializing hashtables");

        File.Open(GAME_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(LCU_HASHES_PATH, FileMode.OpenOrCreate).Dispose();

        LoadHashtable(GAME_HASHES_PATH);
        LoadHashtable(LCU_HASHES_PATH);
    }

    private void InitializeBinHashtables() {
        Log.Information("Initializing BIN hashtables");

        File.Open(BIN_FIELDS_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_CLASSES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_OBJECTS_PATH, FileMode.OpenOrCreate).Dispose();

        LoadBinHashtable(BIN_FIELDS_PATH, this.BinProperties);
        LoadBinHashtable(BIN_CLASSES_PATH, this.BinClasses);
        LoadBinHashtable(BIN_HASHES_PATH, this.BinHashes);
        LoadBinHashtable(BIN_OBJECTS_PATH, this.BinObjects);
    }

    private async Task SyncHashtables(HttpClient client, string hashFilesHtml) {
        Log.Information("Syncing WAD hashtables");

        this.Config.GameHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + GAME_HASHES_FILENAME,
            GAME_HASHES_PATH,
            this.Config.GameHashesLastUpdate
        );
        this.Config.LcuHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + LCU_HASHES_FILENAME,
            LCU_HASHES_PATH,
            this.Config.LcuHashesLastUpdate
        );
    }

    private async Task SyncBinHashtables(HttpClient client, string hashFilesHtml) {
        Log.Information("Syncing BIN hashtables");

        this.Config.BinFieldsHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + BIN_FIELDS_FILENAME,
            BIN_FIELDS_PATH,
            this.Config.BinFieldsHashesLastUpdate
        );
        this.Config.BinTypesHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + BIN_CLASSES_FILENAME,
            BIN_CLASSES_PATH,
            this.Config.BinTypesHashesLastUpdate
        );
        this.Config.BinHashesHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + BIN_HASHES_FILENAME,
            BIN_HASHES_PATH,
            this.Config.BinHashesHashesLastUpdate
        );
        this.Config.BinEntriesHashesLastUpdate = await SyncHashtable(
            client,
            hashFilesHtml,
            HASHES_BASE_URL + BIN_OBJECTS_FILENAME,
            BIN_OBJECTS_PATH,
            this.Config.BinEntriesHashesLastUpdate
        );
    }

    private static async Task<DateTime> SyncHashtable(
        HttpClient client,
        string hashFilesHtml,
        string url,
        string path,
        DateTime lastUpdateTime
    ) {
        // Hashtable is up to date
        DateTime serverTime = ParseServerUpdateTime(hashFilesHtml, Path.GetFileName(url));
        if (serverTime == lastUpdateTime) {
            Log.Information($"{path} is up to date");
            return lastUpdateTime;
        }

        Log.Information($"Downloading hashtable: {path} from {url}");

        using Stream fileContentStream = await client.GetStreamAsync(url);
        using FileStream fileStream = File.Create(path);

        await fileContentStream.CopyToAsync(fileStream);

        return serverTime;
    }

    private static DateTime ParseServerUpdateTime(string html, string fileName) {
        var match = Regex.Match(html, $"""<a href="{fileName}\">{fileName}</a> *([^ ]+) *([^ ]+)""");
        if (!match.Success) throw new Exception($"Failed to find entry for file {fileName}");

        var date = DateOnly.Parse(match.Groups[1].Value, DateTimeFormatInfo.InvariantInfo);
        var time = TimeOnly.Parse(match.Groups[2].Value, DateTimeFormatInfo.InvariantInfo);
        return date.ToDateTime(time);
    }

    public void LoadHashtable(string hashtablePath) {
        using StreamReader reader = new(hashtablePath);

        while (reader.ReadLine() is string line) {
            var separatorIndex = line.IndexOf(' ');

            ulong pathHash = ulong.Parse(line.AsSpan(0, separatorIndex), NumberStyles.HexNumber);

            this.Hashes.TryAdd(pathHash, line[(separatorIndex+1)..]);
        }
    }

    private static void LoadBinHashtable(string hashtablePath, Dictionary<uint, string> hashtable) {
        using StreamReader reader = new(hashtablePath);

        while (reader.ReadLine() is string line) {
            string[] split = line.Split(' ', 2);

            uint hash = uint.Parse(split[0], NumberStyles.HexNumber);

            hashtable.TryAdd(hash, split[1]);
        }
    }

    public string GetChunkPath(WadChunk chunk) {
        if (this.Hashes.TryGetValue(chunk.PathHash, out string existingPath))
            return existingPath;

        return string.Format("{0:x16}", chunk.PathHash);
    }

    public bool TryGetChunkPath(WadChunk chunk, out string path) =>
        this.Hashes.TryGetValue(chunk.PathHash, out path);

    public static string GuessChunkPath(WadChunk chunk, WadFile wad) {
        string extension = chunk.Compression switch {
            WadChunkCompression.Satellite => null,
            _ => GuessChunkExtension(chunk, wad)
        };

        return string.IsNullOrEmpty(extension) switch {
            true => string.Format("{0:x16}", chunk.PathHash),
            false => string.Format("{0:x16}.{1}", chunk.PathHash, extension),
        };

        static string GuessChunkExtension(WadChunk chunk, WadFile wad) {
            using Stream stream = wad.LoadChunkDecompressed(chunk).AsStream();

            return LeagueFile.GetExtension(LeagueFile.GetFileType(stream));
        }
    }
}