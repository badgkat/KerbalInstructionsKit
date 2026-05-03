using System.Collections.Generic;
using System.Globalization;

namespace KerbalInstructionsKit.Core
{
    public sealed class LessonState
    {
        private readonly HashSet<string> unlocked = new HashSet<string>();
        private readonly Dictionary<string, bool> flags = new Dictionary<string, bool>();

        public string LastViewedLesson;
        public int LastViewedPage;

        public bool IsUnlocked(string id) => id != null && unlocked.Contains(id);

        public bool Unlock(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return unlocked.Add(id);
        }

        public IEnumerable<string> AllUnlocked => unlocked;

        public bool GetFlag(string name) => name != null && flags.TryGetValue(name, out var v) && v;

        public void SetFlag(string name, bool value)
        {
            if (string.IsNullOrEmpty(name)) return;
            flags[name] = value;
        }

        public void Save(ConfigNode parent)
        {
            var unlockedNode = parent.AddNode("UNLOCKED");
            foreach (var id in unlocked) unlockedNode.AddValue(id, "true");

            var flagsNode = parent.AddNode("FLAGS");
            foreach (var kv in flags)
                flagsNode.AddValue(kv.Key, kv.Value.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrEmpty(LastViewedLesson))
            {
                parent.AddValue("LAST_VIEWED", LastViewedLesson);
                parent.AddValue("LAST_VIEWED_PAGE", LastViewedPage.ToString(CultureInfo.InvariantCulture));
            }
        }

        public void Load(ConfigNode parent)
        {
            unlocked.Clear();
            flags.Clear();
            LastViewedLesson = null;
            LastViewedPage = 0;

            if (parent.HasNode("UNLOCKED"))
            {
                foreach (ConfigNode.Value v in parent.GetNode("UNLOCKED").values)
                {
                    if (bool.TryParse(v.value, out var b) && b)
                        unlocked.Add(v.name);
                }
            }
            if (parent.HasNode("FLAGS"))
            {
                foreach (ConfigNode.Value v in parent.GetNode("FLAGS").values)
                {
                    if (bool.TryParse(v.value, out var b))
                        flags[v.name] = b;
                }
            }
            if (parent.HasValue("LAST_VIEWED"))
                LastViewedLesson = parent.GetValue("LAST_VIEWED");
            if (parent.HasValue("LAST_VIEWED_PAGE") &&
                int.TryParse(parent.GetValue("LAST_VIEWED_PAGE"),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out var p))
                LastViewedPage = p;
        }
    }
}
