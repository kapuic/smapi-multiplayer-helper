using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerHelper.Components
{
    /// <summary>
    /// Manages automatic game pausing functionality when the host joins multiplayer.
    /// </summary>
    public class AutoPauseManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private bool hasAutoPaused = false;
        private bool useDirectPause = false; // Set to true to use Game1.paused instead of chatbox

        public AutoPauseManager(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
        }

        /// <summary>
        /// Gets or sets whether to use direct pause method instead of chatbox method.
        /// </summary>
        public bool UseDirectPause
        {
            get => useDirectPause;
            set => useDirectPause = value;
        }

        /// <summary>
        /// Initializes the auto-pause manager by subscribing to relevant events.
        /// </summary>
        public void Initialize()
        {
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Player.Warped += OnPlayerWarped;
            helper.Events.Multiplayer.PeerConnected += OnPeerConnected;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            monitor.Log(helper.Translation.Get("debug.pause-manager-init", new { useDirectPause = useDirectPause }), LogLevel.Debug);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
            helper.Events.Player.Warped -= OnPlayerWarped;
            helper.Events.Multiplayer.PeerConnected -= OnPeerConnected;
            helper.Events.Input.ButtonPressed -= OnButtonPressed;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            hasAutoPaused = false;
            monitor.Log(helper.Translation.Get("debug.save-loaded-pause", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), LogLevel.Debug);

            // Auto-pause immediately when hosting multiplayer
            if (Context.IsMainPlayer && Context.IsMultiplayer)
            {
                helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTickedForPause;
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer && Context.IsMultiplayer && !hasAutoPaused && e.IsLocalPlayer)
            {
                AutoPauseGame();
            }
        }

        private void OnOneSecondUpdateTickedForPause(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // Auto-pause when the game is ready and we haven't paused yet
            if (!hasAutoPaused && Context.IsPlayerFree)
            {
                AutoPauseGame();
                // Unsubscribe after pausing
                helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTickedForPause;
            }
        }

        private void AutoPauseGame()
        {
            try
            {
                monitor.Log($"Attempting to auto-pause. Game paused: {Game1.paused}, Should time pass: {Game1.shouldTimePass()}", LogLevel.Debug);

                if (!Game1.paused && Game1.shouldTimePass())
                {
                    if (useDirectPause)
                    {
                        // Stop player movement before pausing
                        Game1.player.Halt();
                        Game1.player.forceCanMove();

                        // Direct pause method (may conflict with other mods)
                        Game1.paused = true;
                        hasAutoPaused = true;

                        Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("pause.hud-paused-direct"), HUDMessage.achievement_type));
                        monitor.Log(helper.Translation.Get("pause.log-auto-direct"), LogLevel.Info);
                    }
                    else
                    {
                        // Use multiplayer pause voting system via chatbox (default)
                        if (Game1.chatBox != null)
                        {
                            Game1.chatBox.activate();
                            Game1.chatBox.setText("/pause");
                            Game1.chatBox.textBoxEnter(Game1.chatBox.chatBox);
                            hasAutoPaused = true;

                            monitor.Log(helper.Translation.Get("pause.log-auto-chatbox"), LogLevel.Info);
                        }
                        else
                        {
                            monitor.Log(helper.Translation.Get("pause.error-chatbox-unavailable"), LogLevel.Debug);
                        }
                    }
                }
                else if (Game1.paused)
                {
                    monitor.Log(helper.Translation.Get("pause.log-already-paused"), LogLevel.Debug);
                    hasAutoPaused = true;
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("pause.error-failed", new { error = ex.Message }), LogLevel.Error);
            }
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                monitor.Log(helper.Translation.Get("pause.log-player-connected", new { playerName = e.Peer.PlayerID }), LogLevel.Debug);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Check if player pressed P key to toggle pause (only in multiplayer as host)
            if (Context.IsWorldReady && Context.IsMainPlayer && Context.IsMultiplayer && e.Button == SButton.P)
            {
                if (Game1.paused)
                {
                    // Resume the game
                    Game1.paused = false;
                    Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("pause.hud-resumed"), HUDMessage.achievement_type));
                    monitor.Log(helper.Translation.Get("pause.log-manual-resumed"), LogLevel.Info);
                }
                else if (useDirectPause)
                {
                    // Stop player movement before pausing
                    Game1.player.Halt();
                    Game1.player.forceCanMove();

                    // Pause the game using direct method
                    Game1.paused = true;
                    Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("pause.hud-paused-manual"), HUDMessage.achievement_type));
                    monitor.Log(helper.Translation.Get("pause.log-manual-paused"), LogLevel.Info);
                }
            }
        }
    }
}