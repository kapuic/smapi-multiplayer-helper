using System;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MultiplayerHelper.Components
{
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
            if (!config.InviteCodeEnabled)
                return;
                
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            monitor.Log(helper.Translation.Get("debug.invite-manager-init"), config.LogLevel);
        }

        /// <summary>
        /// Cleans up event subscriptions.
        /// </summary>
        public void Dispose()
        {
            helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded -= OnSaveLoaded;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            hasHostedSession = false;
            monitor.Log(helper.Translation.Get("debug.save-loaded-invite", new { isMainPlayer = Context.IsMainPlayer, isMultiplayer = Context.IsMultiplayer }), config.LogLevel);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            // Only check every 60 ticks (once per second) to avoid spam
            if (e.IsMultipleOf(60))
            {
                if (Context.IsWorldReady && Context.IsMainPlayer)
                {
                    if (Game1.server != null && !hasHostedSession)
                    {
                        monitor.Log(helper.Translation.Get("invite.log-detecting"), LogLevel.Debug);
                        CheckForInviteCode();
                    }
                }
            }
        }

        private void CheckForInviteCode()
        {
            try
            {
                monitor.Log(helper.Translation.Get("invite.log-detecting"), LogLevel.Debug);

                string inviteCode = null;

                if (Game1.server != null)
                {
                    var serverType = Game1.server.GetType();
                    monitor.Log($"Server type: {serverType.FullName}", LogLevel.Trace);

                    // Try method approach
                    var getInviteCodeMethod = serverType.GetMethod("getInviteCode");
                    if (getInviteCodeMethod != null)
                    {
                        inviteCode = getInviteCodeMethod.Invoke(Game1.server, null) as string;
                        monitor.Log(helper.Translation.Get("invite.log-found", new { code = inviteCode }), LogLevel.Debug);
                    }
                    else
                    {
                        // List all methods for debugging
                        var methods = serverType.GetMethods();
                        foreach (var method in methods)
                        {
                            if (method.Name.ToLower().Contains("invite") || method.Name.ToLower().Contains("code"))
                            {
                                monitor.Log($"Found method: {method.Name}", LogLevel.Trace);
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(inviteCode))
                {
                    CopyToClipboard(inviteCode);
                    hasHostedSession = true;

                    if (config.ShowHudNotifications)
                    {
                        Game1.addHUDMessage(new HUDMessage(helper.Translation.Get("invite.hud-copied", new { code = inviteCode }), HUDMessage.achievement_type));
                    }
                    monitor.Log(helper.Translation.Get("invite.log-copied", new { code = inviteCode }), config.LogLevel);
                }
                else
                {
                    monitor.Log(helper.Translation.Get("invite.log-unavailable"), config.LogLevel);
                }
            }
            catch (Exception ex)
            {
                monitor.Log(helper.Translation.Get("invite.error-detection", new { error = ex.Message }), LogLevel.Error);
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
                monitor.Log(helper.Translation.Get("invite.error-clipboard", new { error = ex.Message }), LogLevel.Error);

                // Fallback: Show manual copy message if enabled
                if (config.ShowManualCopyMessage)
                {
                    monitor.Log(helper.Translation.Get("invite.hud-manual", new { code = text }), LogLevel.Alert);
                }
            }
        }
    }
}