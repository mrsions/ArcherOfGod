using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AOT
{
    [Serializable]
    public struct FInputActionReference
    {
        public string actionId;
        [NonSerialized]
        private InputAction action;

        public InputAction Get(InputActionAsset asset)
        {
            return action ??= asset.FindAction(actionId, true);
        }
    }

    public partial class GameSettings
    {
        [Header("Options")]
        public float skill_delay_onAwake = 1f;
        public float move_input_delay = 0.1f;
        public int gameTime = 90;

        [Header("Prefabs")]
        public UITextDelegate damagePrefab;
        public UITextDelegate criticalPrefab;

        //-- InputActions
        [Header("InputAction")]
        public InputActionAsset playerInputAsset;
        [SerializeField] private FInputActionReference playerMoveActionId;
        [SerializeField] private FInputActionReference[] playerSkillActionId;
        public InputAction GetPlayerMoveAction() => playerMoveActionId.Get(playerInputAsset);
        public InputAction GetPlayerSkillAction(int id) => playerSkillActionId[id].Get(playerInputAsset);

    }


    /// <summary>
    /// 게임 설정 담는 ScriptableObject. Resources/GameSettings에서 로드됨.
    /// 스킬 딜레이, 이동 딜레이, 게임시간 같은 옵션이랑 데미지 텍스트 프리팹 참조 들어있음.
    /// Input System 액션 매핑도 여기서 관리함. 플레이어별 설정은 안되고 전역 하나만 씀.
    /// </summary>
    public partial class GameSettings : ScriptableObject
    {
        #region Editor
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/AOT/Create Game Setting")]
        public static void CreateGameSettingAsset()
        {
            System.IO.Directory.CreateDirectory("Assets/AOG/Resources");
            UnityEditor.AssetDatabase.CreateAsset(CreateInstance<GameSettings>(), "Assets/AOG/Resources/GameSettings.asset");
        }
#endif
        #endregion Editor

        #region Singleton
        private static GameSettings s_Main;

        [RuntimeInitializeOnLoadMethod]
        public static void LoadSettings()
        {
            s_Main = Resources.Load<GameSettings>("GameSettings");
        }

        // speed hack for runtime
#if UNITY_EDITOR || NOPT
        public static GameSettings main => s_Main ??= Resources.Load<GameSettings>("GameSettings") ?? CreateInstance<GameSettings>();
#else
        public static GameSettings main
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => s_Main;
        }
#endif

        public void SetMain()
        {
            s_Main = this;
        }
        #endregion Signleton


    }
}