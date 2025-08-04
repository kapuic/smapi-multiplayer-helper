# Multiplayer Helper for Stardew Valley

A comprehensive SMAPI mod that enhances your multiplayer hosting experience with automatic invite code copying, game pausing, server configuration, and customizable controls.

## ✨ Features

### 🔗 **Invite Code Management**
- **Auto-copy invite codes** to clipboard when hosting multiplayer
- **Configurable notifications** with HUD messages
- **Manual copy fallback** when clipboard access fails
- Works seamlessly with all multiplayer session types

### ⏸️ **Smart Pause System**
- **Auto-pause when hosting** - Game pauses automatically when you start a multiplayer session
- **Configurable keybinds** - Customize your pause/resume toggle key (default: P)
- **Multiple pause methods** - Direct pause (local) or chatbox commands (multiplayer)
- **Manual toggle control** - Enable/disable manual pause controls

### ⚙️ **Server Auto-Configuration**
- **Sleep announce mode** - Control when sleep messages appear (off/first/all)
- **Building permissions** - Set farmhand building move permissions (off/owned/all)
- **Auto-unban players** - Optionally unban all players when configuring
- **Disabled by default** - Enable only when needed

### 🌍 **Full Localization**
- **12 languages supported**: English, German, Spanish, French, Portuguese, Russian, Japanese, Korean, Chinese, Italian, Turkish, Hungarian
- **GMCM integration** - Full Generic Mod Config Menu support with organized sections
- **Optional dependency** - Works with or without GMCM installed

### 🎛️ **Advanced Configuration**
- **Modular architecture** - Each feature can be enabled/disabled independently
- **Granular logging** - Configurable log levels (Trace/Debug/Info/Warn/Error)
- **Hot-reload config** - Changes apply immediately without restart
- **Sane defaults** - Works great out of the box

## 📦 Installation

1. Install [SMAPI](https://smapi.io/) (3.0+)
2. Download the mod from [Nexus Mods] (link pending)
3. Unzip the mod folder into `Stardew Valley/Mods`
4. Run the game using SMAPI
5. *(Optional)* Install [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) for in-game configuration

## 🚀 How to Use

### Basic Usage
1. Start Stardew Valley with SMAPI
2. Load your save and host a multiplayer session
3. **Invite code automatically copied** to clipboard
4. Share with friends by pasting (Ctrl+V / Cmd+V)
5. **Game auto-pauses** when you join the world (if enabled)

### Configuration
- **In-game**: Install GMCM and access via *Mod Options → Multiplayer Helper*
- **File-based**: Edit `config.json` in the mod folder
- **Key features**: Toggle any feature, customize keybinds, set log levels

### Server Management (Optional)
- Enable auto-configure in mod settings
- Automatically runs console commands when hosting:
  - `sleepAnnounceMode [off/first/all]`
  - `moveBuildingPermission [off/owned/on]`
  - `unbanAll` (if enabled)

## 🔧 Configuration Options

| Setting | Default | Description |
|---------|---------|-------------|
| **ModEnabled** | `true` | Enable/disable all mod functionality |
| **LogLevel** | `Info` | Logging verbosity level |
| **InviteCodeEnabled** | `true` | Auto-copy invite codes |
| **ShowHudNotifications** | `true` | Display in-game notifications |
| **AutoPauseEnabled** | `true` | Auto-pause when hosting |
| **PauseMethod** | `Chatbox` | Pause method (Direct/Chatbox) |
| **PauseToggleKey** | `P` | Configurable pause toggle keybind |
| **AutoConfigureEnabled** | `false` | Enable server auto-configuration |

## 🛠️ Building from Source

```bash
# Prerequisites: .NET 5.0+, SMAPI development setup
git clone https://github.com/yourusername/smapi-multiplayer-helper
cd smapi-multiplayer-helper
dotnet build
# Built mod will be in bin/Debug/net5.0
```

## 🎮 Compatibility

- **Stardew Valley**: 1.5+ (1.6+ recommended)
- **SMAPI**: 3.0+ (latest recommended)
- **Platforms**: Windows, Mac, Linux
- **Optional**: Generic Mod Config Menu for GUI configuration
- **Multiplayer**: Full host and farmhand compatibility

## 🔮 Upcoming Features

- **Player whitelist system** - Control who can join your server
- **Advanced server management tools**
- **More automation options**

## 🤝 Contributing

Contributions welcome! This mod uses:
- **Modular architecture** with separate managers for each feature
- **Comprehensive localization** system
- **Optional dependency patterns** for maximum compatibility

## 📄 License

This mod is open source under MIT License. Feel free to contribute, fork, or modify!

---

*Made with ❤️ for the Stardew Valley modding community*