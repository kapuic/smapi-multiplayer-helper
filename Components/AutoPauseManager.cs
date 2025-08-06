// <copyright file="AutoPauseManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MultiplayerHelper.Components
{
    using System;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    /// <summary>
    /// Manages automatic game pausing functionality when the host joins multiplayer.
    /// </summary>
    public class AutoPauseManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly ModConfig config;
        private bool hasAutoPaused = false;
        private bool useDirectPause = false; // Set to true to use Game1.paused instead of chatbox

        public AutoPauseManager(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.config = config;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets whether to use direct pause method instead of chatbox method.
        /// </summary>
        public bool UseDirectPause
        {
            get => this.useDirectPause;
            set => this.useDirectPause = value;
        }

        /// <summary>
        /// Initializes the auto-pause manager by subscribing to relevant events.
        /// </summary>
        public void Initialize()
        {
            if (!this.config.AutoPauseEnabled)
            {
                return;
            }

            this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.helper.Events.Player.Warped += this.OnPlayerWarped;
            this.helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;

            if (this.config.EnableManualToggle)
            {
                this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            }

            this.monitor.Log(this.helper.Translation.Get("debug.pause-manager-init", new { useDirectPause = this.useDirectPause }), this.config.LogLevel);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
            this.helper.Events.Player.Warped -= this.OnPlayerWarped;
            this.helper.Events.Multiplayer.PeerConnected -= this.OnPeerConnected;
            if (this.config.EnableManualToggle)
            {
                this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.hasAutoPaused = false;
            this.monitor.Log(this.helper.Translation.Get("debug.save-loaded-pause", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), this.config.LogLevel);

            // Auto-pause immediately when hosting multiplayer
            if (Context.IsMainPlayer && Context.IsMultiplayer)
            {
                this.helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTickedForPause;
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer && Context.IsMultiplayer && !this.hasAutoPaused && e.IsLocalPlayer)
            {
                this.AutoPauseGame();
            }
        }

        private void OnOneSecondUpdateTickedForPause(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // Auto-pause when the game is ready and we haven't paused yet
            if (!this.hasAutoPaused && Context.IsPlayerFree)
            {
                this.AutoPauseGame();

                // Unsubscribe after pausing
                this.helper.Events.GameLoop.OneSecondUpdateTicked -= this.OnOneSecondUpdateTickedForPause;
            }
        }

        private void AutoPauseGame()
        {
            try
            {
                this.monitor.Log($"Attempting to auto-pause. Game paused: {Game1.paused}, Should time pass: {Game1.shouldTimePass()}", LogLevel.Debug);

                if (!Game1.paused && Game1.shouldTimePass())
                {
                    if (this.useDirectPause)
                    {
                        // Stop player movement before pausing
                        Game1.player.Halt();
                        Game1.player.forceCanMove();

                        // Direct pause method (may conflict with other mods)
                        Game1.paused = true;
                        this.hasAutoPaused = true;

                        Game1.addHUDMessage(new HUDMessage(this.helper.Translation.Get("pause.hud-paused-direct"), HUDMessage.achievement_type));
                        this.monitor.Log(this.helper.Translation.Get("pause.log-auto-direct"), LogLevel.Info);
                    }
                    else
                    {
                        // Use multiplayer pause voting system via chatbox (default)
                        if (Game1.chatBox != null)
                        {
                            Game1.chatBox.activate();
                            Game1.chatBox.setText("/pause");
                            Game1.chatBox.textBoxEnter(Game1.chatBox.chatBox);
                            this.hasAutoPaused = true;

                            this.monitor.Log(this.helper.Translation.Get("pause.log-auto-chatbox"), LogLevel.Info);
                        }
                        else
                        {
                            this.monitor.Log(this.helper.Translation.Get("pause.error-chatbox-unavailable"), LogLevel.Debug);
                        }
                    }
                }
                else if (Game1.paused)
                {
                    this.monitor.Log(this.helper.Translation.Get("pause.log-already-paused"), LogLevel.Debug);
                    this.hasAutoPaused = true;
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("pause.error-failed", new { error = ex.Message }), LogLevel.Error);
            }
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                this.monitor.Log(this.helper.Translation.Get("pause.log-player-connected", new { playerName = e.Peer.PlayerID }), LogLevel.Debug);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Check if player pressed the configured keybind to toggle pause (only in multiplayer as host)
            if (Context.IsWorldReady && Context.IsMainPlayer && Context.IsMultiplayer && this.config.PauseToggleKey.JustPressed())
            {
                if (Game1.paused)
                {
                    // Resume the game
                    Game1.paused = false;
                    Game1.addHUDMessage(new HUDMessage(this.helper.Translation.Get("pause.hud-resumed"), HUDMessage.achievement_type));
                    this.monitor.Log(this.helper.Translation.Get("pause.log-manual-resumed"), LogLevel.Info);
                }
                else if (this.useDirectPause)
                {
                    // Stop player movement before pausing
                    Game1.player.Halt();
                    Game1.player.forceCanMove();

                    // Pause the game using direct method
                    Game1.paused = true;
                    Game1.addHUDMessage(new HUDMessage(this.helper.Translation.Get("pause.hud-paused-manual"), HUDMessage.achievement_type));
                    this.monitor.Log(this.helper.Translation.Get("pause.log-manual-paused"), LogLevel.Info);
                }
            }
        }
    }
}
