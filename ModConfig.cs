using StardewModdingAPI;
using StardewModdingAPI.Utilities;

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

        /// <summary>
        /// Gets or sets the keybind for manual pause/resume toggle.
        /// </summary>
        public KeybindList PauseToggleKey { get; set; } = KeybindList.Parse("P");

        /// <summary>
        /// Gets or sets whether auto-configure functionality is enabled.
        /// </summary>
        public bool AutoConfigureEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets the parameter for sleepAnnounceMode command.
        /// </summary>
        public SleepAnnounceMode SleepAnnounceModeParameter { get; set; } = SleepAnnounceMode.All;

        /// <summary>
        /// Gets or sets the parameter for moveBuildingPermission command.
        /// </summary>
        public MoveBuildingPermission MoveBuildingPermissionParameter { get; set; } = MoveBuildingPermission.Owned;

        /// <summary>
        /// Gets or sets whether to automatically unban all players when configuring the map.
        /// </summary>
        public bool UnbanAllEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether whitelist functionality is enabled.
        /// </summary>
        public bool WhitelistEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show player join information in console with identifiers.
        /// </summary>
        public bool ShowPlayerJoinInfo { get; set; } = true;
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

    /// <summary>
    /// Defines the available sleep announce modes.
    /// </summary>
    public enum SleepAnnounceMode
    {
        /// <summary>
        /// Never show sleep messages.
        /// </summary>
        Off,

        /// <summary>
        /// Only show the first time a player sleeps each day.
        /// </summary>
        First,

        /// <summary>
        /// Show every time a player sleeps.
        /// </summary>
        All
    }

    /// <summary>
    /// Defines the available move building permission levels.
    /// </summary>
    public enum MoveBuildingPermission
    {
        /// <summary>
        /// Farmhands can't move any buildings.
        /// </summary>
        Off,

        /// <summary>
        /// Farmhands can move their own buildings.
        /// </summary>
        Owned,

        /// <summary>
        /// Farmhands can move all buildings.
        /// </summary>
        On
    }

    /// <summary>
    /// Defines the available player identifier types for whitelisting.
    /// </summary>
    public enum PlayerIdentifierType
    {
        /// <summary>
        /// Use the player's display name (Farmer.Name).
        /// </summary>
        DisplayName,

        /// <summary>
        /// Use the player's unique multiplayer ID (Farmer.UniqueMultiplayerID).
        /// </summary>
        UniqueId,

        /// <summary>
        /// Use the peer connection ID (IMultiplayerPeer.PlayerID).
        /// </summary>
        PeerId
    }
}