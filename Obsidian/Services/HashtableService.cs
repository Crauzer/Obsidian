using LeagueToolkit.Core.Wad;
using Obsidian.Data;
using Octokit;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Metadata;
using System.Text;
using FileMode = System.IO.FileMode;

namespace Obsidian.Services;

public class HashtableService
{
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

    public HashtableService(Config config)
    {
        this.Config = config;
    }

    public async Task Initialize()
    {
        using HttpClient client = new();

        Directory.CreateDirectory(HASHES_DIRECTORY);
        
        await InitializeHashtables(client);
        InitializeBinHashtables();
    }

    private async Task InitializeHashtables(HttpClient client)
    {
        File.Open(GAME_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(LCU_HASHES_PATH, FileMode.OpenOrCreate).Dispose();

        if(this.Config.SyncHashtables)
            await SyncHashtables(client);

        LoadHashtable(GAME_HASHES_PATH);
        LoadHashtable(LCU_HASHES_PATH);
    }

    private void InitializeBinHashtables()
    {
        File.Open(BIN_FIELDS_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_CLASSES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_HASHES_PATH, FileMode.OpenOrCreate).Dispose();
        File.Open(BIN_OBJECTS_PATH, FileMode.OpenOrCreate).Dispose();

        LoadBinHashtable(BIN_FIELDS_PATH, this.BinProperties);
        LoadBinHashtable(BIN_CLASSES_PATH, this.BinClasses);
        LoadBinHashtable(BIN_HASHES_PATH, this.BinHashes);
        LoadBinHashtable(BIN_OBJECTS_PATH, this.BinObjects);
    }

    private async Task SyncHashtables(HttpClient client)
    {
        GitHubClient github = new(new ProductHeaderValue("Obsidian"));
        IReadOnlyList<RepositoryContent> content = await github.Repository.Content.GetAllContents(
            "CommunityDragon",
            "CDTB",
            "cdragontoolbox"
        );

        RepositoryContent gameHashesContent = content.FirstOrDefault(
            x => x.Name is "hashes.game.txt"
        );
        RepositoryContent lcuHashesContent = content.FirstOrDefault(
            x => x.Name is "hashes.lcu.txt"
        );

        if (gameHashesContent is null)
            throw new InvalidOperationException("CDTB repository does not contain hashes.game.txt");
        if (lcuHashesContent is null)
            throw new InvalidOperationException("CDTB repository does not contain hashes.lcu.txt");


        await SyncGameHashtable(client, gameHashesContent);
        await SyncLcuHashtable(client, lcuHashesContent);
    }

    private async Task SyncGameHashtable(HttpClient client, RepositoryContent gameHashesContent)
    {
        // Hashtable is up to date
        if (
            File.Exists(GAME_HASHES_PATH) && this.Config.GameHashesChecksum == gameHashesContent.Sha
        )
            return;

        using Stream fileContentStream = await client.GetStreamAsync(GAME_HASHES_URL);
        using FileStream fileStream = File.Create(GAME_HASHES_PATH);

        await fileContentStream.CopyToAsync(fileStream);

        this.Config.GameHashesChecksum = gameHashesContent.Sha;
    }

    private async Task SyncLcuHashtable(HttpClient client, RepositoryContent lcuHashesContent)
    {
        // Hashtable is up to date
        if (File.Exists(LCU_HASHES_PATH) && this.Config.LcuHashesChecksum == lcuHashesContent.Sha)
            return;

        using Stream fileContentStream = await client.GetStreamAsync(LCU_HASHES_URL);
        using FileStream fileStream = File.Create(LCU_HASHES_PATH);

        await fileContentStream.CopyToAsync(fileStream);

        this.Config.LcuHashesChecksum = lcuHashesContent.Sha;
    }

    public void LoadHashtable(string hashtablePath)
    {
        using StreamReader reader = new(hashtablePath);
        StringBuilder nameBuilder = new();

        while (reader.EndOfStream is false)
        {
            string line = reader.ReadLine();
            string[] split = line.Split(' ');

            ulong pathHash = ulong.Parse(split[0], NumberStyles.HexNumber);
            nameBuilder = nameBuilder.AppendJoin(' ', split.Skip(1));

            this.Hashes.TryAdd(pathHash, nameBuilder.ToString());

            nameBuilder.Clear();
        }
    }

    private void LoadBinHashtable(string hashtablePath, Dictionary<uint, string> hashtable)
    {
        using StreamReader reader = new(hashtablePath);
        StringBuilder nameBuilder = new();

        while (reader.EndOfStream is false)
        {
            string line = reader.ReadLine();
            string[] split = line.Split(' ');

            uint hash = uint.Parse(split[0], NumberStyles.HexNumber);
            nameBuilder = nameBuilder.AppendJoin(' ', split.Skip(1));

            hashtable.TryAdd(hash, nameBuilder.ToString());
            nameBuilder.Clear();
        }
    }

    public string GetChunkPath(WadChunk chunk)
    {
        if (this.Hashes.TryGetValue(chunk.PathHash, out string existingPath))
            return existingPath;

        return string.Format("{0:x16}", chunk.PathHash);
    }
}
