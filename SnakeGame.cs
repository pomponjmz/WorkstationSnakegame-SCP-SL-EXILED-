using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WorkstationSnake
{
    /// <summary>
    /// Manages a single Snake game session for one player.
    /// Direction is set externally by EventHandlers (via position-delta WASD detection).
    /// When no new direction is provided the snake continues in its last direction.
    /// No AI — if the player does nothing the snake goes straight until it hits a wall.
    /// </summary>
    internal sealed class SnakeGame
    {
        // ─── Board ────────────────────────────────────────────────────────────
        private readonly int _w;
        private readonly int _h;
        private readonly Config _cfg;
        private readonly System.Random _rng = new System.Random();

        // ─── Snake state ──────────────────────────────────────────────────────
        private readonly LinkedList<Vector2Int> _body = new LinkedList<Vector2Int>();
        /// <summary>Current travel direction. Updated only by player input.</summary>
        private Vector2Int _dir;
        private readonly HashSet<Vector2Int> _bodySet = new HashSet<Vector2Int>();

        // ─── Food ─────────────────────────────────────────────────────────────
        private readonly List<Vector2Int> _food = new List<Vector2Int>();

        // ─── Public state ──────────────────────────────────────────────────────
        internal int Score { get; private set; }
        internal bool IsAlive { get; private set; } = true;

        // ─────────────────────────────────────────────────────────────────────

        internal SnakeGame(Config cfg)
        {
            _cfg = cfg;
            _w = Mathf.Clamp(cfg.BoardWidth,  8, 30);
            _h = Mathf.Clamp(cfg.BoardHeight, 5, 20);

            // Start in the centre, moving right
            int midX = _w / 2;
            int midY = _h / 2;
            _dir = new Vector2Int(1, 0);

            int len = Mathf.Clamp(cfg.InitialSnakeLength, 1, _w - 2);
            for (int i = len - 1; i >= 0; i--)
            {
                var seg = new Vector2Int(midX - i, midY);
                _body.AddLast(seg);
                _bodySet.Add(seg);
            }

            for (int i = 0; i < cfg.FoodCount; i++)
                SpawnFood();
        }

        // ─── Input ────────────────────────────────────────────────────────────

        /// <summary>
        /// Called by EventHandlers when a WASD key press is detected.
        /// Ignores 180° reversals (you can't turn back into yourself).
        /// </summary>
        internal void SetDirection(Vector2Int newDir)
        {
            if (newDir == Vector2Int.zero) return;
            // Prevent direct U-turn
            if (newDir == -_dir) return;
            _dir = newDir;
        }

        // ─── Game tick ────────────────────────────────────────────────────────

        /// <summary>
        /// Moves the snake one step in its current direction.
        /// Returns <c>false</c> if the snake collided (game over).
        /// </summary>
        internal bool Tick()
        {
            if (!IsAlive) return false;

            // No AI — direction is whatever the player last set (or the initial right-ward direction)
            Vector2Int head = _body.Last!.Value;
            Vector2Int next = head + _dir;

            // Wall collision
            if (next.x < 0 || next.x >= _w || next.y < 0 || next.y >= _h)
            {
                IsAlive = false;
                return false;
            }

            // Self collision
            if (_bodySet.Contains(next))
            {
                IsAlive = false;
                return false;
            }

            _body.AddLast(next);
            _bodySet.Add(next);

            bool ate = _food.Remove(next);
            if (ate)
            {
                Score++;
                SpawnFood();
            }
            else
            {
                _bodySet.Remove(_body.First!.Value);
                _body.RemoveFirst();
            }

            return true;
        }

        // ─── Rendering ────────────────────────────────────────────────────────

        /// <summary>Returns the board as SCP:SL rich-text hint markup.</summary>
        /// <param name="playerSteering">True when the player pressed a key this frame.</param>
        internal string Render(bool playerSteering)
        {
            var sb = new StringBuilder();

            // Title + score
            sb.Append($"<color={_cfg.ColorTitle}><b>🐍 WORKSTATION SNAKE</b></color>");
            sb.AppendLine($"   <color={_cfg.ColorScore}>Score: <b>{Score}</b></color>");

            // Controls status
            if (playerSteering)
                sb.AppendLine("<color=#44ff88><size=70%><b>WASD</b> — steering active ✔</size></color>");
            else
                sb.AppendLine("<color=#aaaaaa><size=70%>Use <b>WASD</b> to steer the snake</size></color>");

            // Top border
            sb.Append($"<color={_cfg.ColorBorder}><mspace=0.55em>╔");
            sb.Append('═', _w);
            sb.AppendLine("╗</mspace></color>");

            // Board
            bool[,] bodyGrid = new bool[_w, _h];
            Vector2Int snakeHead = _body.Last!.Value;
            foreach (var seg in _body)
                bodyGrid[seg.x, seg.y] = true;
            var foodSet = new HashSet<Vector2Int>(_food);

            for (int y = _h - 1; y >= 0; y--)
            {
                sb.Append($"<color={_cfg.ColorBorder}><mspace=0.55em>║</mspace></color><mspace=0.55em>");

                for (int x = 0; x < _w; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (cell == snakeHead)
                        sb.Append($"<color={_cfg.ColorHead}>{_cfg.SnakeHead}</color>");
                    else if (bodyGrid[x, y])
                        sb.Append($"<color={_cfg.ColorBody}>{_cfg.SnakeBody}</color>");
                    else if (foodSet.Contains(cell))
                        sb.Append($"<color={_cfg.ColorFood}>{_cfg.FoodChar}</color>");
                    else
                        sb.Append($"<color=#2a2a3a>{_cfg.EmptyChar}</color>");
                }

                sb.AppendLine($"</mspace><color={_cfg.ColorBorder}><mspace=0.55em>║</mspace></color>");
            }

            // Bottom border
            sb.Append($"<color={_cfg.ColorBorder}><mspace=0.55em>╚");
            sb.Append('═', _w);
            sb.Append("╝</mspace></color>");

            if (!IsAlive)
                sb.Append($"\n<color={_cfg.ColorGameOver}><b>💀 GAME OVER — Score: {Score}</b></color>");

            return sb.ToString();
        }

        // ─── Food spawner ─────────────────────────────────────────────────────

        private void SpawnFood()
        {
            for (int attempt = 0; attempt < 100; attempt++)
            {
                var pos = new Vector2Int(_rng.Next(_w), _rng.Next(_h));
                if (!_bodySet.Contains(pos) && !_food.Contains(pos))
                {
                    _food.Add(pos);
                    return;
                }
            }
        }
    }
}
