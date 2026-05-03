using System;

namespace KerbalInstructionsKit.Util
{
    public static class KikLog
    {
        internal static Action<string> WarnHandler = DefaultWarn;
        internal static Action<string> ErrorHandler = DefaultError;

        public static void Warn(string msg) => WarnHandler(msg);
        public static void Error(string msg) => ErrorHandler(msg);

        private static void DefaultWarn(string msg)
        {
            try { UnityEngine.Debug.LogWarning(msg); }
            catch { }
        }

        private static void DefaultError(string msg)
        {
            try { UnityEngine.Debug.LogError(msg); }
            catch { }
        }
    }
}
