using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Obsidian.Utilities
{
    public class DiscordRpcContext: IDisposable
    {
        public DiscordRpcTimestampMode TimestampMode { get; set; } = DiscordRpcTimestampMode.LaunchTime;

        private DiscordRpcClient _client;

        private DateTime _launchTime;

        private bool _isDisposed;

        public DiscordRpcContext()
        {
            this._client = new DiscordRpcClient("747894440105869413");
            this._launchTime = DateTime.UtcNow;
        }

        ~DiscordRpcContext()
        {
            Dispose(false);
        }

        public void Initialize()
        {
            this._client.Initialize();
        }
        public void Deinitialize()
        {
            this._client.Deinitialize();
        }

        public void SetPresence(RichPresence presence)
        {
            this._client.SetPresence(presence);
        }
        public void ClearPresence()
        {
            this._client.ClearPresence();
        }

        public void SetIdlePresence()
        {
            SetPresence(ConstructIdlePresence());
        }
        public void SetViewingWadPresence(string wadName)
        {
            SetPresence(ConstructViewingWadPresence(wadName));
        }

        private RichPresence ConstructIdlePresence()
        {
            return new RichPresence()
            {
                State = "Idling",
                Timestamps = GenerateTimestamp(),
                Assets = new Assets()
                {
                    LargeImageKey = "obsidian_logo",
                    SmallImageKey = "idle",
                    LargeImageText = "Obsidian",
                    SmallImageText = "Idling"
                }
            };
        }
        private RichPresence ConstructViewingWadPresence(string wadName)
        {
            return new RichPresence()
            {
                Details = wadName,
                State = "Viewing",
                Timestamps = GenerateTimestamp(),
                Assets = new Assets()
                {
                    LargeImageKey = "obsidian_logo",
                    SmallImageKey = "viewing",
                    LargeImageText = "Obsidian",
                    SmallImageText = "Viewing"
                }
            };
        }

        private Timestamps GenerateTimestamp()
        {
            switch (this.TimestampMode)
            {
                case DiscordRpcTimestampMode.LatestAction: return new Timestamps(DateTime.UtcNow);
                case DiscordRpcTimestampMode.LaunchTime: return new Timestamps(this._launchTime);
                default: return new Timestamps(this._launchTime);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(!this._isDisposed)
            {
                this._client.ClearPresence();
                this._client.Dispose();

                this._isDisposed = true;
            }
        }
    }

    public enum DiscordRpcTimestampMode
    {
        LaunchTime,
        LatestAction
    }
}
