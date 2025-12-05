using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// 플레이어 입력 처리. Input System에서 이동 액션 읽어서 캐릭터한테 넘겨줌.
    /// UniTask로 매 프레임 폴링하고 PreUpdate에서 InputAxis 갱신함.
    /// 스킬 입력은 UISkillButton이 함. 캐릭터 죽으면 폴링 멈춤.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        //-- Serializable
        [SerializeField] private CharacterBehaviour m_Cha;

        //-- Private

        //-- Properties

        //------------------------------------------------------------------------------

        private void Start()
        {
            RunAsync().Forget();
        }

        private async UniTask RunAsync()
        {
            while(m_Cha.IsLive)
            {
                m_Cha.InputAxis = GameSettings.main.GetPlayerMoveAction().ReadValue<Vector2>();

                await UniTask.Yield(PlayerLoopTiming.PreUpdate, destroyCancellationToken);
            }
        }
    }
}