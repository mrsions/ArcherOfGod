#nullable enable

using TMPro;
using UnityEngine;

namespace AOT
{
    /// <summary>
    /// TMP_Text 래퍼. SetText로 외부에서 텍스트 설정 가능.
    /// 데미지 숫자 같은거 풀에서 꺼내서 텍스트 넣을 때 씀.
    /// </summary>
    public class UITextDelegate : MonoBehaviour
    {
        [SerializeField] private TMP_Text m_Text;

        public string text
        {
            get => m_Text.text;
            set => m_Text.text = value;
        }

        public void SetText(string msg)
        {
            m_Text.text = msg;
        }
    }
}