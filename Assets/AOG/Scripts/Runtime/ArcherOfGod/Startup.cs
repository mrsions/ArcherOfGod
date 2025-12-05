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