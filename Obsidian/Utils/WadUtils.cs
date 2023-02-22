using LeagueToolkit.Core.Wad;
using XXHash3NET;

namespace Obsidian.Utils;

public static class WadUtils
{
    public static void SaveChunk(WadFile wad, WadChunk chunk, string path)
    {
        using FileStream chunkFileStream = File.Create(path);
        using Stream chunkStream = wad.OpenChunk(chunk);

        chunkStream.CopyTo(chunkFileStream);
    }

    public static void SaveChunk(WadFile wad, WadChunk chunk, string chunkPath, string saveDirectory)
    {
        string filePath = CreateChunkFilePath(saveDirectory, chunkPath);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        SaveChunk(wad, chunk, filePath);
    }

    public static string CreateChunkFilePath(string saveDirectory, string chunkPath)
    {
        string naivePath = Path.Join(saveDirectory, chunkPath);
        if (naivePath.Length <= 260)
            return naivePath;

        return Path.Join(
            saveDirectory,
            string.Format(
                "{0:x16}{1}",
                XXHash64.Compute(chunkPath.ToLower()),
                Path.GetExtension(chunkPath)
            )
        );
    }
}
