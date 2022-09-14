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

        public PqEvent<HorizontalInput>       movementInputChanged = delegate { };
        public PqEvent                        jumpCommand          = delegate { };
        public PqEvent                        lieDownCommand       = delegate { };
        public PqEvent                        standUpCommand       = delegate { };
        public PqEvent                        useCommand           = delegate { };
        public PqEvent                        fireCommand          = delegate { };

        public PqEvent<PlayerProgressionInfo> scoreChange          = delegate { };
        public PqEvent<PlayerSettingsInfo>    startNewGame         = delegate { };
        public PqEvent<PlayerProgressionInfo> pauseGame            = delegate { };
        public PqEvent<PlayerProgressionInfo> gameOver             = delegate { };
        public PqEvent                        resumeGame           = delegate { };
        public PqEvent                        gotoMainMenu         = delegate { };
        public PqEvent                        restartGame          = delegate { };

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
