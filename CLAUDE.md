# Development Notes for MultiplayerHelper

## Internationalization (i18n) System

**📋 IMPORTANT: Always use the i18n system for any user-facing strings!**

### Quick Reference
- **Translation files**: Located in `i18n/` directory
- **Key format**: `kebab-case` with component prefixes
- **Parameters**: Use `{parameterName}` format
- **Usage**: `Helper.Translation.Get("key.name", new { param = value })`

### Key Naming Convention
```
{component}.{category}-{specific}
```

**Components:**
- `core.*` - Core mod functionality and messages
- `invite.*` - Invite code detection and copying
- `pause.*` - Game pause/resume functionality  
- `debug.*` - Debug/development messages
- `config.*` - Configuration UI (future)
- `help.*` - Help and documentation text (future)

**Categories:**
- `hud-*` - HUD messages shown to players (HIGH PRIORITY)
- `log-*` - Console log messages
- `error-*` - Error messages
- `tooltip-*` - UI tooltips (future)

### Supported Languages
Only use languages officially supported by Stardew Valley:
- `default.json` (English - fallback)
- `de.json` (German), `es.json` (Spanish), `fr.json` (French)
- `hu.json` (Hungarian), `it.json` (Italian), `ja.json` (Japanese)
- `ko.json` (Korean), `pt.json` (Portuguese-BR), `ru.json` (Russian)
- `tr.json` (Turkish), `zh.json` (Chinese Simplified)

### Adding New Strings
1. **Never hardcode strings in C# files**
2. Add key to `i18n/default.json` first
3. Use `Helper.Translation.Get("key.name")` in code
4. Test with `reload_i18n` console command
5. Document parameters in code comments

### Examples
```csharp
// Simple message
Helper.Translation.Get("core.loaded")

// Message with parameters
Helper.Translation.Get("invite.hud-copied", new { code = inviteCode })

// With fallback for safety
Helper.Translation.Get("pause.hud-resumed")
    .Default("Game resumed")
```

### Current Key Structure
See `docs/translation-key-design.md` for complete key catalog.

**Most Important Keys (HUD Messages):**
- `invite.hud-copied` - Invite code copied notification
- `invite.hud-manual` - Manual copy fallback message
- `pause.hud-paused-direct` - Direct pause notification
- `pause.hud-resumed` - Game resumed notification
- `pause.hud-paused-manual` - Manual pause notification

## Architecture Notes

### Modular Design
- **ModEntry.cs** - Central orchestrator, minimal logic
- **Components/InviteCodeManager.cs** - Invite code functionality
- **Components/AutoPauseManager.cs** - Pause/resume functionality
- **Components/** - Add new features as separate components

### Best Practices
- Each component handles its own events and cleanup
- Use `Dispose()` pattern for proper cleanup
- Keep ModEntry minimal - delegate to components
- Follow SMAPI conventions and patterns

## Development Workflow

### When Adding Features
1. Create component class in `Components/` directory
2. Add i18n keys to `default.json` for any strings
3. Initialize component in `ModEntry.Entry()`
4. Add disposal in `ModEntry.Dispose()`
5. Test with various scenarios

### When Modifying Strings  
1. Update translation keys, never hardcode
2. Test with `reload_i18n` console command
3. Consider impact on all supported languages
4. Update `docs/translation-key-design.md` if needed

## Build and CI/CD

### Automated Processes
- **GitHub Actions** - CI/CD pipeline in `.github/workflows/`
- **VS Code** - Tasks and debug configuration in `.vscode/`
- **Formatting** - StyleCop + EditorConfig for code style
- **Auto-build** - File watcher with `watch-build.sh`

### Quality Checks
- Build validation on all PRs
- Code formatting enforcement
- Analyzer warnings (StyleCop + Microsoft.CodeAnalysis.NetAnalyzers)

---

**Remember**: This project uses modular architecture with comprehensive i18n support. Always follow the established patterns when extending functionality!