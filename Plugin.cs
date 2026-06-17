using System;
using Exiled.API.Features;

namespace WorkstationSnake
{
    /// <summary>
    /// Main plugin entry point for WorkstationSnake.
    /// Registers / unregisters all event listeners on enable / disable.
    /// </summary>
    public class Plugin : Plugin<Config>
    {
        /// <summary>Singleton access — lets other classes reach the plugin if needed.</summary>
        public static Plugin? Instance { get; private set; }

        private EventHandlers? _handlers;

        /// <inheritdoc />
        public override string Name => "WorkstationSnake";

        /// <inheritdoc />
        public override string Author => "Pustkownia";

        /// <inheritdoc />
        public override string Prefix => "workstation_snake";

        /// <inheritdoc />
        public override Version Version => new Version(1, 0, 0);

        /// <inheritdoc />
        public override Version RequiredExiledVersion => new Version(8, 0, 0);

        /// <inheritdoc />
        public override void OnEnabled()
        {
            Instance  = this;
            _handlers = new EventHandlers(Config);

            // Fired when a player presses [I] while looking at a gun workstation
            Exiled.Events.Handlers.Player.ActivatingWorkstation += _handlers.OnActivatingWorkstation;
            Exiled.Events.Handlers.Player.Left                  += _handlers.OnPlayerLeft;
            Exiled.Events.Handlers.Player.Died                  += _handlers.OnPlayerDied;

            base.OnEnabled();
            Log.Info($"{Name} v{Version} enabled — approach a Gun Workstation and press [I] to play Snake!");
        }

        /// <inheritdoc />
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.ActivatingWorkstation -= _handlers!.OnActivatingWorkstation;
            Exiled.Events.Handlers.Player.Left                  -= _handlers.OnPlayerLeft;
            Exiled.Events.Handlers.Player.Died                  -= _handlers.OnPlayerDied;

            _handlers.KillAllSessions();
            _handlers = null;
            Instance  = null;

            base.OnDisabled();
            Log.Info($"{Name} disabled.");
        }
    }
}
