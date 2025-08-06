// <copyright file="ModEntry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MultiplayerHelper
{
    using System;
    using MultiplayerHelper.Components;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    /// <summary>
    /// The central mod entry point that orchestrates all components.
    /// This is the main class that SMAPI loads and initializes.
    /// </summary>
    public class ModEntry : Mod
    {
        private ModConfig config;
        private InviteCodeManager inviteCodeManager;
        private AutoPauseManager autoPauseManager;
        private AutoConfigureManager autoConfigureManager;
        private WhitelistManager whitelistManager;

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// Initializes and sets up all mod components.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Load configuration
            this.config = helper.ReadConfig<ModConfig>();

            // Early exit if mod is disabled
            if (!this.config.ModEnabled)
            {
                this.Monitor.Log(this.Helper.Translation.Get("core.disabled"), LogLevel.Info);
                return;
            }

            // Initialize components with config
            this.inviteCodeManager = new InviteCodeManager(helper, this.Monitor, this.config);
            this.autoPauseManager = new AutoPauseManager(helper, this.Monitor, this.config);
            this.autoConfigureManager = new AutoConfigureManager(helper, this.Monitor, this.config);
            this.whitelistManager = new WhitelistManager(helper, this.Monitor, this.config);

            // Subscribe to mod-level events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;

            // Initialize components
            this.inviteCodeManager.Initialize();
            this.autoPauseManager.Initialize();
            this.autoConfigureManager.Initialize();

            this.Monitor.Log(this.Helper.Translation.Get("core.loaded"), LogLevel.Info);
        }

        /// <summary>
        /// Configures component settings based on config values.
        /// </summary>
        private void ConfigureComponents()
        {
            if (this.autoPauseManager != null)
            {
                this.autoPauseManager.UseDirectPause = this.config.PauseMethod == PauseMethod.Direct;
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!this.config.ModEnabled)
            {
                return;
            }

            this.Monitor.Log(this.Helper.Translation.Get("debug.game-launched"), this.config.LogLevel);
            this.ConfigureComponents();

            // Setup GMCM integration
            this.SetupGenericModConfigMenu();
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // Handle inter-mod communication here if needed in the future
        }

        /// <summary>
        /// Sets up Generic Mod Config Menu integration if available.
        /// </summary>
        private void SetupGenericModConfigMenu()
        {
            var configMenuApi = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenuApi == null)
            {
                return;
            }

            // Register mod
            configMenuApi.Register(
                mod: this.ModManifest,
                reset: () => this.config = new ModConfig(),
                save: () =>
                {
                    this.Helper.WriteConfig(this.config);
                    this.ConfigureComponents();
                });

            // General Settings Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.general.header"));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.general.mod-enabled"),
                tooltip: () => this.Helper.Translation.Get("config.general.mod-enabled.tooltip"),
                getValue: () => this.config.ModEnabled,
                setValue: value => this.config.ModEnabled = value);

            configMenuApi.AddTextOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.general.log-level"),
                tooltip: () => this.Helper.Translation.Get("config.general.log-level.tooltip"),
                getValue: () => this.config.LogLevel.ToString(),
                setValue: value => this.config.LogLevel = Enum.Parse<LogLevel>(value),
                allowedValues: Enum.GetNames(typeof(LogLevel)));

            // Invite Code Settings Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.invite.header"));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.invite.enabled"),
                tooltip: () => this.Helper.Translation.Get("config.invite.enabled.tooltip"),
                getValue: () => this.config.InviteCodeEnabled,
                setValue: value => this.config.InviteCodeEnabled = value);

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.invite.show-hud-notifications"),
                tooltip: () => this.Helper.Translation.Get("config.invite.show-hud-notifications.tooltip"),
                getValue: () => this.config.ShowHudNotifications,
                setValue: value => this.config.ShowHudNotifications = value);

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.invite.show-manual-copy-message"),
                tooltip: () => this.Helper.Translation.Get("config.invite.show-manual-copy-message.tooltip"),
                getValue: () => this.config.ShowManualCopyMessage,
                setValue: value => this.config.ShowManualCopyMessage = value);

            // Auto-Pause Settings Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.pause.header"));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.pause.enabled"),
                tooltip: () => this.Helper.Translation.Get("config.pause.enabled.tooltip"),
                getValue: () => this.config.AutoPauseEnabled,
                setValue: value => this.config.AutoPauseEnabled = value);

            configMenuApi.AddTextOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.pause.method"),
                tooltip: () => this.Helper.Translation.Get("config.pause.method.tooltip"),
                getValue: () => this.config.PauseMethod == PauseMethod.Direct
                    ? this.Helper.Translation.Get("config.pause.method-direct")
                    : this.Helper.Translation.Get("config.pause.method-chatbox"),
                setValue: value => this.config.PauseMethod = value.Contains("Direct") ? PauseMethod.Direct : PauseMethod.Chatbox,
                allowedValues: new[]
                {
                    this.Helper.Translation.Get("config.pause.method-direct").ToString(),
                    this.Helper.Translation.Get("config.pause.method-chatbox").ToString(),
                });

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.pause.enable-manual-toggle"),
                tooltip: () => this.Helper.Translation.Get("config.pause.enable-manual-toggle.tooltip"),
                getValue: () => this.config.EnableManualToggle,
                setValue: value => this.config.EnableManualToggle = value);

            configMenuApi.RegisterSimpleOption(
                mod: this.ModManifest,
                optionName: this.Helper.Translation.Get("config.pause.toggle-key"),
                optionDesc: this.Helper.Translation.Get("config.pause.toggle-key.tooltip"),
                optionGet: () => this.config.PauseToggleKey,
                optionSet: value => this.config.PauseToggleKey = value);

            // Auto-Configure Settings Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.configure.header"));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.configure.enabled"),
                tooltip: () => this.Helper.Translation.Get("config.configure.enabled.tooltip"),
                getValue: () => this.config.AutoConfigureEnabled,
                setValue: value => this.config.AutoConfigureEnabled = value);

            configMenuApi.AddTextOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.configure.sleep-announce-mode"),
                tooltip: () => this.Helper.Translation.Get("config.configure.sleep-announce-mode.tooltip"),
                getValue: () => this.config.SleepAnnounceModeParameter == SleepAnnounceMode.Off
                    ? this.Helper.Translation.Get("config.configure.sleep-announce-mode-off")
                    : this.config.SleepAnnounceModeParameter == SleepAnnounceMode.First
                        ? this.Helper.Translation.Get("config.configure.sleep-announce-mode-first")
                        : this.Helper.Translation.Get("config.configure.sleep-announce-mode-all"),
                setValue: value => this.config.SleepAnnounceModeParameter = value.Contains("off") ? SleepAnnounceMode.Off
                    : value.Contains("first") ? SleepAnnounceMode.First : SleepAnnounceMode.All,
                allowedValues: new[]
                {
                    this.Helper.Translation.Get("config.configure.sleep-announce-mode-off").ToString(),
                    this.Helper.Translation.Get("config.configure.sleep-announce-mode-first").ToString(),
                    this.Helper.Translation.Get("config.configure.sleep-announce-mode-all").ToString(),
                });

            configMenuApi.AddTextOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.configure.move-building-permission"),
                tooltip: () => this.Helper.Translation.Get("config.configure.move-building-permission.tooltip"),
                getValue: () => this.config.MoveBuildingPermissionParameter == MoveBuildingPermission.Off
                    ? this.Helper.Translation.Get("config.configure.move-building-permission-off")
                    : this.config.MoveBuildingPermissionParameter == MoveBuildingPermission.Owned
                        ? this.Helper.Translation.Get("config.configure.move-building-permission-owned")
                        : this.Helper.Translation.Get("config.configure.move-building-permission-on"),
                setValue: value => this.config.MoveBuildingPermissionParameter = value.Contains("off") ? MoveBuildingPermission.Off
                    : value.Contains("owned") ? MoveBuildingPermission.Owned : MoveBuildingPermission.On,
                allowedValues: new[]
                {
                    this.Helper.Translation.Get("config.configure.move-building-permission-off").ToString(),
                    this.Helper.Translation.Get("config.configure.move-building-permission-owned").ToString(),
                    this.Helper.Translation.Get("config.configure.move-building-permission-on").ToString(),
                });

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.configure.unban-all"),
                tooltip: () => this.Helper.Translation.Get("config.configure.unban-all.tooltip"),
                getValue: () => this.config.UnbanAllEnabled,
                setValue: value => this.config.UnbanAllEnabled = value);

            // Whitelist Settings Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.whitelist.header"));

            configMenuApi.AddParagraph(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.whitelist.setup-info"));

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.whitelist.enabled"),
                tooltip: () => this.Helper.Translation.Get("config.whitelist.enabled.tooltip"),
                getValue: () => this.config.WhitelistEnabled,
                setValue: value =>
                {
                    this.config.WhitelistEnabled = value;
                    this.whitelistManager?.SetEnabled(value);
                });

            configMenuApi.AddBoolOption(
                mod: this.ModManifest,
                name: () => this.Helper.Translation.Get("config.whitelist.log-player-identifiers"),
                tooltip: () => this.Helper.Translation.Get("config.whitelist.log-player-identifiers.tooltip"),
                getValue: () => this.config.ShowPlayerJoinInfo,
                setValue: value => this.config.ShowPlayerJoinInfo = value);

            // About Section
            configMenuApi.AddSectionTitle(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.about.header"));

            configMenuApi.AddParagraph(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.about.repository"));

            configMenuApi.AddParagraph(
                mod: this.ModManifest,
                text: () => this.Helper.Translation.Get("config.about.copyright"));
        }

        /// <summary>
        /// Cleanup when mod is being disposed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.inviteCodeManager?.Dispose();
                this.autoPauseManager?.Dispose();
                this.autoConfigureManager?.Dispose();
                this.whitelistManager?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
