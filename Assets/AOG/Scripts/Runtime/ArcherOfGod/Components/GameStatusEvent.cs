using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AOT
{
    /// <summary>
    /// 게임 상태가 eventStatus랑 같아지면 onEvent 발동시킴.
    /// 인스펙터에서 UnityEvent 연결해두면 됨. 진입할 때만 발동하고 나갈때는 안함.
    /// </summary>
    public class GameStatusEvent : MonoBehaviour
    {
        //-- Serializable
        [SerializeField]
        [FormerlySerializedAs("eventStatus")]
        private EGameStatus m_EventStatus;
        [SerializeField]
        [FormerlySerializedAs("onEvent")]
        private UnityEvent m_OnEvent;

        //-- Private
        private GameManager m_GameManager;

        //------------------------------------------------------------------------------

        private void Awake()
        {
            Debug.Log($"[GameStatusEvent] RegistOnChangedStatus");
            m_GameManager = GameManager.main;
            m_GameManager.OnChangedStatus += OnChangedStatus;
            OnChangedStatus(m_GameManager, m_GameManager.Status);
        }

        private void OnDestroy()
        {
            if (m_GameManager != null)
                m_GameManager.OnChangedStatus -= OnChangedStatus;
        }

        private void OnChangedStatus(GameManager manager, EGameStatus status)
        {
            if (status == m_EventStatus)
            {
                m_OnEvent?.Invoke();
            }
        }
    }
}