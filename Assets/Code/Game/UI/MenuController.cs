using UnityEngine;


namespace PQ.Game.UI
{
    // todo: when incorporating this stuff directly in game state machine, get rid of the events,
    //       and instead expose open/close directly
    public class MenuController : MonoBehaviour
    {
        private GameEventCenter _eventCenter;


        void Awake()
        {
            _eventCenter = GameEventCenter.Instance;
        }

        void OnEnable()
        {
            _eventCenter.pauseGame .AddHandler(Open);
            _eventCenter.resumeGame.AddHandler(Close);
        }

        void OnDisable()
        {
            _eventCenter.pauseGame .RemoveHandler(Open);
            _eventCenter.resumeGame.RemoveHandler(Close);
        }


        private void Open()
        {
            // TBD
        }

        private void Close()
        {
            // TBD
        }
    }
}
