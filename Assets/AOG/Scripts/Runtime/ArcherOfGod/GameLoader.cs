using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AOT
{
    /// <summary>
    /// Awake에서 assets 배열에 있는 프리팹들 다 Instantiate 해줌.
    /// 루트에 생성되고 풀링 없이 동기로 다 로드함. 생성 후 관리는 안함.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        public GameObject[] assets;

        private void Awake()
        {
            for(int i=0; i<assets.Length; i++)
            {
                Instantiate(assets[i]);
            }
        }
    }
}