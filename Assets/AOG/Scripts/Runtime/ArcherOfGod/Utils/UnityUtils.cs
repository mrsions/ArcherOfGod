#nullable enable

namespace AOT
{
    /// <summary>
    /// 유니티 유틸. GetPath로 씬 경로 포함한 전체 하이어라키 경로 구함.
    /// 디버깅용. 근데 버그 있음 - while문에서 p.parent 대신 t.parent 씀.
    /// </summary>
    public static class UnityUtils
    {
        //public static string GetPath(Transform t)
        //{
        //    string path = t.name;
        //    Transform p = t.parent;
        //    for(int i=0; i<100 && p!=null; i++)
        //    {
        //        path = p.name + "/" + path;
        //        p = p.parent;
        //    }

        //    return t.gameObject.scene.path + "@" + path;
        //}
    }
}