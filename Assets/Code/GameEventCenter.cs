using PQ.Common.Events;


namespace PQ
{
    // todo: find a better place for this
    public struct HorizontalInput
    {
        public enum Type { None, Left, Right }
        public readonly Type value;
        public HorizontalInput(Type value)
        {
            this.value = value;
        }
    }

    // todo: replace this with GameEventRegistries for UI, Commands, etc

    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public class GameEventCenter
    {
        // note that by default we have a dummy listener as we always want the events to fire
        // also note that since we omit the 'event' keyword, we allow invocation anywhere

        public PqEvent<HorizontalInput>       movementInputChange = new("command.movement.changed");
        public PqEvent                        jumpCommand         = new("command.jump");
        public PqEvent                        lieDownCommand      = new("command.liedown");
        public PqEvent                        standUpCommand      = new("command.standup");
        public PqEvent                        useCommand          = new("command.use");
        public PqEvent                        fireCommand         = new("command.fire");

        public PqEvent<PlayerProgressionInfo> scoreChange         = new("score.changed");
        public PqEvent<PlayerSettingsInfo>    startNewGame        = new("game.new");
        public PqEvent<PlayerProgressionInfo> pauseGame           = new("game.pause");
        public PqEvent<PlayerProgressionInfo> gameOver            = new("game.over");
        public PqEvent                        resumeGame          = new("game.resume");
        public PqEvent                        gotoMainMenu        = new("game.home");
        public PqEvent                        restartGame         = new("game.restart");

        private static GameEventCenter _instance;
        public static GameEventCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameEventCenter();
                }
                return _instance;
            }
        }
    }
}
