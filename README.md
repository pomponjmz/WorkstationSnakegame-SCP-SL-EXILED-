# WorkstationSnake 🐍

An Easter-egg plugin for **SCP: Secret Laboratory** (EXILED 8+) that launches a **Snake game** on a player's screen whenever they interact with a **Gun Workstation** (press `[I]` while looking at one).

---

## How It Works

1. A player approaches any Gun Workstation and presses `[I]` (the interact key).
2. The EXILED `ActivatingWorkstation` event fires — the plugin intercepts it.
3. A per-player Snake coroutine starts, rendering the board as SCP:SL **hint panel** rich-text each tick.
4. An AI steers the snake toward food automatically — players watch the game unfold on-screen.
5. The session ends when:
   - The snake hits a wall or itself (**Game Over** message shown), or
   - The session timer expires (default 60 s).
6. A short **cooldown** (default 5 s) prevents instant re-triggering.

> **Why AI-controlled?**  
> SCP:SL does not expose real-time keyboard input server-side, so the snake plays
> itself using a greedy pathfinding algorithm — identical to how the CI Card Snake
> Easter egg works.

---

## Features

| Feature | Details |
|---|---|
| 🐍 Snake game | Fully animated ASCII board in the hint panel |
| 🤖 Smart AI | Greedy pathfinder keeps the snake alive as long as possible |
| 🎨 Configurable colours | Every colour is an HTML hex in the YAML config |
| ⏱ Configurable speed | `TickRate` sets seconds per move (lower = faster) |
| 🔒 Cooldown | Prevents spam-triggering |
| 🧹 Safe cleanup | Sessions killed on player leave/death and plugin disable |

---

## Installation

1. Build the project or grab the compiled `WorkstationSnake.dll`.
2. Drop `WorkstationSnake.dll` into your server's `EXILED/Plugins/` folder.
3. Restart the server — a config file is auto-generated in `EXILED/Configs/`.

---

## Configuration (`EXILED/Configs/workstation_snake.yml`)

```yaml
workstation_snake:
  is_enabled: true
  debug: false

  # Board dimensions (characters)
  board_width: 20
  board_height: 10

  # Seconds between each snake move
  tick_rate: 0.35

  # Session auto-closes after this many seconds
  session_duration: 60

  # Cooldown before the same player can trigger again
  game_cooldown: 5

  # Starting snake length
  initial_snake_length: 3

  # Visual characters
  snake_head: "●"
  snake_body: "○"
  food_char:  "★"
  empty_char: "·"

  # Colours (HTML hex)
  color_head:      "#00ff88"
  color_body:      "#44aa66"
  color_food:      "#ffdd00"
  color_border:    "#555577"
  color_score:     "#aaddff"
  color_game_over: "#ff4444"
  color_title:     "#cc88ff"
```

---

## Requirements

- **EXILED** ≥ 8.0.0
- **.NET 4.8** (matches the game server)

---

## Credits

Made for **Pustkownia** SCP:SL server.
