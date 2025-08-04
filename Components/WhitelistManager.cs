using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerHelper.Components
{
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
            
            LoadWhitelist();
            SetupFileWatcher();
            
            if (config.WhitelistEnabled)
            {
                helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
                helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            }
        }

        /// <summary>
        /// Enable or disable whitelist functionality.
        /// </summary>
        /// <param name="enabled">Whether to enable whitelist.</param>
        public void SetEnabled(bool enabled)
        {
            if (enabled && !config.WhitelistEnabled)
            {
                helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
                helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
                monitor.Log(helper.Translation.Get("whitelist.log-enabled"), LogLevel.Info);
            }
            else if (!enabled && config.WhitelistEnabled)
            {
                helper.Events.Multiplayer.PeerConnected -= OnPeerConnected;
                helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
                monitor.Log(helper.Translation.Get("whitelist.log-disabled"), LogLevel.Info);
            }
        }

        /// <summary>
        /// Load whitelist from JSON file.
        /// </summary>
        private void LoadWhitelist()
        {
            try
            {
                if (File.Exists(whitelistFilePath))
                {
                    whitelist = helper.Data.ReadJsonFile<WhitelistData>(Path.GetFileName(whitelistFilePath)) ?? new WhitelistData();
                    monitor.Log(helper.Translation.Get("whitelist.log-loaded", new { count = whitelist.GetTotalCount() }), LogLevel.Debug);
                }
                else
                {
                    whitelist = new WhitelistData();
                    SaveWhitelist();
                    monitor.Log(helper.Translation.Get("whitelist.log-created"), LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-loading", new { error = ex.Message }), LogLevel.Error);
                whitelist = new WhitelistData();
            }
        }

        /// <summary>
        /// Save whitelist to JSON file.
        /// </summary>
        private void SaveWhitelist()
        {
            try
            {
                helper.Data.WriteJsonFile(Path.GetFileName(whitelistFilePath), whitelist);
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-saving", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Setup file system watcher for hot-reload of whitelist.
        /// </summary>
        private void SetupFileWatcher()
        {
            try
            {
                var directory = Path.GetDirectoryName(whitelistFilePath);
                var fileName = Path.GetFileName(whitelistFilePath);

                fileWatcher = new FileSystemWatcher(directory, fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    EnableRaisingEvents = true
                };

                fileWatcher.Changed += OnWhitelistFileChanged;
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-file-watcher", new { error = ex.Message }), LogLevel.Warn);
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
                LoadWhitelist();
                monitor.Log(helper.Translation.Get("whitelist.log-reloaded"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-reload", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Handle peer connected events.
        /// </summary>
        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (!config.WhitelistEnabled || !Context.IsMainPlayer)
                return;

            try
            {
                var peer = e.Peer;
                
                // Get player information
                var displayName = GetPlayerDisplayName(peer);
                var uniqueId = GetPlayerUniqueId(peer);
                var peerId = peer.PlayerID.ToString();

                // Log player join info if enabled
                if (config.ShowPlayerJoinInfo)
                {
                    monitor.Log(helper.Translation.Get("whitelist.log-player-joined", new 
                    { 
                        displayName = displayName ?? "Unknown",
                        uniqueId = uniqueId ?? "Unknown", 
                        peerId = peerId 
                    }), LogLevel.Info);
                }

                // Check whitelist
                if (!IsPlayerWhitelisted(displayName, uniqueId, peerId))
                {
                    KickPlayer(displayName);
                    monitor.Log(helper.Translation.Get("whitelist.log-player-kicked", new { player = displayName ?? peerId }), LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-checking", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Handle save loaded events to re-check connected players.
        /// </summary>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!config.WhitelistEnabled || !Context.IsMainPlayer)
                return;

            // Check existing connected players
            helper.Events.GameLoop.UpdateTicked += OnUpdateTickedOnce;
        }

        /// <summary>
        /// One-time check of connected players after save loaded.
        /// </summary>
        private void OnUpdateTickedOnce(object sender, UpdateTickedEventArgs e)
        {
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTickedOnce;
            
            try
            {
                foreach (var peer in helper.Multiplayer.GetConnectedPlayers())
                {
                    var displayName = GetPlayerDisplayName(peer);
                    var uniqueId = GetPlayerUniqueId(peer);
                    var peerId = peer.PlayerID.ToString();

                    if (!IsPlayerWhitelisted(displayName, uniqueId, peerId))
                    {
                        KickPlayer(displayName);
                        monitor.Log(helper.Translation.Get("whitelist.log-player-kicked", new { player = displayName ?? peerId }), LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-checking", new { error = ex.Message }), LogLevel.Error);
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
            if (whitelist.GetTotalCount() == 0)
                return true;

            // Check display name
            if (!string.IsNullOrEmpty(displayName) && whitelist.DisplayNames.Contains(displayName))
                return true;

            // Check unique ID
            if (!string.IsNullOrEmpty(uniqueId) && whitelist.UniqueIds.Contains(uniqueId))
                return true;

            // Check peer ID
            if (!string.IsNullOrEmpty(peerId) && whitelist.PeerIds.Contains(peerId))
                return true;

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
                    monitor.Log(helper.Translation.Get("whitelist.log-kick-executed", new { command }), LogLevel.Debug);
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("whitelist.error-kick", new { error = ex.Message }), LogLevel.Error);
            }
        }

        /// <summary>
        /// Dispose of resources.
        /// </summary>
        public void Dispose()
        {
            helper.Events.Multiplayer.PeerConnected -= OnPeerConnected;
            helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTickedOnce;
            fileWatcher?.Dispose();
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
        public int GetTotalCount()
        {
            return DisplayNames.Count + UniqueIds.Count + PeerIds.Count;
        }
    }
}