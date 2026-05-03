using KerbalInstructionsKit.Tests.TestHelpers;
using KerbalInstructionsKit.Triggers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class TriggerLoaderTests
    {
        [Fact]
        public void Load_ParsesGameStartTrigger()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_Intro")
                .Set("onGameStart", "true");
            var t = TriggerLoader.Load(n);
            Assert.NotNull(t);
            Assert.Equal(TriggerKind.GameStart, t.Kind);
            Assert.Equal("LSN_Intro", t.LessonId);
        }

        [Fact]
        public void Load_ParsesGameEventTrigger()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_Pause")
                .Set("onGameEvent", "onVesselRecovered");
            var t = TriggerLoader.Load(n);
            Assert.NotNull(t);
            Assert.Equal(TriggerKind.GameEvent, t.Kind);
            Assert.Equal("onVesselRecovered", t.EventName);
        }

        [Fact]
        public void Load_ParsesContractTrigger_DefaultsToAccepted()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_Contract")
                .Set("onContract", "FirstOrbit");
            var t = TriggerLoader.Load(n);
            Assert.NotNull(t);
            Assert.Equal(TriggerKind.Contract, t.Kind);
            Assert.Equal("FirstOrbit", t.ContractName);
            Assert.Equal(ContractState.Accepted, t.ContractState);
        }

        [Fact]
        public void Load_ParsesContractTrigger_Offered()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_C")
                .Set("onContract", "MyContract")
                .Set("state", "OFFERED");
            var t = TriggerLoader.Load(n);
            Assert.Equal(ContractState.Offered, t.ContractState);
        }

        [Fact]
        public void Load_ParsesContractTrigger_Completed()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_C")
                .Set("onContract", "MyContract")
                .Set("state", "COMPLETED");
            var t = TriggerLoader.Load(n);
            Assert.Equal(ContractState.Completed, t.ContractState);
        }

        [Fact]
        public void Load_ParsesContractTrigger_Failed()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_C")
                .Set("onContract", "MyContract")
                .Set("state", "FAILED");
            var t = TriggerLoader.Load(n);
            Assert.Equal(ContractState.Failed, t.ContractState);
        }

        [Fact]
        public void Load_ParsesFlagTrigger()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_Flag")
                .Set("onFlag", "advanced_mode");
            var t = TriggerLoader.Load(n);
            Assert.NotNull(t);
            Assert.Equal(TriggerKind.Flag, t.Kind);
            Assert.Equal("advanced_mode", t.FlagName);
        }

        [Fact]
        public void Load_RejectsMissingLesson()
        {
            var n = new FakeSceneNode()
                .Set("onGameStart", "true");
            Assert.Null(TriggerLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsNoCondition()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_X");
            Assert.Null(TriggerLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsNullNode()
        {
            Assert.Null(TriggerLoader.Load(null));
        }

        [Fact]
        public void Load_GameStartFalseDoesNotMatch()
        {
            var n = new FakeSceneNode()
                .Set("lesson", "LSN_X")
                .Set("onGameStart", "false");
            // onGameStart=false doesn't match, falls through to check other conditions
            Assert.Null(TriggerLoader.Load(n));
        }
    }
}
