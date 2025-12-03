using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
        public PlayerBehaviour player;
        public PlayerBehaviour enemy;

        //-- Events
        public event Action<GameManager, EGameStatus> OnChangedStatus;

        //-- Private 
        private EGameStatus m_Status;


        //-- Properties
        public PlayerBehaviour[] Players { get; private set; }
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
            Players = new PlayerBehaviour[] { player, enemy };

            StartProcessAsync().Forget();
        }

        private async UniTaskVoid StartProcessAsync()
        {
            try
            {
                Status = EGameStatus.Loading;

                //TODO : Sync Data

                //TODO : Sync Ping

                Status = EGameStatus.Ready;

                //TODO : Ready Animation
                await UniTask.Delay(1000);

                Status = EGameStatus.Start;
                //TODO : something settings.

                Status = EGameStatus.Battle;

                // wait for game end
                await UniTask.Delay(90_000);

                Status = EGameStatus.Finish;

                //TODO : Finish Animation
                //TODO : Send game result to server
                await UniTask.Delay(1000);

                Status = EGameStatus.End;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }


    }
}