#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace AOT
{
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