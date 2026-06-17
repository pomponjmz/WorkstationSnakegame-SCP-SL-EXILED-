using Exiled.API.Interfaces;
using System.ComponentModel;

namespace WorkstationSnake
{
    /// <summary>
    /// Configuration for the WorkstationSnake plugin.
    /// Edit values in your EXILED/Configs/ YAML file — no recompile needed.
    /// </summary>
    public class Config : IConfig
    {
        // ── General ───────────────────────────────────────────────────────────

        [Description("Set to false to completely disable this plugin.")]
        public bool IsEnabled { get; set; } = true;

        [Description("Prints verbose debug lines to the server console.")]
        public bool Debug { get; set; } = false;

        // ── Game Settings ─────────────────────────────────────────────────────

        [Description("Width of the snake game board in characters. Default: 20")]
        public int BoardWidth { get; set; } = 20;

        [Description("Height of the snake game board in characters. Default: 10")]
        public int BoardHeight { get; set; } = 10;

        [Description("Seconds between each snake movement tick. Lower = faster. Default: 0.35")]
        public float TickRate { get; set; } = 0.35f;

        [Description("How often (seconds) the board is re-rendered to the hint panel. Very low = flicker-free. Default: 0.08")]
        public float RenderRate { get; set; } = 0.08f;

        [Description("How long the game session lasts in seconds before auto-closing. Default: 60")]
        public float SessionDuration { get; set; } = 60f;

        [Description("Cooldown in seconds before the same player can open the snake game again. Default: 5")]
        public float GameCooldown { get; set; } = 5f;

        [Description("Number of food items that can exist on the board at once. Default: 1")]
        public int FoodCount { get; set; } = 1;

        [Description("How many points to start the snake at (initial snake length). Default: 3")]
        public int InitialSnakeLength { get; set; } = 3;

        // ── Visual Settings ───────────────────────────────────────────────────

        [Description("Character used to draw the snake head.")]
        public string SnakeHead { get; set; } = "●";

        [Description("Character used to draw the snake body.")]
        public string SnakeBody { get; set; } = "○";

        [Description("Character used to draw food.")]
        public string FoodChar { get; set; } = "★";

        [Description("Character used to draw empty cells.")]
        public string EmptyChar { get; set; } = "·";

        [Description("Color of the snake head (HTML hex). Default: #00ff88")]
        public string ColorHead { get; set; } = "#00ff88";

        [Description("Color of the snake body (HTML hex). Default: #44aa66")]
        public string ColorBody { get; set; } = "#44aa66";

        [Description("Color of the food (HTML hex). Default: #ffdd00")]
        public string ColorFood { get; set; } = "#ffdd00";

        [Description("Color of the board border (HTML hex). Default: #555577")]
        public string ColorBorder { get; set; } = "#555577";

        [Description("Color of the score text (HTML hex). Default: #aaddff")]
        public string ColorScore { get; set; } = "#aaddff";

        [Description("Color of the game-over text (HTML hex). Default: #ff4444")]
        public string ColorGameOver { get; set; } = "#ff4444";

        [Description("Color of the title text (HTML hex). Default: #cc88ff")]
        public string ColorTitle { get; set; } = "#cc88ff";

        // ── Hint Settings ─────────────────────────────────────────────────────

        // NOTE: hint display duration is internally set to 3600s (persistent) to avoid flicker.
        //       RenderRate controls visual refresh speed instead.

        [Description("Message shown when the player opens the snake game.")]
        public string HintOpen { get; set; } =
            "<color=#cc88ff><b>🐍 SNAKE — Gun Workstation Easter Egg activated!</b></color>\n" +
            "<color=#aaaaaa><size=70%>Watch the snake eat. Closes automatically after 60s.</size></color>";

        [Description("Message shown when the game ends due to collision.")]
        public string HintGameOver { get; set; } =
            "<color=#ff4444><b>GAME OVER</b></color> — <color=#aaddff>The snake ate itself!</color>";

        [Description("Message shown when the session timer expires.")]
        public string HintTimeout { get; set; } =
            "<color=#aaaaaa>Snake session ended. Approach a workstation and press <b>[I]</b> to play again!</color>";

        [Description("Message shown when a player tries to open while on cooldown.")]
        public string HintCooldown { get; set; } =
            "<color=#ff8844>Workstation busy — try again in a moment.</color>";
    }
}
