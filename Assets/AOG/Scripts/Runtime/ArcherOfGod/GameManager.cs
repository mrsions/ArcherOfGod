using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AOT
{
    public enum EGameStatus
    {
        None = 0,
        Loading,
        Ready,
        Start,
        Battle,
        Battle_LimitOver,
        Finish,
        End,
        MAX
    }


    public class GameManager : MonoBehaviour
    {
        private static GameManager s_Main;

        public static GameManager main => s_Main ??= Resources.FindObjectsOfTypeAll<GameManager>().FirstOrDefault();

        //-- Serializable
        [FormerlySerializedAs("player")]
        public CharacterBehaviour m_Player;
        [FormerlySerializedAs("enemy")]
        public CharacterBehaviour m_Enemy;

        //-- Events
        public event Action<GameManager, EGameStatus> OnChangedStatus;

        //-- Private 
        private EGameStatus m_Status;


        //-- Properties
        public CharacterBehaviour[] Players { get; private set; }
        public EGameStatus Status
        {
            get => m_Status;
            private set
            {
                if (m_Status == value) return;
                m_Status = value;
                OnChangedStatus?.Invoke(this, m_Status);
            }
        }


        //------------------------------------------------------------------------------

        private void Awake()
        {
            s_Main = this;
        }

        private void Start()
        {
            Players = new CharacterBehaviour[] { m_Player, m_Enemy };

            StartProcessAsync().Forget();
        }

        private async UniTask StartProcessAsync()
        {
            try
            {
                Status = EGameStatus.Loading;

                //TODO : Sync Data

                //TODO : Sync Ping

                Status = EGameStatus.Ready;

                //TODO : Ready Animation
                await UniTask.WaitForSeconds(3);

                Status = EGameStatus.Start;
                //TODO : something settings.

                Status = EGameStatus.Battle;

                // wait for game end
                await UniTask.WaitForSeconds(90);

                Status = EGameStatus.Finish;

                //TODO : Finish Animation
                //TODO : Send game result to server
                await UniTask.WaitForSeconds(1);

                Status = EGameStatus.End;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public CharacterBehaviour GetCharacter(bool m_IsPlayer)
        {
            return m_IsPlayer ? m_Player : m_Enemy;
        }
    }
}