using System;
using KerbalInstructionsKit.Util.Expression;

namespace KerbalInstructionsKit.Core
{
    public static class InstructionsKit
    {
        public static IKikRuntime Runtime { get; internal set; }

        public static LessonRegistry Lessons => Runtime?.Lessons;
        public static LessonState State => Runtime?.State;
        public static IExpressionContext ExpressionContext => Runtime?.ExpressionContext;

        public static event Action<string> LessonUnlocked
        {
            add    { if (Runtime != null) Runtime.LessonUnlocked += value; }
            remove { if (Runtime != null) Runtime.LessonUnlocked -= value; }
        }

        public static void OpenLesson(string lessonId) => Runtime?.OpenLesson(lessonId);
        public static void OpenArchive() => Runtime?.OpenArchive();
        public static void Close() => Runtime?.Close();

        public static void SetFlag(string name, bool value) => Runtime?.SetFlag(name, value);
        public static bool GetFlag(string name) => Runtime?.GetFlag(name) ?? false;
    }

    public interface IKikRuntime
    {
        LessonRegistry Lessons { get; }
        LessonState State { get; }
        IExpressionContext ExpressionContext { get; }
        event Action<string> LessonUnlocked;

        void OpenLesson(string lessonId);
        void OpenArchive();
        void Close();
        void SetFlag(string name, bool value);
        bool GetFlag(string name);
    }
}
