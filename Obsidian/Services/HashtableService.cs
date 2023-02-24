using LeagueToolkit.Core.Wad;
using Obsidian.Data;
using Octokit;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Obsidian.Services;

public class HashtableService
{
    public Config Config { get; }

    public Dictionary<ulong, string> Hashes { get; private set; } = new();

    private const string GAME_HASHES_URL =
        "https://github.com/CommunityDragon/CDTB/raw/master/cdragontoolbox/hashes.game.txt";
    private const string LCU_HASHES_URL =
        "https://github.com/CommunityDragon/CDTB/raw/master/cdragontoolbox/hashes.lcu.txt";

    private const string HASHES_DIRECTORY = "hashes";
    private const string GAME_HASHES_FILE = "hashes/hashes.game.txt";
    private const string LCU_HASHES_FILE = "hashes/hashes.lcu.txt";

    public HashtableService(Config config)
    {
        this.Config = config;
    }

    public async Task Initialize()
    {
        using HttpClient client = new();

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

        Directory.CreateDirectory(HASHES_DIRECTORY);

        await SyncGameHashtable(client, gameHashesContent);
        await SyncLcuHashtable(client, lcuHashesContent);

        LoadHashtable(GAME_HASHES_FILE);
        LoadHashtable(LCU_HASHES_FILE);
    }

    private async Task SyncGameHashtable(HttpClient client, RepositoryContent gameHashesContent)
    {
        // Hashtable is up to date
        if (
            File.Exists(GAME_HASHES_FILE) && this.Config.GameHashesChecksum == gameHashesContent.Sha
        )
            return;

        using Stream fileContentStream = await client.GetStreamAsync(GAME_HASHES_URL);
        using FileStream fileStream = File.Create(GAME_HASHES_FILE);

        await fileContentStream.CopyToAsync(fileStream);

        this.Config.GameHashesChecksum = gameHashesContent.Sha;
    }

    private async Task SyncLcuHashtable(HttpClient client, RepositoryContent lcuHashesContent)
    {
        // Hashtable is up to date
        if (File.Exists(LCU_HASHES_FILE) && this.Config.LcuHashesChecksum == lcuHashesContent.Sha)
            return;

        using Stream fileContentStream = await client.GetStreamAsync(LCU_HASHES_URL);
        using FileStream fileStream = File.Create(LCU_HASHES_FILE);

        await fileContentStream.CopyToAsync(fileStream);

        this.Config.LcuHashesChecksum = lcuHashesContent.Sha;
    }

    public void LoadHashtable(string hashtablePath)
    {
        using StreamReader reader = new(hashtablePath);
        StringBuilder nameBuilder = new();

        do
        {
            string line = reader.ReadLine();
            string[] split = line.Split(' ');

            ulong pathHash = ulong.Parse(split[0], NumberStyles.HexNumber);
            nameBuilder = nameBuilder.AppendJoin(' ', split.Skip(1));

            this.Hashes.TryAdd(pathHash, nameBuilder.ToString());

            nameBuilder.Clear();
        } while (reader.EndOfStream is false);
    }

    public string GetChunkPath(WadChunk chunk) 
    {
        if(this.Hashes.TryGetValue(chunk.PathHash, out string existingPath))
            return existingPath;

        return string.Format("{0:x16}", chunk.PathHash);
    }
}
