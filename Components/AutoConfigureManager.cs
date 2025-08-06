// <copyright file="AutoConfigureManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MultiplayerHelper.Components
{
    using System;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

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
            if (!this.config.AutoConfigureEnabled)
            {
                return;
            }

            this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.helper.Events.Player.Warped += this.OnPlayerWarped;

            this.monitor.Log(this.helper.Translation.Get("debug.configure-manager-init"), this.config.LogLevel);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
            this.helper.Events.Player.Warped -= this.OnPlayerWarped;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.hasAutoConfigured = false;
            this.monitor.Log(this.helper.Translation.Get("debug.save-loaded-configure", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), this.config.LogLevel);

            // Auto-configure immediately when hosting multiplayer
            if (Context.IsMainPlayer && Context.IsMultiplayer)
            {
                this.helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTickedForConfigure;
            }
        }

        private void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (Context.IsMainPlayer && Context.IsMultiplayer && !this.hasAutoConfigured && e.IsLocalPlayer)
            {
                this.AutoConfigureMap();
            }
        }

        private void OnOneSecondUpdateTickedForConfigure(object sender, OneSecondUpdateTickedEventArgs e)
        {
            // Auto-configure when the game is ready and we haven't configured yet
            if (!this.hasAutoConfigured && Context.IsPlayerFree)
            {
                this.AutoConfigureMap();

                // Unsubscribe after configuring
                this.helper.Events.GameLoop.OneSecondUpdateTicked -= this.OnOneSecondUpdateTickedForConfigure;
            }
        }

        private void AutoConfigureMap()
        {
            try
            {
                this.monitor.Log(this.helper.Translation.Get("configure.log-attempting"), LogLevel.Info);

                // Execute sleepAnnounceMode command
                this.ExecuteCommand("sleepAnnounceMode", this.GetSleepAnnounceModeString(this.config.SleepAnnounceModeParameter));

                // Execute moveBuildingPermission command
                this.ExecuteCommand("moveBuildingPermission", this.GetMoveBuildingPermissionString(this.config.MoveBuildingPermissionParameter));

                // Execute unbanAll command if enabled
                if (this.config.UnbanAllEnabled)
                {
                    this.ExecuteCommand("unbanAll", string.Empty);
                }

                this.hasAutoConfigured = true;
                this.monitor.Log(this.helper.Translation.Get("configure.log-completed"), LogLevel.Info);
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("configure.error-failed", new { error = ex.Message }), LogLevel.Error);
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

                this.monitor.Log(this.helper.Translation.Get("configure.log-command-executed", new { command = fullCommand }), this.config.LogLevel);
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("configure.error-command-failed", new { command, parameter, error = ex.Message }), LogLevel.Error);
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
