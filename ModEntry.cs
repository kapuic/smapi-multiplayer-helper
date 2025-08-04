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
        private InviteCodeManager inviteCodeManager;
        private AutoPauseManager autoPauseManager;

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// Initializes and sets up all mod components.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Initialize components
            inviteCodeManager = new InviteCodeManager(helper, Monitor);
            autoPauseManager = new AutoPauseManager(helper, Monitor);

            // Subscribe to mod-level events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            // Initialize components
            inviteCodeManager.Initialize();
            autoPauseManager.Initialize();

            Monitor.Log(Helper.Translation.Get("core.loaded"), LogLevel.Info);
        }

        /// <summary>
        /// Configures component settings. This could be expanded to read from config files.
        /// </summary>
        private void ConfigureComponents()
        {
            // Example: Configure auto-pause method
            // autoPauseManager.UseDirectPause = true; // Uncomment to use direct pause instead of chatbox
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Monitor.Log(Helper.Translation.Get("debug.game-launched"), LogLevel.Debug);
            ConfigureComponents();
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            // Handle inter-mod communication here if needed in the future
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
            }
            base.Dispose(disposing);
        }
    }
}
