using PQ.Common.Events;
using PQ.Game.Entities;
using PQ.Game.Sound;


// todo: replace this with GameEventRegistries for UI, Commands, etc, and move HorizontalInput out of here
namespace PQ.Game
{
    public struct HorizontalInput
    {
        public enum Type { None, Left, Right }
        public readonly Type value;
        public HorizontalInput(Type value)
        {
            this.value = value;
        }
    }

    /*
    All game events are held here (as static data, so it can be accessed across scenes).

    Note that events are triggered and handled ENTIRELY programmatically (via listeners and invocations).
    */
    public class GameEventCenter
    {
        public readonly PqEvent startGame      = new("game.restart");
        public readonly PqEvent pauseGame      = new("game.pause");
        public readonly PqEvent resumeGame     = new("game.resume");
        public readonly PqEvent levelLost      = new("game.level.lost");
        public readonly PqEvent levelWon       = new("game.level.won");
        public readonly PqEvent endGame        = new("game.finished");

        public readonly PqEvent jumpCommand    = new("command.jump");
        public readonly PqEvent lieDownCommand = new("command.liedown");
        public readonly PqEvent standUpCommand = new("command.standup");
        public readonly PqEvent useCommand     = new("command.use");
        public readonly PqEvent fireCommand    = new("command.fire");

        public readonly PqEvent<HorizontalInput> movementInputChange    = new("command.movement.changed");
        public readonly PqEvent<CharacterStatus> characterStatusChanged = new("character.status.changed");


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
