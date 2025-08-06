# Multiplayer Helper for Stardew Valley

A comprehensive SMAPI mod that enhances your multiplayer hosting experience with automatic invite code copying, game pausing, server configuration, and customizable controls.

## ✨ Features

| **🔗 Invite Code Management**                                                                                                                                                                                                                                                                            | **⏸️ Smart Pause System**                                                                                                                                                                                                                                                                                                                                                                           |
| -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| - **Auto-copy invite codes** to clipboard when hosting multiplayer<br>- **Configurable notifications** with HUD messages<br>- **Manual copy fallback** when clipboard access fails<br>- Works seamlessly with all multiplayer session types                                                              | - **Auto-pause when hosting** - Game pauses automatically when you start a multiplayer session<br>- **Configurable keybinds** - Customize your pause/resume toggle key (default: P)<br>- **Multiple pause methods** - Direct pause (local) or chatbox commands (multiplayer)<br>- **Manual toggle control** - Enable/disable manual pause controls                                                  |
| **⚙️ Server Auto-Configuration**                                                                                                                                                                                                                                                                         | **🛡️ Player Whitelist System**                                                                                                                                                                                                                                                                                                                                                                      |
| - **Sleep announce mode** - Control when sleep messages appear (off/first/all)<br>- **Building permissions** - Set farmhand building move permissions (off/owned/all)<br>- **Auto-unban players** - Optionally unban all players when configuring<br>- **Disabled by default** - Enable only when needed | - **JSON-based whitelist** - Control who can join your server with whitelist.json<br>- **Three identification methods** - DisplayName, UniqueID, and PeerID support<br>- **Auto-kick non-whitelisted players** - Automatic enforcement<br>- **Setup assistance** - Log player identifiers to help configure whitelist<br>- **Hot-reload support** - Changes apply immediately when file is modified |
| **🌍 Full Localization**                                                                                                                                                                                                                                                                                 | **🎛️ Advanced Configuration**                                                                                                                                                                                                                                                                                                                                                                       |
| - **12 languages supported**: English, German, Spanish, French, Portuguese, Russian, Japanese, Korean, Chinese, Italian, Turkish, Hungarian<br>- **GMCM integration** - Full Generic Mod Config Menu support with organized sections<br>- **Optional dependency** - Works with or without GMCM installed | - **Modular architecture** - Each feature can be enabled/disabled independently<br>- **Granular logging** - Configurable log levels (Trace/Debug/Info/Warn/Error)<br>- **Hot-reload config** - Changes apply immediately without restart<br>- **Sane defaults** - Works great out of the box                                                                                                        |

## 📦 Installation

1. Install [SMAPI](https://smapi.io/) (4.0+)
2. Download the mod from the [GitHub Releases](https://github.com/kapuic/smapi-multiplayer-helper/releases)
3. Unzip the mod folder into `Stardew Valley/Mods`
4. Run the game using SMAPI
5. _(Optional)_ Install [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) ([GitHub](https://github.com/spacechase0/StardewValleyMods/tree/develop/GenericModConfigMenu)) for in-game configuration

## 🎮 Compatibility

- **Stardew Valley**: 1.6.14+ (latest recommended)
- **SMAPI**: 4.0+ (4.3.2+ recommended)
- **.NET**: 6.0+ (automatically handled by SMAPI)
- **Platforms**: Windows, Mac, Linux
- **Optional**: Generic Mod Config Menu for GUI configuration
- **Multiplayer**: Full host and farmhand compatibility

## 🚀 How to Use

### Basic Usage

1. Start Stardew Valley with SMAPI
2. Load your save and host a multiplayer session
3. **Invite code automatically copied** to clipboard
4. Share with friends by pasting (Ctrl+V / Cmd+V)
5. **Game auto-pauses** when you join the world (if enabled)

### Configuration

- **In-game**: Install GMCM and access via _Mod Options → Multiplayer Helper_
- **File-based**: Edit `config.json` in the mod folder
- **Key features**: Toggle any feature, customize keybinds, set log levels

### Server Management (Optional)

- **Auto-configure**: Enable in mod settings to automatically run console commands:
  - `sleepAnnounceMode [off/first/all]`
  - `moveBuildingPermission [off/owned/on]`
  - `unbanAll` (if enabled)
- **Whitelist**: Enable player whitelist system:
  - Create/edit `whitelist.json` in mod folder
  - Add player DisplayNames, UniqueIDs, or PeerIDs
  - Enable "Log Player Identifiers" to see player info in console

## 🔧 Configuration Options

| Setting                  | Default   | Description                            |
| ------------------------ | --------- | -------------------------------------- |
| **ModEnabled**           | `true`    | Enable/disable all mod functionality   |
| **LogLevel**             | `Info`    | Logging verbosity level                |
| **InviteCodeEnabled**    | `true`    | Auto-copy invite codes                 |
| **ShowHudNotifications** | `true`    | Display in-game notifications          |
| **AutoPauseEnabled**     | `true`    | Auto-pause when hosting                |
| **PauseMethod**          | `Chatbox` | Pause method (Direct/Chatbox)          |
| **PauseToggleKey**       | `P`       | Configurable pause toggle keybind      |
| **AutoConfigureEnabled** | `false`   | Enable server auto-configuration       |
| **WhitelistEnabled**     | `false`   | Enable player whitelist system         |
| **LogPlayerIdentifiers** | `true`    | Log player IDs to help setup whitelist |

## 🛠️ Building from Source

```bash
# Prerequisites: .NET 5.0+, SMAPI development setup
git clone https://github.com/yourusername/smapi-multiplayer-helper
cd smapi-multiplayer-helper
dotnet build
# Built mod will be in bin/Debug/net5.0
```

## 🤝 Contributing

Contributions welcome! This mod uses:

- **Modular architecture** with separate managers for each feature
- **Comprehensive localization** system
- **Optional dependency patterns** for maximum compatibility

## 📄 License

This mod is open source under the MIT License.
Feel free to contribute, fork, or modify according to the license terms.

© 2025 Ka Pui Cheung
