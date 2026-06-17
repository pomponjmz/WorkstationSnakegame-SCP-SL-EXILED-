using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace WorkstationSnake
{
    /// <summary>
    /// Handles all game events for the WorkstationSnake plugin.
    ///
    /// WASD input is detected by comparing the player's position each frame.
    /// If the player moved even a tiny bit, their direction is captured and
    /// they are immediately teleported back to their locked start position —
    /// effectively freezing them in place while still registering directional input.
    /// </summary>
    public class EventHandlers
    {
        private readonly Config _cfg;

        private readonly Dictionary<string, CoroutineHandle> _sessions  = new Dictionary<string, CoroutineHandle>();
        private readonly Dictionary<string, DateTime>        _cooldowns = new Dictionary<string, DateTime>();

        // Minimum position delta (metres) that counts as a keypress.
        // Walk speed is ~3.5 m/s; at 0.08 s poll rate that's ~0.28 m per frame,
        // so even a tiny threshold catches it well before the player visually moves.
        private const float MoveDeltaThreshold = 0.03f;

        public EventHandlers(Config cfg) => _cfg = cfg;

        // ─── Event: Player activates a workstation ─────────────────────────────

        public void OnActivatingWorkstation(ActivatingWorkstationEventArgs ev)
        {
            if (!_cfg.IsEnabled) return;
            try
            {
                if (ev?.Player == null || !ev.Player.IsConnected) return;

                string uid = ev.Player.UserId;

                if (_cooldowns.TryGetValue(uid, out var cooldownEnd) && DateTime.UtcNow < cooldownEnd)
                {
                    ev.Player.ShowHint(_cfg.HintCooldown, 3f);
                    return;
                }

                if (_sessions.ContainsKey(uid)) return; // already playing

                ev.Player.ShowHint(_cfg.HintOpen, 3f);
                var handle = Timing.RunCoroutine(RunSnakeSession(ev.Player));
                _sessions[uid] = handle;

                if (_cfg.Debug)
                    Log.Debug($"[WorkstationSnake] Session started for {ev.Player.Nickname}.");
            }
            catch (Exception ex) { Log.Error($"[WorkstationSnake] OnActivatingWorkstation: {ex}"); }
        }

        // ─── Event: Player leaves ─────────────────────────────────────────────

        public void OnPlayerLeft(LeftEventArgs ev)
        {
            try { if (ev?.Player != null) EndSession(ev.Player.UserId, ev.Player.Nickname); }
            catch (Exception ex) { Log.Error($"[WorkstationSnake] OnPlayerLeft: {ex}"); }
        }

        // ─── Event: Player dies ───────────────────────────────────────────────

        public void OnPlayerDied(DiedEventArgs ev)
        {
            try { if (ev?.Player != null) EndSession(ev.Player.UserId, ev.Player.Nickname); }
            catch (Exception ex) { Log.Error($"[WorkstationSnake] OnPlayerDied: {ex}"); }
        }

        // ─── Session coroutine ────────────────────────────────────────────────

        private IEnumerator<float> RunSnakeSession(Player player)
        {
            string uid = player.UserId;
            var game   = new SnakeGame(_cfg);

            float renderRate = Mathf.Clamp(_cfg.RenderRate, 0.05f, 0.5f);
            float tickRate   = Mathf.Max(_cfg.TickRate, renderRate);
            float tickTimer  = 0f;
            float elapsed    = 0f;

            // Persistent hint duration — never expires on-screen; we keep re-sending it
            const float HintDuration = 3600f;

            // Short intro pause
            yield return Timing.WaitForSeconds(1.2f);

            // ── Capture the spawn position — this is the player's lock point ──
            Vector3 lockedPosition = player.Position;

            while (true)
            {
                // ── Safety ────────────────────────────────────────────────────
                if (player == null || !player.IsConnected) break;

                // ── Session timeout ───────────────────────────────────────────
                if (elapsed >= _cfg.SessionDuration)
                {
                    if (player.IsConnected)
                        player.ShowHint(_cfg.HintTimeout, 8f);
                    break;
                }

                // ── Detect WASD via position delta, then freeze player ─────────
                bool playerSteering = false;
                try
                {
                    Vector3 currentPos = player.Position;
                    Vector3 delta      = currentPos - lockedPosition;

                    // Ignore vertical movement (falling, stairs) — only XZ matters
                    Vector3 deltaXZ = new Vector3(delta.x, 0f, delta.z);

                    if (deltaXZ.magnitude >= MoveDeltaThreshold)
                    {
                        // Convert world-space XZ delta into the player's local space
                        // so W = forward relative to the player's facing direction.
                        Vector3 localDelta = player.Transform.InverseTransformDirection(deltaXZ);

                        Vector2Int inputDir;
                        if (Mathf.Abs(localDelta.z) >= Mathf.Abs(localDelta.x))
                        {
                            // Forward / Backward dominant
                            inputDir = localDelta.z > 0
                                ? new Vector2Int(0, 1)   // W → snake UP
                                : new Vector2Int(0, -1); // S → snake DOWN
                        }
                        else
                        {
                            // Left / Right dominant
                            inputDir = localDelta.x > 0
                                ? new Vector2Int(1, 0)   // D → snake RIGHT
                                : new Vector2Int(-1, 0); // A → snake LEFT
                        }

                        game.SetDirection(inputDir);
                        playerSteering = true;

                        // ── Snap the player back — they don't actually move ───
                        player.Position = lockedPosition;
                    }
                    else
                    {
                        // Keep Y-locked as well to prevent slow drift on slopes
                        if (Mathf.Abs(delta.y) > 0.1f)
                            player.Position = lockedPosition;
                    }
                }
                catch { /* ignore position read/write errors on edge frames */ }

                // ── Advance the snake at TickRate intervals ───────────────────
                tickTimer += renderRate;
                if (tickTimer >= tickRate)
                {
                    tickTimer = 0f;
                    bool alive = game.Tick();

                    if (!alive)
                    {
                        // Show game-over frame then end
                        try { player.ShowHint(game.Render(false), HintDuration); } catch { }
                        yield return Timing.WaitForSeconds(3.5f);

                        if (player != null && player.IsConnected)
                            player.ShowHint(_cfg.HintGameOver, 8f);
                        break;
                    }
                }

                // ── Render (flicker-free: long duration, re-sent every RenderRate) ──
                try { player.ShowHint(game.Render(playerSteering), HintDuration); }
                catch { break; }

                elapsed   += renderRate;
                yield return Timing.WaitForSeconds(renderRate);
            }

            _cooldowns[uid] = DateTime.UtcNow.AddSeconds(_cfg.GameCooldown);
            _sessions.Remove(uid);

            if (_cfg.Debug)
                Log.Debug($"[WorkstationSnake] Session ended for {player?.Nickname ?? uid}. Score: {game.Score}");
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private void EndSession(string uid, string nickname)
        {
            if (string.IsNullOrEmpty(uid)) return;
            if (!_sessions.TryGetValue(uid, out var handle)) return;

            Timing.KillCoroutines(handle);
            _sessions.Remove(uid);
            _cooldowns[uid] = DateTime.UtcNow.AddSeconds(_cfg.GameCooldown);

            if (_cfg.Debug)
                Log.Debug($"[WorkstationSnake] Session force-closed for {nickname}.");
        }

        public void KillAllSessions()
        {
            foreach (var kv in _sessions)
                Timing.KillCoroutines(kv.Value);
            _sessions.Clear();
            _cooldowns.Clear();
        }
    }
}
