﻿using CommunityToolkit.HighPerformance;
using LeagueToolkit.Core.Wad;
using LeagueToolkit.Utils;
using Obsidian.Data;
using Octokit;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using FileMode = System.IO.FileMode;

namespace Obsidian.Services;

public class HashtableService {
    public Config Config { get; }

    public Dictionary<ulong, string> Hashes { get; private set; } = new();

    public Dictionary<uint, string> BinClasses { get; private set; } = new();
    public Dictionary<uint, string> BinProperties { get; private set; } = new();
    public Dictionary<uint, string> BinHashes { get; private set; } = new();
    public Dictionary<uint, string> BinObjects { get; private set; } = new();

    private const string GAME_HASHES_URL =
        "https://github.com/CommunityDragon/CDTB/raw/master/cdragontoolbox/hashes.game.txt";
    private const string LCU_HASHES_URL =
        "https://github.com/CommunityDragon/CDTB/raw/master/cdragontoolbox/hashes.lcu.txt";

    private const string HASHES_DIRECTORY = "hashes";
    private const string GAME_HASHES_PATH = "hashes/hashes.game.txt";
    private const string LCU_HASHES_PATH = "hashes/hashes.lcu.txt";

    private const string BIN_FIELDS_PATH = "hashes/hashes.binfields.txt";
    private const string BIN_CLASSES_PATH = "hashes/hashes.bintypes.txt";
    private const string BIN_HASHES_PATH = "hashes/hashes.binhashes.txt";
    private const string BIN_OBJECTS_PATH = "hashes/hashes.binentries.txt";

    public HashtableService(Config config) {
        this.Config = config;
    }

    public async Task Initialize() {
        using HttpClient client = new();

        Directory.CreateDirectory(HASHES_DIRECTORY);

        await InitializeHashtables(client);
        await InitializeBinHashtables(client);
    }

    private async Task InitializeHashtables(HttpClient client) {
        Log.Information("Initializing hashtables");

        File.Open(GAME_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(LCU_HASHES_PATH, FileMode.OpenOrCreate).Dispose();

        if (this.Config.SyncHashtables)
            await SyncHashtables(client);

        LoadHashtable(GAME_HASHES_PATH);
        LoadHashtable(LCU_HASHES_PATH);
    }

    private async Task InitializeBinHashtables(HttpClient client) {
        Log.Information("Initializing BIN hashtables");

        File.Open(BIN_FIELDS_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_CLASSES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_OBJECTS_PATH, FileMode.OpenOrCreate).Dispose();

        if (this.Config.SyncHashtables)
            await SyncBinHashtables(client);

        LoadBinHashtable(BIN_FIELDS_PATH, this.BinProperties);
        LoadBinHashtable(BIN_CLASSES_PATH, this.BinClasses);
        LoadBinHashtable(BIN_HASHES_PATH, this.BinHashes);
        LoadBinHashtable(BIN_OBJECTS_PATH, this.BinObjects);
    }

    private async Task SyncHashtables(HttpClient client) {
        Log.Information("Syncing WAD hashtables");

        GitHubClient github = new(new ProductHeaderValue("Obsidian"));
        IReadOnlyList<RepositoryContent> content = await github.Repository.Content.GetAllContents(
            "CommunityDragon",
            "CDTB",
            "cdragontoolbox"
        );

        RepositoryContent gameHashesContent = GetRepositoryContent(content, "hashes.game.txt");
        RepositoryContent lcuHashesContent = GetRepositoryContent(content, "hashes.lcu.txt");

        this.Config.GameHashesChecksum = await SyncHashtable(
            client,
            gameHashesContent,
            GAME_HASHES_URL,
            GAME_HASHES_PATH,
            this.Config.GameHashesChecksum
        );
        this.Config.LcuHashesChecksum = await SyncHashtable(
            client,
            lcuHashesContent,
            LCU_HASHES_URL,
            LCU_HASHES_PATH,
            this.Config.LcuHashesChecksum
        );
    }

    private async Task SyncBinHashtables(HttpClient client) {
        Log.Information("Syncing BIN hashtables");

        GitHubClient github = new(new ProductHeaderValue("Obsidian"));
        IReadOnlyList<RepositoryContent> content = await github.Repository.Content.GetAllContents(
            "CommunityDragon",
            "CDTB",
            "cdragontoolbox"
        );

        RepositoryContent fieldsContent = GetRepositoryContent(content, "hashes.binfields.txt");
        RepositoryContent typesContent = GetRepositoryContent(content, "hashes.bintypes.txt");
        RepositoryContent hashesContent = GetRepositoryContent(content, "hashes.binhashes.txt");
        RepositoryContent entriesContent = GetRepositoryContent(content, "hashes.binentries.txt");

        this.Config.BinFieldsHashesChecksum = await SyncHashtable(
            client,
            fieldsContent,
            fieldsContent.DownloadUrl,
            BIN_FIELDS_PATH,
            this.Config.BinFieldsHashesChecksum
        );
        this.Config.BinTypesHashesChecksum = await SyncHashtable(
            client,
            typesContent,
            typesContent.DownloadUrl,
            BIN_CLASSES_PATH,
            this.Config.BinTypesHashesChecksum
        );
        this.Config.BinHashesHashesChecksum = await SyncHashtable(
            client,
            hashesContent,
            hashesContent.DownloadUrl,
            BIN_HASHES_PATH,
            this.Config.BinHashesHashesChecksum
        );
        this.Config.BinEntriesHashesChecksum = await SyncHashtable(
            client,
            entriesContent,
            entriesContent.DownloadUrl,
            BIN_OBJECTS_PATH,
            this.Config.BinEntriesHashesChecksum
        );
    }

    private static async Task<string> SyncHashtable(
        HttpClient client,
        RepositoryContent content,
        string url,
        string path,
        string checksum
    ) {
        // Hashtable is up to date
        if (checksum == content.Sha) {
            Log.Information($"{path} is up to date");
            return checksum;
        }

        Log.Information($"Downloading hashtable: {path} from {url}");

        using Stream fileContentStream = await client.GetStreamAsync(url);
        using FileStream fileStream = File.Create(path);

        await fileContentStream.CopyToAsync(fileStream);

        return content.Sha;
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

    private static RepositoryContent GetRepositoryContent(
        IReadOnlyList<RepositoryContent> content,
        string name
    ) {
        RepositoryContent foundContent = content.FirstOrDefault(x => x.Name == name);
        if (foundContent is null)
            throw new InvalidOperationException($"Failed to find {name} in repository");

        return foundContent;
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