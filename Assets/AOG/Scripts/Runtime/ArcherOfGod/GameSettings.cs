using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using TMPro;
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
#if UNITY_EDITOR
        public static GameSettings main => s_Main ??= Resources.Load<GameSettings>("GameSettings") ?? CreateInstance<GameSettings>();
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]]
        public static GameSettings main => s_Main;
#endif

        #endregion Signleton


    }
}