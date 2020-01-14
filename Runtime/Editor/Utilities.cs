#if UNITY_EDITOR

using System.IO;

namespace UnityExtensions.Steamworks.Editor
{
    public struct Utilities
    {
        public static readonly string RuntimeDirectory = Path.GetFullPath("Packages/com.yuyang.unity-extensions.steamworks/Runtime");
    }
}

#endif // UNITY_EDITOR