using StardewModdingAPI;

namespace MultiplayerHelper
{
    /// <summary>
    /// Configuration options for the MultiplayerHelper mod.
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Gets or sets whether the mod is enabled.
        /// </summary>
        public bool ModEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the log level for debug output.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// Gets or sets whether invite code functionality is enabled.
        /// </summary>
        public bool InviteCodeEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show HUD notifications when invite codes are copied.
        /// </summary>
        public bool ShowHudNotifications { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show manual copy messages when clipboard fails.
        /// </summary>
        public bool ShowManualCopyMessage { get; set; } = true;

        /// <summary>
        /// Gets or sets whether auto-pause functionality is enabled.
        /// </summary>
        public bool AutoPauseEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the pause method to use.
        /// </summary>
        public PauseMethod PauseMethod { get; set; } = PauseMethod.Chatbox;

        /// <summary>
        /// Gets or sets whether manual P key pause/resume toggle is enabled.
        /// </summary>
        public bool EnableManualToggle { get; set; } = true;
    }

    /// <summary>
    /// Defines the available pause methods.
    /// </summary>
    public enum PauseMethod
    {
        /// <summary>
        /// Direct pause using Game1.paused (local only).
        /// </summary>
        Direct,

        /// <summary>
        /// Chatbox pause command (multiplayer synchronized).
        /// </summary>
        Chatbox
    }
}