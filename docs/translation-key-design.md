# Translation Key Design for MultiplayerHelper

## Overview

This document outlines the comprehensive translation key structure for the MultiplayerHelper SMAPI mod. The design follows SMAPI i18n conventions and hierarchical organization principles to ensure scalability, maintainability, and logical grouping of translated content.

## Key Structure Philosophy

### Naming Convention
- **Format**: `{component}.{specific_key}` or `{component}.{category}.{specific_key}` 
- **Case**: `kebab-case` for consistency with SMAPI conventions
- **Hierarchy**: Maximum 3 levels deep to prevent over-nesting
- **Descriptive**: Keys should be self-documenting and concise

### Organization Principles
1. **Component-based**: Group by functionality/component
2. **Context-aware**: Keys reflect where and how they're used
3. **Future-proof**: Structure accommodates planned features
4. **SMAPI-aligned**: Follows SMAPI i18n best practices

## Current String Analysis

### User-Facing Strings (Require Translation)
```
HUD Messages:
- "Invite code copied to clipboard: {code}"
- "Game paused - Press P to resume"
- "Game resumed"
- "Game paused"
- "Invite code (copy manually): {code}" (fallback)

Log Messages (User-visible in SMAPI console):
- "MultiplayerHelper mod loaded with modular architecture!"
- "Auto-paused game using direct pause method (press P to resume)"
- "Sent /pause command via chatbox"
- "Game resumed by pressing P"
- "Game paused by pressing P"
```

### Debug/Internal Strings (Optional Translation)
```
- "InviteCodeManager initialized"
- "AutoPauseManager initialized (useDirectPause: {flag})"
- Various debug and trace messages
- Error messages
```

## Translation Key Structure

Following SMAPI conventions, keys use kebab-case and are organized by component with minimal nesting.

```json
{
  // Core mod information
  "core.name": "Multiplayer Helper",
  "core.description": "Automatically copies multiplayer invite codes and pauses game when host joins",
  "core.loaded": "MultiplayerHelper mod loaded successfully!",
  
  // Invite code functionality - HUD messages (user-facing)
  "invite.hud-copied": "Invite code copied to clipboard: {code}",
  "invite.hud-manual": "Invite code (copy manually): {code}",
  
  // Invite code functionality - Log messages  
  "invite.log-detecting": "Server detected! Attempting to get invite code...",
  "invite.log-found": "Found invite code: {code}",
  "invite.log-copied": "Copied invite code to clipboard: {code}",
  "invite.log-unavailable": "No invite code available yet",
  
  // Invite code functionality - Error messages
  "invite.error-clipboard": "Error copying to clipboard: {error}",
  "invite.error-detection": "Error getting invite code: {error}",
  
  // Pause functionality - HUD messages (user-facing)
  "pause.hud-paused-direct": "Game paused - Press P to resume",
  "pause.hud-resumed": "Game resumed", 
  "pause.hud-paused-manual": "Game paused",
  
  // Pause functionality - Log messages
  "pause.log-auto-direct": "Auto-paused game using direct pause method (press P to resume)",
  "pause.log-auto-chatbox": "Sent /pause command via chatbox",
  "pause.log-manual-resumed": "Game resumed by pressing P",
  "pause.log-manual-paused": "Game paused by pressing P",
  "pause.log-already-paused": "Game is already paused",
  "pause.log-player-connected": "Player {playerName} connected to the game",
  
  // Pause functionality - Error messages
  "pause.error-failed": "Error auto-pausing game: {error}",
  "pause.error-chatbox-unavailable": "Chatbox not available yet",
  
  // Component initialization (debug/development)
  "debug.invite-manager-init": "InviteCodeManager initialized",
  "debug.pause-manager-init": "AutoPauseManager initialized (useDirectPause: {useDirectPause})",
  "debug.game-launched": "MultiplayerHelper: Game launched, configuring components...",
  "debug.save-loaded-invite": "Save loaded. Reset hosted session flag. IsMainPlayer: {isMainPlayer}, IsMultiplayer: {isMultiplayer}",
  "debug.save-loaded-pause": "Save loaded. Reset auto-pause flag. IsMainPlayer: {isMainPlayer}, IsMultiplayer: {isMultiplayer}",
  
  // Future extensibility - Configuration UI
  "config.title": "Multiplayer Helper Settings",
  "config.general": "General Settings", 
  "config.invite-settings": "Invite Code Settings",
  "config.pause-settings": "Pause Settings",
  "config.enabled": "Enabled",
  "config.disabled": "Disabled",
  "config.use-direct-pause": "Use Direct Pause Method",
  "config.auto-copy-invite": "Auto-Copy Invite Codes",
  "config.pause-on-host-join": "Pause When Host Joins",
  
  // Future extensibility - Controls/Help
  "help.pause-key": "Press P to pause/resume",
  "help.direct-pause": "Direct pause (local only)",
  "help.chatbox-pause": "Multiplayer vote pause (synced)",
  "help.mod-purpose": "This mod helps with multiplayer sessions by automatically copying invite codes and managing game pause state"
}
```

## File Organization

### Directory Structure
Following SMAPI conventions, translation files are placed in an `i18n/` subfolder with `default.json` as the fallback. Only officially supported Stardew Valley languages are included:

```
i18n/
├── default.json         # English (fallback - required by SMAPI)
├── de.json             # German (Deutsch)
├── es.json             # Spanish (Español)  
├── fr.json             # French (Français)
├── hu.json             # Hungarian (Magyar)
├── it.json             # Italian (Italiano)
├── ja.json             # Japanese (日本語)
├── ko.json             # Korean (한국어)
├── pt.json             # Portuguese (Português - Brazil)
├── ru.json             # Russian (Русский)
├── tr.json             # Turkish (Türkçe)
└── zh.json             # Chinese Simplified (简体中文)
```

### File Format (JSON)
Following SMAPI's flat key structure with parameters using `{token}` format:

```json
{
  "core.name": "Multiplayer Helper",
  "core.description": "Automatically copies multiplayer invite codes and pauses game when host joins",
  "core.loaded": "MultiplayerHelper mod loaded successfully!",
  
  "invite.hud-copied": "Invite code copied to clipboard: {code}",
  "invite.hud-manual": "Invite code (copy manually): {code}",
  
  "pause.hud-paused-direct": "Game paused - Press P to resume",
  "pause.hud-resumed": "Game resumed",
  
  "pause.log-player-connected": "Player {playerName} connected to the game"
}
```

## Implementation Considerations

### SMAPI Translation Helper Usage
```csharp
// Simple translation
Helper.Translation.Get("core.loaded")

// Translation with parameters
Helper.Translation.Get("invite.hud-copied", new { code = inviteCode })
Helper.Translation.Get("pause.log-player-connected", new { playerName = player.Name })

// With fluent interface for fallbacks
Helper.Translation.Get("invite.hud-copied")
    .Tokens(new { code = inviteCode })
    .Default("Invite code copied: {code}")
```

### Parameter Formatting
- Use `{parameter}` for string interpolation (SMAPI standard)
- Support named parameters: `{code}`, `{playerName}`, `{error}`
- Handle pluralization where needed: `{count} player(s)`
- Parameters are case-insensitive in SMAPI

### Fallback Strategy (Handled by SMAPI)
1. Specific key in current language file
2. Specific key in `default.json`
3. Custom default text if provided
4. "Missing translation: {key}" if no fallback

### Performance
- SMAPI handles lazy loading and caching automatically
- Translation files are loaded once at startup
- `reload_i18n` console command for development testing

## Future Extensibility

### Planned Features
- **Configuration UI**: Menu strings, tooltips, validation messages
- **Advanced Pause Options**: Custom pause reasons, timer displays  
- **Invite Code Enhancements**: QR codes, custom messages, expiration
- **Statistics/Analytics**: Usage tracking, session info
- **Integration Features**: Discord webhooks, third-party mod compatibility

### Key Categories for Future Features
```json
{
  // Configuration UI (when implemented)
  "config.tooltip-direct-pause": "Use direct pause (faster but may conflict with other mods)",
  "config.tooltip-chatbox-pause": "Use multiplayer pause voting (slower but compatible)",
  "config.validation-invalid": "Invalid setting value",
  
  // Statistics (when implemented)  
  "stats.sessions-hosted": "Sessions Hosted: {count}",
  "stats.codes-copied": "Invite Codes Copied: {count}",
  "stats.pauses-triggered": "Auto-Pauses Triggered: {count}",
  
  // Integrations (when implemented)
  "integration.discord-sent": "Invite code sent to Discord",
  "integration.mod-conflict": "Conflict detected with {modName}"
}
```

## Translation Guidelines

### For Translators
1. **Context is Key**: Understand where text appears (HUD vs logs vs config)
2. **Consistency**: Use consistent terminology throughout
3. **Brevity**: HUD messages should be concise (~50 characters max)
4. **Gaming Culture**: Adapt to local Stardew Valley terminology
5. **Testing**: Use `reload_i18n` console command to test changes

### Technical Constraints
- **HUD Messages**: Max ~50 characters for proper display
- **Parameter Preservation**: `{code}` tokens must remain unchanged
- **Hotkey References**: Adapt key names to local keyboard layouts where appropriate
- **Special Characters**: Test with game's font rendering

## Validation Rules

### Key Naming Standards
- Use `kebab-case` following SMAPI conventions
- Maximum 3 hierarchy levels: `component.category.key`
- Must be unique across all translation files
- No spaces or special characters except hyphens and dots

### Translation Content Requirements
- Parameters must be preserved exactly: `{code}` → `{code}`
- Parameter names cannot be translated
- Maintain consistent terminology across all keys
- No HTML, markdown, or special formatting

## Development Workflow

### Adding New Translatable Strings
1. Add key to `default.json` with English text
2. Replace hardcoded string in code with `Helper.Translation.Get("key")`
3. Test with `reload_i18n` console command
4. Add translations to other language files as needed

### Best Practices
- Always provide meaningful default text
- Use descriptive key names that indicate usage context
- Group related keys by component prefix
- Document parameter meanings in code comments

This design ensures our i18n system follows SMAPI best practices while providing excellent localization support for all official Stardew Valley languages.