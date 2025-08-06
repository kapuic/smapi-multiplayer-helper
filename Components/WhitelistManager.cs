// <copyright file="WhitelistManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MultiplayerHelper.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    /// <summary>
    /// Manages player whitelisting functionality.
    /// </summary>
    public class WhitelistManager : IDisposable
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly ModConfig config;
        private readonly string whitelistFilePath;
        private WhitelistData whitelist;
        private FileSystemWatcher fileWatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="WhitelistManager"/> class.
        /// </summary>
        /// <param name="helper">SMAPI mod helper.</param>
        /// <param name="monitor">SMAPI monitor for logging.</param>
        /// <param name="config">Mod configuration.</param>
        public WhitelistManager(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.config = config;
            this.whitelistFilePath = Path.Combine(helper.DirectoryPath, "whitelist.json");

            this.monitor.Log(helper.Translation.Get("debug.whitelist-manager-init"), LogLevel.Debug);

            this.LoadWhitelist();
            this.SetupFileWatcher();

            if (config.WhitelistEnabled)
            {
                helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
                helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            }
        }

        /// <summary>
        /// Enable or disable whitelist functionality.
        /// </summary>
        /// <param name="enabled">Whether to enable whitelist.</param>
        public void SetEnabled(bool enabled)
        {
            if (enabled && !this.config.WhitelistEnabled)
            {
                this.helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
                this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
                this.monitor.Log(this.helper.Translation.Get("whitelist.log-enabled"), LogLevel.Info);
            }
            else if (!enabled && this.config.WhitelistEnabled)
            {
                this.helper.Events.Multiplayer.PeerConnected -= this.OnPeerConnected;
                this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
                this.monitor.Log(this.helper.Translation.Get("whitelist.log-disabled"), LogLevel.Info);
            }
        }

        /// <summary>
        /// Load whitelist from JSON file.
        /// </summary>
        private void LoadWhitelist()
        {
            try
            {
                if (File.Exists(this.whitelistFilePath))
                {
                    this.whitelist = this.helper.Data.ReadJsonFile<WhitelistData>(Path.GetFileName(this.whitelistFilePath)) ?? new WhitelistData();
                    this.monitor.Log(this.helper.Translation.Get("whitelist.log-loaded", new { count = this.whitelist.GetTotalCount() }), LogLevel.Debug);
                }
                else
                {
                    this.whitelist = new WhitelistData();
                    this.SaveWhitelist();
                    this.monitor.Log(this.helper.Translation.Get("whitelist.log-created"), LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-loading", new { error = ex.Message }), LogLevel.Error);
                this.whitelist = new WhitelistData();
            }
        }

        /// <summary>
        /// Save whitelist to JSON file.
        /// </summary>
        private void SaveWhitelist()
        {
            try
            {
                this.helper.Data.WriteJsonFile(Path.GetFileName(this.whitelistFilePath), this.whitelist);
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-saving", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Setup file system watcher for hot-reload of whitelist.
        /// </summary>
        private void SetupFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(this.whitelistFilePath);
                var fileName = Path.GetFileName(this.whitelistFilePath);

                this.fileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true,
                };

                this.fileWatcher.Changed += this.OnWhitelistFileChanged;
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-file-watcher", new { error = ex.Message }), LogLevel.Warn);
            }
        }

        /// <summary>
        /// Handle whitelist file changes for hot-reload.
        /// </summary>
        private void OnWhitelistFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Add a small delay to ensure file write is complete
                System.Threading.Thread.Sleep(100);
                this.LoadWhitelist();
                this.monitor.Log(this.helper.Translation.Get("whitelist.log-reloaded"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-reload", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Handle peer connected events.
        /// </summary>
        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (!this.config.WhitelistEnabled || !Context.IsMainPlayer)
            {
                return;
            }

            try
            {
                var peer = e.Peer;

                // Get player information
                var displayName = this.GetPlayerDisplayName(peer);
                var uniqueId = this.GetPlayerUniqueId(peer);
                var peerId = peer.PlayerID.ToString();

                // Log player join info if enabled
                if (this.config.ShowPlayerJoinInfo)
                {
                    this.monitor.Log(
                        this.helper.Translation.Get("whitelist.log-player-joined", new
                    {
                        displayName = displayName ?? "Unknown",
                        uniqueId = uniqueId ?? "Unknown",
                        peerId = peerId,
                    }), LogLevel.Info);
                }

                // Check whitelist
                if (!this.IsPlayerWhitelisted(displayName, uniqueId, peerId))
                {
                    this.KickPlayer(displayName);
                    this.monitor.Log(this.helper.Translation.Get("whitelist.log-player-kicked", new { player = displayName ?? peerId }), LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-checking", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Handle save loaded events to re-check connected players.
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!this.config.WhitelistEnabled || !Context.IsMainPlayer)
            {
                return;
            }

            // Check existing connected players
            this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTickedOnce;
        }

        /// <summary>
        /// One-time check of connected players after save loaded.
        /// </summary>
        private void OnUpdateTickedOnce(object sender, UpdateTickedEventArgs e)
        {
            this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTickedOnce;

            try
            {
                foreach (var peer in this.helper.Multiplayer.GetConnectedPlayers())
                {
                    var displayName = this.GetPlayerDisplayName(peer);
                    var uniqueId = this.GetPlayerUniqueId(peer);
                    var peerId = peer.PlayerID.ToString();

                    if (!this.IsPlayerWhitelisted(displayName, uniqueId, peerId))
                    {
                        this.KickPlayer(displayName);
                        this.monitor.Log(this.helper.Translation.Get("whitelist.log-player-kicked", new { player = displayName ?? peerId }), LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-checking", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Get player display name from peer or connected farmer.
        /// </summary>
        private string GetPlayerDisplayName(IMultiplayerPeer peer)
        {
            try
            {
                // Try to get from connected farmers first
                var farmer = Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == peer.PlayerID);
                return farmer?.Name;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get player unique ID from peer or connected farmer.
        /// </summary>
        private string GetPlayerUniqueId(IMultiplayerPeer peer)
        {
            try
            {
                var farmer = Game1.getOnlineFarmers().FirstOrDefault(f => f.UniqueMultiplayerID == peer.PlayerID);
                return farmer?.UniqueMultiplayerID.ToString();
            }
            catch
            {
                return peer.PlayerID.ToString();
            }
        }

        /// <summary>
        /// Check if player is whitelisted using any of the three identifier types.
        /// </summary>
        private bool IsPlayerWhitelisted(string displayName, string uniqueId, string peerId)
        {
            // If whitelist is empty, allow all players
            if (this.whitelist.GetTotalCount() == 0)
            {
                return true;
            }

            // Check display name
            if (!string.IsNullOrEmpty(displayName) && this.whitelist.DisplayNames.Contains(displayName))
            {
                return true;
            }

            // Check unique ID
            if (!string.IsNullOrEmpty(uniqueId) && this.whitelist.UniqueIds.Contains(uniqueId))
            {
                return true;
            }

            // Check peer ID
            if (!string.IsNullOrEmpty(peerId) && this.whitelist.PeerIds.Contains(peerId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kick a player using console command.
        /// </summary>
        private void KickPlayer(string playerName)
        {
            try
            {
                if (!string.IsNullOrEmpty(playerName))
                {
                    var command = $"kick {playerName}";
                    Game1.game1.parseDebugInput(command);
                    this.monitor.Log(this.helper.Translation.Get("whitelist.log-kick-executed", new { command }), LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("whitelist.error-kick", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
        {
            this.helper.Events.Multiplayer.PeerConnected -= this.OnPeerConnected;
            this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
            this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTickedOnce;
            this.fileWatcher?.Dispose();
        }
    }

    /// <summary>
    /// Data structure for whitelist JSON file.
    /// </summary>
    public class WhitelistData
    {
        /// <summary>
        /// Gets or sets the list of whitelisted display names.
        /// </summary>
        public List<string> DisplayNames { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of whitelisted unique IDs.
        /// </summary>
        public List<string> UniqueIds { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of whitelisted peer IDs.
        /// </summary>
        public List<string> PeerIds { get; set; } = new List<string>();

        /// <summary>
        /// Get total count of all whitelisted identifiers.
        /// </summary>
        /// <returns></returns>
        public int GetTotalCount()
        {
            return this.DisplayNames.Count + this.UniqueIds.Count + this.PeerIds.Count;
        }
    }
}
