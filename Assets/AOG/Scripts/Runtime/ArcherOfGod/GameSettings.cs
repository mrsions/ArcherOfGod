using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AOT
{
    public class GameSettings : ScriptableObject
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


        public float skill_delay_onAwake = 1f;
    }
}