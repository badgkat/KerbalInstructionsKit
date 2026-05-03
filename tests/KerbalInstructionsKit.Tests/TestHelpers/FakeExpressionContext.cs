using System.Collections.Generic;
using KerbalInstructionsKit.Util.Expression;

namespace KerbalInstructionsKit.Tests.TestHelpers
{
    public sealed class FakeExpressionContext : IExpressionContext
    {
        private readonly HashSet<string> unlockedLessons = new HashSet<string>();
        private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

        public FakeExpressionContext UnlockLesson(string id)
        {
            unlockedLessons.Add(id);
            return this;
        }

        public FakeExpressionContext SetFlag(string name, bool value)
        {
            flags[name] = value;
            return this;
        }

        public bool IsLessonUnlocked(string id) => unlockedLessons.Contains(id);
        public bool GetFlag(string name) => flags.TryGetValue(name, out var v) && v;
    }
}
