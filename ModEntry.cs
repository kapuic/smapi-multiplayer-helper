using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MultiplayerHelper.Components;

namespace MultiplayerHelper
{
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

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// Initializes and sets up all mod components.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Load configuration
            config = helper.ReadConfig<ModConfig>();
            
            // Early exit if mod is disabled
            if (!config.ModEnabled)
            {
                Monitor.Log(Helper.Translation.Get("core.disabled"), LogLevel.Info);
                return;
            }
            
            // Initialize components with config
            inviteCodeManager = new InviteCodeManager(helper, Monitor, config);
            autoPauseManager = new AutoPauseManager(helper, Monitor, config);
            autoConfigureManager = new AutoConfigureManager(helper, Monitor, config);

            // Subscribe to mod-level events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            // Initialize components
            inviteCodeManager.Initialize();
            autoPauseManager.Initialize();
            autoConfigureManager.Initialize();

            Monitor.Log(Helper.Translation.Get("core.loaded"), LogLevel.Info);
        }

        /// <summary>
        /// Configures component settings based on config values.
        /// </summary>
        private void ConfigureComponents()
        {
            if (autoPauseManager != null)
            {
                autoPauseManager.UseDirectPause = config.PauseMethod == PauseMethod.Direct;
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!config.ModEnabled) return;
            
            Monitor.Log(Helper.Translation.Get("debug.game-launched"), config.LogLevel);
            ConfigureComponents();
            
            // Setup GMCM integration
            SetupGenericModConfigMenu();
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
            var configMenuApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenuApi == null)
                return;
                
            // Register mod
            configMenuApi.Register(
                mod: ModManifest,
                reset: () => config = new ModConfig(),
                save: () => {
                    Helper.WriteConfig(config);
                    ConfigureComponents();
                }
            );
            
            // General Settings Section
            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.general.header")
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.general.mod-enabled"),
                tooltip: () => Helper.Translation.Get("config.general.mod-enabled.tooltip"),
                getValue: () => config.ModEnabled,
                setValue: value => config.ModEnabled = value
            );
            
            configMenuApi.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.general.log-level"),
                tooltip: () => Helper.Translation.Get("config.general.log-level.tooltip"),
                getValue: () => config.LogLevel.ToString(),
                setValue: value => config.LogLevel = Enum.Parse<LogLevel>(value),
                allowedValues: Enum.GetNames(typeof(LogLevel))
            );
            
            // Invite Code Settings Section
            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.invite.header")
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.invite.enabled"),
                tooltip: () => Helper.Translation.Get("config.invite.enabled.tooltip"),
                getValue: () => config.InviteCodeEnabled,
                setValue: value => config.InviteCodeEnabled = value
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.invite.show-hud-notifications"),
                tooltip: () => Helper.Translation.Get("config.invite.show-hud-notifications.tooltip"),
                getValue: () => config.ShowHudNotifications,
                setValue: value => config.ShowHudNotifications = value
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.invite.show-manual-copy-message"),
                tooltip: () => Helper.Translation.Get("config.invite.show-manual-copy-message.tooltip"),
                getValue: () => config.ShowManualCopyMessage,
                setValue: value => config.ShowManualCopyMessage = value
            );
            
            // Auto-Pause Settings Section
            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.pause.header")
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.pause.enabled"),
                tooltip: () => Helper.Translation.Get("config.pause.enabled.tooltip"),
                getValue: () => config.AutoPauseEnabled,
                setValue: value => config.AutoPauseEnabled = value
            );
            
            configMenuApi.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.pause.method"),
                tooltip: () => Helper.Translation.Get("config.pause.method.tooltip"),
                getValue: () => config.PauseMethod == PauseMethod.Direct 
                    ? Helper.Translation.Get("config.pause.method-direct")
                    : Helper.Translation.Get("config.pause.method-chatbox"),
                setValue: value => config.PauseMethod = value.Contains("Direct") ? PauseMethod.Direct : PauseMethod.Chatbox,
                allowedValues: new[] { 
                    Helper.Translation.Get("config.pause.method-direct").ToString(),
                    Helper.Translation.Get("config.pause.method-chatbox").ToString() 
                }
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.pause.enable-manual-toggle"),
                tooltip: () => Helper.Translation.Get("config.pause.enable-manual-toggle.tooltip"),
                getValue: () => config.EnableManualToggle,
                setValue: value => config.EnableManualToggle = value
            );
            
            configMenuApi.RegisterSimpleOption(
                mod: ModManifest,
                optionName: Helper.Translation.Get("config.pause.toggle-key"),
                optionDesc: Helper.Translation.Get("config.pause.toggle-key.tooltip"),
                optionGet: () => config.PauseToggleKey,
                optionSet: value => config.PauseToggleKey = value
            );
            
            // Auto-Configure Settings Section
            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.configure.header")
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.configure.enabled"),
                tooltip: () => Helper.Translation.Get("config.configure.enabled.tooltip"),
                getValue: () => config.AutoConfigureEnabled,
                setValue: value => config.AutoConfigureEnabled = value
            );
            
            configMenuApi.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.configure.sleep-announce-mode"),
                tooltip: () => Helper.Translation.Get("config.configure.sleep-announce-mode.tooltip"),
                getValue: () => config.SleepAnnounceModeParameter == SleepAnnounceMode.Off 
                    ? Helper.Translation.Get("config.configure.sleep-announce-mode-off")
                    : config.SleepAnnounceModeParameter == SleepAnnounceMode.First
                        ? Helper.Translation.Get("config.configure.sleep-announce-mode-first")
                        : Helper.Translation.Get("config.configure.sleep-announce-mode-all"),
                setValue: value => config.SleepAnnounceModeParameter = value.Contains("off") ? SleepAnnounceMode.Off 
                    : value.Contains("first") ? SleepAnnounceMode.First : SleepAnnounceMode.All,
                allowedValues: new[] { 
                    Helper.Translation.Get("config.configure.sleep-announce-mode-off").ToString(),
                    Helper.Translation.Get("config.configure.sleep-announce-mode-first").ToString(),
                    Helper.Translation.Get("config.configure.sleep-announce-mode-all").ToString() 
                }
            );
            
            configMenuApi.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.configure.move-building-permission"),
                tooltip: () => Helper.Translation.Get("config.configure.move-building-permission.tooltip"),
                getValue: () => config.MoveBuildingPermissionParameter == MoveBuildingPermission.Off 
                    ? Helper.Translation.Get("config.configure.move-building-permission-off")
                    : config.MoveBuildingPermissionParameter == MoveBuildingPermission.Owned
                        ? Helper.Translation.Get("config.configure.move-building-permission-owned")
                        : Helper.Translation.Get("config.configure.move-building-permission-on"),
                setValue: value => config.MoveBuildingPermissionParameter = value.Contains("off") ? MoveBuildingPermission.Off 
                    : value.Contains("owned") ? MoveBuildingPermission.Owned : MoveBuildingPermission.On,
                allowedValues: new[] { 
                    Helper.Translation.Get("config.configure.move-building-permission-off").ToString(),
                    Helper.Translation.Get("config.configure.move-building-permission-owned").ToString(),
                    Helper.Translation.Get("config.configure.move-building-permission-on").ToString() 
                }
            );
            
            configMenuApi.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("config.configure.unban-all"),
                tooltip: () => Helper.Translation.Get("config.configure.unban-all.tooltip"),
                getValue: () => config.UnbanAllEnabled,
                setValue: value => config.UnbanAllEnabled = value
            );
            
            // About Section
            configMenuApi.AddSectionTitle(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.about.header")
            );
            
            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.about.repository")
            );
            
            configMenuApi.AddParagraph(
                mod: ModManifest,
                text: () => Helper.Translation.Get("config.about.copyright")
            );
        }
        
        /// <summary>
        /// Cleanup when mod is being disposed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inviteCodeManager?.Dispose();
                autoPauseManager?.Dispose();
                autoConfigureManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
