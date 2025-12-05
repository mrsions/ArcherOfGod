using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AOT
{
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