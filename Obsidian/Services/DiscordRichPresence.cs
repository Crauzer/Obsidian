using DiscordRPC;
using DiscordRPC.Logging;

namespace Obsidian.Services;

public sealed class DiscordRichPresence : IDisposable
{
    private readonly DiscordRpcClient _client;

    private readonly DateTime _startTime;

    public DiscordRichPresence()
    {
        this._startTime = DateTime.UtcNow;

        this._client = new("747894440105869413")
        {
            Logger = new ConsoleLogger() { Level = LogLevel.Warning }
        };

        //Connect to the RPC
        this._client.Initialize();

        SetPresenceIdle();
    }

    public void SetPresenceIdle() =>
        this._client.SetPresence(
            new()
            {
                Timestamps = new(this._startTime),
                State = "Idling",
                Assets = new()
                {
                    LargeImageKey = "obsidian_logo",
                    SmallImageKey = "idle",
                    LargeImageText = "Obsidian",
                    SmallImageText = "Idling"
                }
            }
        );

    public void SetPresenceViewing(string name) =>
        this._client.SetPresence(
            new()
            {
                Timestamps = new(this._startTime),
                State = "Viewing",
                Details = name,
                Assets = new()
                {
                    LargeImageKey = "obsidian_logo",
                    SmallImageKey = "viewing",
                    LargeImageText = "Obsidian",
                    SmallImageText = "Viewing"
                }
            }
        );

    public void Dispose()
    {
        this._client?.ClearPresence();
        this._client?.Dispose();
    }
}
