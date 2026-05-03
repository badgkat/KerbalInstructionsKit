using System.Linq;
using KerbalInstructionsKit.Core;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class LessonRegistryTests
    {
        private static Lesson MakeLesson(string id) => new Lesson
        {
            Id = id,
            Title = id,
            Pages = { new Page { Text = "content" } }
        };

        [Fact]
        public void Register_AddsLesson()
        {
            var reg = new LessonRegistry();
            Assert.True(reg.Register(MakeLesson("A")));
            Assert.True(reg.Contains("A"));
        }

        [Fact]
        public void Register_RejectsDuplicate()
        {
            var reg = new LessonRegistry();
            reg.Register(MakeLesson("A"));
            Assert.False(reg.Register(MakeLesson("A")));
        }

        [Fact]
        public void Register_RejectsNull()
        {
            var reg = new LessonRegistry();
            Assert.False(reg.Register(null));
        }

        [Fact]
        public void Register_RejectsNullId()
        {
            var reg = new LessonRegistry();
            Assert.False(reg.Register(new Lesson { Id = null }));
        }

        [Fact]
        public void Register_RejectsEmptyId()
        {
            var reg = new LessonRegistry();
            Assert.False(reg.Register(new Lesson { Id = "" }));
        }

        [Fact]
        public void Get_ReturnsLesson()
        {
            var reg = new LessonRegistry();
            var lesson = MakeLesson("B");
            reg.Register(lesson);
            Assert.Same(lesson, reg.Get("B"));
        }

        [Fact]
        public void Get_ReturnsNullForMissing()
        {
            var reg = new LessonRegistry();
            Assert.Null(reg.Get("missing"));
        }

        [Fact]
        public void Get_ReturnsNullForNull()
        {
            var reg = new LessonRegistry();
            Assert.Null(reg.Get(null));
        }

        [Fact]
        public void Contains_ReturnsFalseForNull()
        {
            var reg = new LessonRegistry();
            Assert.False(reg.Contains(null));
        }

        [Fact]
        public void All_ReturnsAllRegistered()
        {
            var reg = new LessonRegistry();
            reg.Register(MakeLesson("X"));
            reg.Register(MakeLesson("Y"));
            reg.Register(MakeLesson("Z"));
            Assert.Equal(3, reg.All.Count());
        }
    }
}
