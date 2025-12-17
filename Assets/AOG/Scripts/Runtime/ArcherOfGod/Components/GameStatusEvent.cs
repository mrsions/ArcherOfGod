using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AOT
{
    /// <summary>
    /// 게임 상태가 eventStatus랑 같아지면 onEvent 발동시킴.
    /// 인스펙터에서 UnityEvent 연결해두면 됨. 진입할 때만 발동하고 나갈때는 안함.
    /// RequireComponent(Button) 붙어있는데 안 쓰는듯?
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GameStatusEvent : MonoBehaviour
    {
        //-- Serializable
        public EGameStatus eventStatus;
        public UnityEvent onEvent;

        //-- Private

        //------------------------------------------------------------------------------

        private void Awake()
        {
            print($"[GameStatusEvent] RegistOnChangedStatus");
            GameManager.main.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(GameManager.main, GameManager.main.Status);
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            if (status == eventStatus)
            {
                onEvent?.Invoke();
            }
        }
    }
}