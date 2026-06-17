<div align="center">

# 🐍 WorkstationSnake

**A hidden Snake game Easter egg for SCP: Secret Laboratory**

[![EXILED](https://img.shields.io/badge/EXILED-8.0%2B-blueviolet?style=for-the-badge)](https://github.com/ExMod-Team/EXILED)
[![SCP:SL](https://img.shields.io/badge/SCP%3ASL-Compatible-red?style=for-the-badge)](https://store.steampowered.com/app/700330/)
[![.NET](https://img.shields.io/badge/.NET-4.8-blue?style=for-the-badge)](https://dotnet.microsoft.com/)
[![Version](https://img.shields.io/badge/version-1.0.0-brightgreen?style=for-the-badge)](https://github.com/)
[![License](https://img.shields.io/badge/license-MIT-orange?style=for-the-badge)](LICENSE)

*Walk up to any Gun Workstation, press* ***[I]***, *and discover a fully playable Snake game hiding inside it.*

</div>

---

## 📖 Overview

WorkstationSnake is an [EXILED](https://github.com/ExMod-Team/EXILED) plugin that embeds a **fully playable Snake game** as a secret Easter egg inside every Gun Workstation on the map. When a player interacts with a workstation the SCP:SL hint panel transforms into an animated ASCII game board — and the player **actually steers the snake** using their movement keys.

The game is rendered entirely using SCP:SL's built-in rich-text hint system with box-drawing characters, Unicode symbols, and per-element colouring — no client-side mods required.

---

## ✨ Features

| | |
|---|---|
| 🎮 **Fully playable** | Player steers the snake with WASD — the game is not just a passive animation |
| 🔒 **Movement lock** | Player is frozen in place during the session so WASD input only controls the snake |
| 🖥️ **Flicker-free display** | Board re-renders at 0.08 s intervals with a persistent hint duration — no fade-out flashing |
| 🎨 **Fully themeable** | Every colour, character, board size, speed and message is configurable in YAML |
| ⏱️ **Session management** | Auto-closes after a configurable timeout; per-player cooldown prevents spam |
| 🧹 **Safe cleanup** | Sessions are killed automatically on player death, disconnect, or plugin disable |
| 💀 **Real game over** | Hit a wall or yourself and the board freezes on the crash frame before showing the score |
| 🐛 **Robust error handling** | All event handlers are wrapped in try-catch; Exiled 9 null-player edge cases handled |

---

## 🎮 How to Play

1. **Find** any Gun Workstation in the facility (Armory rooms, Surface, etc.)
2. **Look** at it and press **`[I]`** (the interact key)
3. The hint panel turns into the Snake board — **you are now playing**
4. Use **`W A S D`** to steer:

   | Key | Direction |
   |:---:|:---:|
   | `W` | ⬆ Up |
   | `S` | ⬇ Down |
   | `A` | ⬅ Left |
   | `D` | ➡ Right |

5. Eat **★** food to grow and score points
6. Avoid hitting the **walls** or your own **body**
7. The game closes automatically after 60 seconds, or on game over

> **Note:** You cannot move while playing — your WASD input is captured by the game. You will be snapped back to your starting position until the session ends.

---

## 📦 Installation

1. Download the latest `WorkstationSnake.dll` from the [Releases](../../releases) page
2. Drop it into your server's `EXILED/Plugins/` folder
3. **Restart** the server — the config file is generated automatically at `EXILED/Configs/`
4. Done ✅

---

## ⚙️ Configuration

The config file is located at `EXILED/Configs/<port>-config.yml` under the key `workstation_snake`.

```yaml
workstation_snake:

  # ── General ────────────────────────────────────────────────────────────────
  is_enabled: true        # Set to false to disable the plugin entirely
  debug: false            # Print verbose session logs to the server console

  # ── Game ───────────────────────────────────────────────────────────────────
  board_width: 20         # Board width  (clamped 8–30)
  board_height: 10        # Board height (clamped 5–20)
  tick_rate: 0.35         # Seconds between each snake move (lower = faster)
  render_rate: 0.08       # Seconds between display refreshes (lower = smoother)
  session_duration: 60    # Seconds before the session auto-closes
  game_cooldown: 5        # Cooldown (seconds) before a player can start again
  food_count: 1           # Food items on the board at once
  initial_snake_length: 3 # Starting snake length

  # ── Appearance ─────────────────────────────────────────────────────────────
  snake_head: "●"         # Character for the snake head
  snake_body: "○"         # Character for the snake body
  food_char:  "★"         # Character for food
  empty_char: "·"         # Character for empty cells

  color_head:      "#00ff88"  # Snake head colour
  color_body:      "#44aa66"  # Snake body colour
  color_food:      "#ffdd00"  # Food colour
  color_border:    "#555577"  # Board border colour
  color_score:     "#aaddff"  # Score text colour
  color_game_over: "#ff4444"  # Game-over message colour
  color_title:     "#cc88ff"  # Title bar colour

  # ── Messages (supports SCP:SL rich-text: <b>, <color=#hex>, <size=N%>) ────
  hint_open:     "<color=#cc88ff><b>🐍 SNAKE — Gun Workstation Easter Egg activated!</b></color>..."
  hint_game_over: "<color=#ff4444><b>GAME OVER</b></color> — <color=#aaddff>The snake ate itself!</color>"
  hint_timeout:  "<color=#aaaaaa>Snake session ended. Approach a workstation and press <b>[I]</b> to play again!</color>"
  hint_cooldown: "<color=#ff8844>Workstation busy — try again in a moment.</color>"
```

---

## 🏗️ Project Structure

```
WorkstationSnake/
├── Plugin.cs           # Entry point — registers/unregisters event hooks
├── EventHandlers.cs    # Session coroutines, WASD detection, position locking
├── SnakeGame.cs        # Self-contained game engine — board, movement, rendering
├── Config.cs           # All YAML-configurable settings
└── WorkstationSnake.csproj
```

### How the input system works

SCP:SL does not expose raw keyboard events server-side. WorkstationSnake detects WASD input by:

1. **Saving** the player's world position when the session starts (`lockedPosition`)
2. **Polling** every `render_rate` seconds — if the player's position drifted more than **3 cm** from `lockedPosition`:
   - Their intended direction is calculated from the XZ position delta projected into **local space**
   - `game.SetDirection()` is called with the resolved `W/A/S/D` vector
   - The player is **immediately teleported back** to `lockedPosition`
3. The net effect: the player cannot move, but every keypress is registered as a directional input

### How the flicker-free display works

Hints in SCP:SL fade out when their duration expires. To prevent the board from flickering:
- Each frame sends the hint with a **3600-second duration** (effectively permanent)
- A new frame is sent every `render_rate` seconds **before** the previous one would visually change
- The client updates the displayed text instantly without triggering the fade animation

---

## 🔧 Requirements

| Requirement | Version |
|---|---|
| [EXILED](https://github.com/ExMod-Team/EXILED) | ≥ 8.0.0 |
| .NET Framework | 4.8 |
| SCP: Secret Laboratory | Latest stable |

---

## 📜 License

This project is licensed under the **MIT License** — see [LICENSE](LICENSE) for details.  
You are free to use, modify, and redistribute this plugin with attribution.

---

## 🤝 Contributing

Pull requests are welcome! If you find a bug or want to suggest a feature, please open an [Issue](../../issues).

When contributing, please:
- Keep code style consistent with the existing files
- Test your changes on a live server before submitting
- Update this README if you add or change configurable options

---

<div align="center">

Made with ❤️ Please credit if you use it!

</div>
