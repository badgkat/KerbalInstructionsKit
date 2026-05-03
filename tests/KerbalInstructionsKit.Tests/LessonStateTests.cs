using System.Linq;
using KerbalInstructionsKit.Core;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class LessonStateTests
    {
        [Fact]
        public void Unlock_ReturnsTrueOnFirstUnlock()
        {
            var state = new LessonState();
            Assert.True(state.Unlock("LSN_A"));
        }

        [Fact]
        public void Unlock_ReturnsFalseOnDuplicate()
        {
            var state = new LessonState();
            state.Unlock("LSN_A");
            Assert.False(state.Unlock("LSN_A"));
        }

        [Fact]
        public void Unlock_RejectsNull()
        {
            var state = new LessonState();
            Assert.False(state.Unlock(null));
        }

        [Fact]
        public void Unlock_RejectsEmpty()
        {
            var state = new LessonState();
            Assert.False(state.Unlock(""));
        }

        [Fact]
        public void IsUnlocked_ReturnsTrueAfterUnlock()
        {
            var state = new LessonState();
            state.Unlock("LSN_B");
            Assert.True(state.IsUnlocked("LSN_B"));
        }

        [Fact]
        public void IsUnlocked_ReturnsFalseIfNotUnlocked()
        {
            var state = new LessonState();
            Assert.False(state.IsUnlocked("LSN_X"));
        }

        [Fact]
        public void IsUnlocked_ReturnsFalseForNull()
        {
            var state = new LessonState();
            Assert.False(state.IsUnlocked(null));
        }

        [Fact]
        public void AllUnlocked_ReturnsAllIds()
        {
            var state = new LessonState();
            state.Unlock("A");
            state.Unlock("B");
            state.Unlock("C");
            var all = state.AllUnlocked.ToList();
            Assert.Equal(3, all.Count);
            Assert.Contains("A", all);
            Assert.Contains("B", all);
            Assert.Contains("C", all);
        }

        [Fact]
        public void SetFlag_GetFlag_Works()
        {
            var state = new LessonState();
            state.SetFlag("myFlag", true);
            Assert.True(state.GetFlag("myFlag"));
        }

        [Fact]
        public void GetFlag_ReturnsFalseForUnset()
        {
            var state = new LessonState();
            Assert.False(state.GetFlag("missing"));
        }

        [Fact]
        public void GetFlag_ReturnsFalseForNull()
        {
            var state = new LessonState();
            Assert.False(state.GetFlag(null));
        }

        [Fact]
        public void SetFlag_CanSetToFalse()
        {
            var state = new LessonState();
            state.SetFlag("f", true);
            state.SetFlag("f", false);
            Assert.False(state.GetFlag("f"));
        }

        [Fact]
        public void SetFlag_IgnoresNullName()
        {
            var state = new LessonState();
            state.SetFlag(null, true);
            // Should not throw
        }

        [Fact]
        public void SetFlag_IgnoresEmptyName()
        {
            var state = new LessonState();
            state.SetFlag("", true);
            Assert.False(state.GetFlag(""));
        }

        [Fact]
        public void LastViewedLesson_DefaultsToNull()
        {
            var state = new LessonState();
            Assert.Null(state.LastViewedLesson);
            Assert.Equal(0, state.LastViewedPage);
        }
    }
}
