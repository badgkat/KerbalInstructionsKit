using KerbalInstructionsKit.Util;

namespace KerbalInstructionsKit.Triggers
{
    public static class TriggerLoader
    {
        public static LessonTrigger Load(ISceneNode node)
        {
            if (node == null) return null;
            var lessonId = node.GetValue("lesson");
            if (string.IsNullOrEmpty(lessonId))
            {
                KikLog.Warn("[KIK] LESSON_TRIGGER missing 'lesson', skipping");
                return null;
            }

            var t = new LessonTrigger { LessonId = lessonId };

            if (node.HasValue("onGameStart") &&
                bool.TryParse(node.GetValue("onGameStart"), out var b) && b)
            {
                t.Kind = TriggerKind.GameStart;
                return t;
            }

            var ev = node.GetValue("onGameEvent");
            if (!string.IsNullOrEmpty(ev))
            {
                t.Kind = TriggerKind.GameEvent;
                t.EventName = ev;
                return t;
            }

            var contract = node.GetValue("onContract");
            if (!string.IsNullOrEmpty(contract))
            {
                t.Kind = TriggerKind.Contract;
                t.ContractName = contract;
                t.ContractState = ParseContractState(node.GetValue("state"));
                return t;
            }

            var flag = node.GetValue("onFlag");
            if (!string.IsNullOrEmpty(flag))
            {
                t.Kind = TriggerKind.Flag;
                t.FlagName = flag;
                return t;
            }

            KikLog.Warn($"[KIK] LESSON_TRIGGER for '{lessonId}' has no recognized trigger condition, skipping");
            return null;
        }

        private static ContractState ParseContractState(string s)
        {
            if (string.IsNullOrEmpty(s)) return ContractState.Accepted;
            switch (s.ToUpperInvariant())
            {
                case "OFFERED":   return ContractState.Offered;
                case "COMPLETED": return ContractState.Completed;
                case "FAILED":    return ContractState.Failed;
                default:          return ContractState.Accepted;
            }
        }
    }
}
