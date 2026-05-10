# 🎮 VibeSplash — Playnite Splash Screen Addon

> ⚠️ **VIBE CODED** — This fork was written entirely with AI assistance (Claude). It may work perfectly. It may have quirks. It was not hand-crafted by a seasoned C# dev poring over the Playnite SDK at 2am. It was *vibed* into existence. Use it, enjoy it, and report issues with that in mind.

---

## What is VibeSplash?

VibeSplash is a fork of the excellent [Playnite-Splash-Addon by Artzox](https://github.com/artzox/Playnite-Splash-Addon). It shows a beautiful full-screen splash screen when you launch a game, combining your game's background image with its logo (via the Extra Metadata Loader plugin).

**VibeSplash adds one key feature on top of the original:**

### ✨ Splash Screen on Game Close
A new option lets the splash screen appear **after you close a game** — welcoming you back to Playnite. Perfect for that "returning to base" cinematic feel.

---

## Features

- Full-screen splash on **game launch** (original feature)
- Full-screen splash **after closing a game** ⚡ *(VibeSplash exclusive)*
- Displays game background image + logo (requires Extra Metadata Loader)
- Configurable splash duration (global, per-platform, or per-game)
- "Wait for game to start" timer mode
- Excluded game list
- Logo size control
- Smooth fade-in / fade-out animations

---

## Installation

1. Download the latest release from the [Releases](https://github.com/EvoShot/Playnite-Splash-Addon-VibeSplash/releases) page.
2. In Playnite, go to **Add-ons → Install from file** and select the `.pext` file.
3. Restart Playnite.
4. Configure via **Add-ons → VibeSplash → Settings**.

---

## Configuration

| Setting | Description |
|---|---|
| Default Duration | How long the splash shows (seconds) |
| Logo Size | Width of the game logo overlay (px, default 300) |
| Wait for game to start | Starts the timer only after the game process is detected |
| **Show splash on game close** | ⚡ New — shows splash when you return from a game |
| **Disable in Fullscreen mode** | ⚡ New — suppresses the splash entirely when Playnite is in Fullscreen mode |
| **Disable in Desktop mode** | ⚡ New — suppresses the splash entirely when Playnite is in Desktop mode |
| Platform-specific timers | Different durations per platform |
| Game-specific durations | Override duration for specific game IDs |
| Excluded Game IDs | Skip splash entirely for listed games |

---

## Requirements

- [Playnite](https://playnite.link/) (latest version recommended)
- [Extra Metadata Loader](https://playnite.link/addons.html) *(optional — for logo display)*

---

## Credits & Attribution

**Original addon:** [Playnite-Splash-Addon](https://github.com/artzox/Playnite-Splash-Addon) by [Artzox](https://github.com/artzox)  
**This fork:** [VibeSplash](https://github.com/EvoShot/Playnite-Splash-Addon-VibeSplash) by EvoShot

All core functionality is built on Artzox's original work. VibeSplash exists because of that solid foundation. Go star the original too. 🙏

---

## Disclaimer

> 🤖 **This addon was vibe coded.** That means it was developed through AI-assisted iteration rather than traditional software craftsmanship. The logic is sound, the feature works, but if something breaks in an unexpected way — that's the vibe tax. PRs welcome.

---

## License

Inherits from the original project. See the [original repository](https://github.com/artzox/Playnite-Splash-Addon) for licensing details.
