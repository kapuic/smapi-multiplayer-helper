using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerHelper.Components
{
    /// <summary>
    /// Manages automatic map configuration functionality when the host joins multiplayer.
    /// </summary>
    public class AutoConfigureManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly ModConfig config;
        private bool hasAutoConfigured = false;

        public AutoConfigureManager(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.config = config;
        }

        /// <summary>
        /// Initializes the auto-configure manager by subscribing to relevant events.
        /// </summary>
        public void Initialize()
        {
            if (!config.AutoConfigureEnabled)
                return;
                
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.Player.Warped += OnPlayerWarped;
            
            monitor.Log(helper.Translation.Get("debug.configure-manager-init"), config.LogLevel);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
            helper.Events.Player.Warped -= OnPlayerWarped;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            hasAutoConfigured = false;
            monitor.Log(helper.Translation.Get("debug.save-loaded-configure", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), config.LogLevel);

            // Auto-configure immediately when hosting multiplayer
            if (Context.IsMainPlayer && Context.IsMultiplayer)
            {
                helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTickedForConfigure;
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer && Context.IsMultiplayer && !hasAutoConfigured && e.IsLocalPlayer)
            {
                AutoConfigureMap();
            }
        }

        private void OnOneSecondUpdateTickedForConfigure(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // Auto-configure when the game is ready and we haven't configured yet
            if (!hasAutoConfigured && Context.IsPlayerFree)
            {
                AutoConfigureMap();
                // Unsubscribe after configuring
                helper.Events.GameLoop.OneSecondUpdateTicked -= OnOneSecondUpdateTickedForConfigure;
            }
        }

        private void AutoConfigureMap()
        {
            try
            {
                monitor.Log(helper.Translation.Get("configure.log-attempting"), LogLevel.Info);

                // Execute sleepAnnounceMode command
                ExecuteCommand("sleepAnnounceMode", GetSleepAnnounceModeString(config.SleepAnnounceModeParameter));
                
                // Execute moveBuildingPermission command
                ExecuteCommand("moveBuildingPermission", GetMoveBuildingPermissionString(config.MoveBuildingPermissionParameter));

                // Execute unbanAll command if enabled
                if (config.UnbanAllEnabled)
                {
                    ExecuteCommand("unbanAll", "");
                }

                hasAutoConfigured = true;
                monitor.Log(helper.Translation.Get("configure.log-completed"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("configure.error-failed", new { error = ex.Message }), LogLevel.Error);
            }
        }

        private void ExecuteCommand(string command, string parameter)
        {
            try
            {
                // Execute console commands using the game's built-in console
                string fullCommand = string.IsNullOrEmpty(parameter) 
                    ? command 
                    : $"{command} {parameter}";
                
                // Use the game's console system to execute commands
                Game1.game1.parseDebugInput(fullCommand);
                
                monitor.Log(helper.Translation.Get("configure.log-command-executed", new { command = fullCommand }), config.LogLevel);
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("configure.error-command-failed", new { command, parameter, error = ex.Message }), LogLevel.Error);
            }
        }

        private string GetSleepAnnounceModeString(SleepAnnounceMode mode)
        {
            return mode switch
            {
                SleepAnnounceMode.Off => "off",
                SleepAnnounceMode.First => "first",
                SleepAnnounceMode.All => "all",
                _ => "all"
            };
        }

        private string GetMoveBuildingPermissionString(MoveBuildingPermission permission)
        {
            return permission switch
            {
                MoveBuildingPermission.Off => "off",
                MoveBuildingPermission.Owned => "owned",
                MoveBuildingPermission.On => "on",
                _ => "owned"
            };
        }
    }
}