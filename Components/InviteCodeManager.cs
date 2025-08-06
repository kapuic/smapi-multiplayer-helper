// <copyright file="InviteCodeManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MultiplayerHelper.Components
{
    using System;
    using System.Reflection;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    /// <summary>
    /// Manages automatic invite code detection and clipboard copying functionality.
    /// </summary>
    public class InviteCodeManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private readonly ModConfig config;
        private bool hasHostedSession = false;

        public InviteCodeManager(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.config = config;
        }

        /// <summary>
        /// Initializes the invite code manager by subscribing to relevant events.
        /// </summary>
        public void Initialize()
        {
            if (!this.config.InviteCodeEnabled)
            {
                return;
            }

            this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.monitor.Log(this.helper.Translation.Get("debug.invite-manager-init"), this.config.LogLevel);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.hasHostedSession = false;
            this.monitor.Log(this.helper.Translation.Get("debug.save-loaded-invite", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), this.config.LogLevel);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Only check every 60 ticks (once per second) to avoid spam
            if (e.IsMultipleOf(60))
            {
                if (Context.IsWorldReady && Context.IsMainPlayer)
                {
                    if (Game1.server != null && !this.hasHostedSession)
                    {
                        this.monitor.Log(this.helper.Translation.Get("invite.log-detecting"), LogLevel.Debug);
                        this.CheckForInviteCode();
                    }
                }
            }
        }

        private void CheckForInviteCode()
        {
            try
            {
                this.monitor.Log(this.helper.Translation.Get("invite.log-detecting"), LogLevel.Debug);

                string inviteCode = null;

                if (Game1.server != null)
                {
                    var serverType = Game1.server.GetType();
                    this.monitor.Log($"Server type: {serverType.FullName}", LogLevel.Trace);

                    // Try method approach
                    var getInviteCodeMethod = serverType.GetMethod("getInviteCode");
                    if (getInviteCodeMethod != null)
                    {
                        inviteCode = getInviteCodeMethod.Invoke(Game1.server, null) as string;
                        this.monitor.Log(this.helper.Translation.Get("invite.log-found", new { code = inviteCode }), LogLevel.Debug);
                    }
                    else
                    {
                        // List all methods for debugging
                        var methods = serverType.GetMethods();
                        foreach (var method in methods)
                        {
                            if (method.Name.ToLower().Contains("invite") || method.Name.ToLower().Contains("code"))
                            {
                                this.monitor.Log($"Found method: {method.Name}", LogLevel.Trace);
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(inviteCode))
                {
                    this.CopyToClipboard(inviteCode);
                    this.hasHostedSession = true;

                    if (this.config.ShowHudNotifications)
                    {
                        Game1.addHUDMessage(new HUDMessage(this.helper.Translation.Get("invite.hud-copied", new { code = inviteCode }), HUDMessage.achievement_type));
                    }

                    this.monitor.Log(this.helper.Translation.Get("invite.log-copied", new { code = inviteCode }), this.config.LogLevel);
                }
                else
                {
                    this.monitor.Log(this.helper.Translation.Get("invite.log-unavailable"), this.config.LogLevel);
                }
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("invite.error-detection", new { error = ex.Message }), LogLevel.Error);
            }
        }

        private void CopyToClipboard(string text)
        {
            try
            {
                // Use SMAPI's built-in clipboard functionality
                DesktopClipboard.SetText(text);
            }
            catch (Exception ex)
            {
                this.monitor.Log(this.helper.Translation.Get("invite.error-clipboard", new { error = ex.Message }), LogLevel.Error);

                // Fallback: Show manual copy message if enabled
                if (this.config.ShowManualCopyMessage)
                {
                    this.monitor.Log(this.helper.Translation.Get("invite.hud-manual", new { code = text }), LogLevel.Alert);
                }
            }
        }
    }
}
