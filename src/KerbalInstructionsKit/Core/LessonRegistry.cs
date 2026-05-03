using System.Collections.Generic;

namespace KerbalInstructionsKit.Core
{
    public sealed class LessonRegistry
    {
        private readonly Dictionary<string, Lesson> lessons = new Dictionary<string, Lesson>();

        public bool Register(Lesson l)
        {
            if (l == null || string.IsNullOrEmpty(l.Id)) return false;
            if (lessons.ContainsKey(l.Id)) return false;
            lessons[l.Id] = l;
            return true;
        }

        public bool Contains(string id) => id != null && lessons.ContainsKey(id);

        public Lesson Get(string id)
        {
            if (id == null) return null;
            lessons.TryGetValue(id, out var l);
            return l;
        }

        public IEnumerable<Lesson> All => lessons.Values;
    }
}
