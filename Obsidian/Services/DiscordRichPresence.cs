using DiscordRPC;
using DiscordRPC.Logging;
using Obsidian.Data;
using Serilog;

namespace Obsidian.Services;

public sealed class DiscordRichPresence : IDisposable
{
    private readonly DiscordRpcClient _client;

    private readonly DateTime _startTime;

    private readonly Config _config;

    public DiscordRichPresence(Config config)
    {
        this._config = config;
        this._startTime = DateTime.UtcNow;

        this._client = new("747894440105869413")
        {
            Logger = new ConsoleLogger() { Level = LogLevel.Warning }
        };

        if (this._config.IsRichPresenceEnabled)
        {
            Log.Information("Initializing Discord Rich Presence");

            this._client.Initialize();
            SetPresenceIdle();
        }
    }

    public void SetPresenceIdle() =>
        SetPresence(
            new()
            {
                Timestamps = new(this._startTime),
                Assets = new()
                {
                    LargeImageKey = "obsidian_logo",
                    LargeImageText = "Obsidian",
                }
            }
        );

    public void SetPresence(RichPresence presence)
    {
        if (this._config.IsRichPresenceEnabled)
            this._client.SetPresence(presence);
    }

    public void Dispose()
    {
        this._client?.ClearPresence();
        this._client?.Dispose();
    }
}
