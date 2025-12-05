#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

namespace AOT
{
    /// <summary>
    /// 아무 키나 누르면 현재 씬 리로드. 키보드/마우스/게임패드/터치 다 됨.
    /// 게임오버 화면에서 재시작할 때 씀. 딜레이나 확인창 없음.
    /// </summary>
    public class ReloadAnyKey : MonoBehaviour
    {
        private void Update()
        {
            // Keyboard
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                Reload();
                return;
            }

            // Mouse
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
            {
                Reload();
                return;
            }

            // Gamepad
            if (Gamepad.current != null && Gamepad.current.allControls
                    .Any(control => control is ButtonControl btn && btn.wasPressedThisFrame))
            {
                Reload();
                return;
            }

            if (Touchscreen.current != null &&
                Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Reload();
                return;
            }
        }

        public void Reload()
        {
            SceneManager.LoadScene(gameObject.scene.buildIndex, LoadSceneMode.Single);
        }
    }
}