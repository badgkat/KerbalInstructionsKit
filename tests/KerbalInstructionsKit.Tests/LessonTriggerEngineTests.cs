using System.Collections.Generic;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Triggers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class LessonTriggerEngineTests
    {
        private LessonState state;
        private LessonTriggerEngine engine;
        private List<string> unlocked;

        public LessonTriggerEngineTests()
        {
            state = new LessonState();
            engine = new LessonTriggerEngine(state);
            unlocked = new List<string>();
            engine.LessonUnlocked += id => unlocked.Add(id);
        }

        [Fact]
        public void RunGameStartTriggers_UnlocksGameStartLessons()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameStart, LessonId = "LSN_A" });
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameStart, LessonId = "LSN_B" });
            engine.RunGameStartTriggers();
            Assert.Contains("LSN_A", unlocked);
            Assert.Contains("LSN_B", unlocked);
            Assert.True(state.IsUnlocked("LSN_A"));
            Assert.True(state.IsUnlocked("LSN_B"));
        }

        [Fact]
        public void RunGameStartTriggers_DoesNotFireNonGameStart()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameEvent, LessonId = "LSN_X", EventName = "onSomething" });
            engine.RunGameStartTriggers();
            Assert.Empty(unlocked);
        }

        [Fact]
        public void OnGameEvent_FiresMatchingTrigger()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameEvent, LessonId = "LSN_E", EventName = "onVesselRecovered" });
            engine.OnGameEvent("onVesselRecovered");
            Assert.Contains("LSN_E", unlocked);
        }

        [Fact]
        public void OnGameEvent_DoesNotFireNonMatching()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameEvent, LessonId = "LSN_E", EventName = "onVesselRecovered" });
            engine.OnGameEvent("onSomethingElse");
            Assert.Empty(unlocked);
        }

        [Fact]
        public void OnContractEvent_FiresMatchingTrigger()
        {
            engine.Register(new LessonTrigger
            {
                Kind = TriggerKind.Contract,
                LessonId = "LSN_C",
                ContractName = "FirstOrbit",
                ContractState = ContractState.Accepted
            });
            engine.OnContractEvent("FirstOrbit", ContractState.Accepted);
            Assert.Contains("LSN_C", unlocked);
        }

        [Fact]
        public void OnContractEvent_DoesNotFireWrongState()
        {
            engine.Register(new LessonTrigger
            {
                Kind = TriggerKind.Contract,
                LessonId = "LSN_C",
                ContractName = "FirstOrbit",
                ContractState = ContractState.Completed
            });
            engine.OnContractEvent("FirstOrbit", ContractState.Accepted);
            Assert.Empty(unlocked);
        }

        [Fact]
        public void OnContractEvent_DoesNotFireWrongContract()
        {
            engine.Register(new LessonTrigger
            {
                Kind = TriggerKind.Contract,
                LessonId = "LSN_C",
                ContractName = "FirstOrbit",
                ContractState = ContractState.Accepted
            });
            engine.OnContractEvent("OtherContract", ContractState.Accepted);
            Assert.Empty(unlocked);
        }

        [Fact]
        public void OnFlagSet_FiresMatchingTrigger()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.Flag, LessonId = "LSN_F", FlagName = "advanced" });
            engine.OnFlagSet("advanced", true);
            Assert.Contains("LSN_F", unlocked);
        }

        [Fact]
        public void OnFlagSet_DoesNotFireOnFalse()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.Flag, LessonId = "LSN_F", FlagName = "advanced" });
            engine.OnFlagSet("advanced", false);
            Assert.Empty(unlocked);
        }

        [Fact]
        public void OnFlagSet_DoesNotFireWrongFlag()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.Flag, LessonId = "LSN_F", FlagName = "advanced" });
            engine.OnFlagSet("other", true);
            Assert.Empty(unlocked);
        }

        [Fact]
        public void Idempotent_UnlockOnlyFiresOnce()
        {
            engine.Register(new LessonTrigger { Kind = TriggerKind.GameStart, LessonId = "LSN_Once" });
            engine.RunGameStartTriggers();
            engine.RunGameStartTriggers();
            Assert.Single(unlocked);
        }

        [Fact]
        public void Register_IgnoresNull()
        {
            engine.Register(null);
            engine.RunGameStartTriggers();
            Assert.Empty(unlocked);
        }
    }
}
