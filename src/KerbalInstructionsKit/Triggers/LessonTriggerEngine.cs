using System;
using System.Collections.Generic;
using KerbalInstructionsKit.Core;

namespace KerbalInstructionsKit.Triggers
{
    public sealed class LessonTriggerEngine
    {
        private readonly LessonState state;
        private readonly List<LessonTrigger> triggers = new List<LessonTrigger>();

        public event Action<string> LessonUnlocked;

        public LessonTriggerEngine(LessonState state)
        {
            this.state = state;
        }

        public void Register(LessonTrigger t)
        {
            if (t != null) triggers.Add(t);
        }

        public IEnumerable<LessonTrigger> All => triggers;

        public void RunGameStartTriggers()
        {
            foreach (var t in triggers)
                if (t.Kind == TriggerKind.GameStart) Fire(t.LessonId);
        }

        public void OnGameEvent(string name)
        {
            foreach (var t in triggers)
                if (t.Kind == TriggerKind.GameEvent && t.EventName == name)
                    Fire(t.LessonId);
        }

        public void OnContractEvent(string contractName, ContractState eventState)
        {
            foreach (var t in triggers)
                if (t.Kind == TriggerKind.Contract &&
                    t.ContractName == contractName &&
                    t.ContractState == eventState)
                    Fire(t.LessonId);
        }

        public void OnFlagSet(string flagName, bool value)
        {
            if (!value) return;
            foreach (var t in triggers)
                if (t.Kind == TriggerKind.Flag && t.FlagName == flagName)
                    Fire(t.LessonId);
        }

        private void Fire(string lessonId)
        {
            if (state.Unlock(lessonId))
                LessonUnlocked?.Invoke(lessonId);
        }
    }
}
