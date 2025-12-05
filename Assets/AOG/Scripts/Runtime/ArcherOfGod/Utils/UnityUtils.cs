#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace AOT
{
    public static class UnityUtils
    {
        public static string GetPath(Transform t)
        {
            string path = t.name;
            Transform p = t.parent;
            while(p != null)
            {
                path = p.name + "/" + path;
                p = t.parent;
            }

            return t.gameObject.scene.path + "@" + path;
        }
    }
}