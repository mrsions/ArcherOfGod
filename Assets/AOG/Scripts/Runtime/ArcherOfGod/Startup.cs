using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace AOT
{
    /// <summary>
    /// 게임 시작점. GameSettings 초기화하고 로딩 화면 띄우면서 게임 씬(인덱스 1)으로 넘어감.
    /// Scene 0에서만 동작하고 씬 전환되면 같이 날아감. 에러 처리 따로 없음.
    /// </summary>
    public class Startup : MonoBehaviour
    {
        public GameSettings gameSettings;
        public TMP_Text m_Text;

        private void Start()
        {
            LoadAsync().Forget();
        }

        private async UniTask LoadAsync()
        {
            gameSettings.SetMain();

            await UniTask.WaitForSeconds(1);

            var op = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
            op.allowSceneActivation = true;

            while (this && op.isDone)
            {
                m_Text.text = $"{op.progress * 100:f0}%";
                await UniTask.Yield();
            }
        }
    }
}