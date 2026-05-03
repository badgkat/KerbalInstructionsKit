using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Triggers;
using KerbalInstructionsKit.Util;
using UnityEngine;

namespace KerbalInstructionsKit.Config
{
    public static class LessonContentLoader
    {
        public static LessonRegistry LoadAll()
        {
            var reg = new LessonRegistry();
            int loaded = 0, skipped = 0;
            foreach (var url in GameDatabase.Instance.GetConfigs("INSTRUCTION_LESSON"))
            {
                var lesson = LessonLoader.Load(new ConfigNodeAdapter(url.config));
                if (lesson != null && reg.Register(lesson)) loaded++;
                else skipped++;
            }
            Debug.Log($"[KIK] loaded {loaded} lessons (skipped {skipped})");
            return reg;
        }

        public static void LoadTriggers(LessonTriggerEngine engine)
        {
            int loaded = 0;
            foreach (var url in GameDatabase.Instance.GetConfigs("LESSON_TRIGGER"))
            {
                var t = TriggerLoader.Load(new ConfigNodeAdapter(url.config));
                if (t != null) { engine.Register(t); loaded++; }
            }
            Debug.Log($"[KIK] loaded {loaded} standalone LESSON_TRIGGER blocks");
        }
    }
}
