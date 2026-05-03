using KerbalInstructionsKit.Config;
using KerbalInstructionsKit.Tests.TestHelpers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class LessonLoaderTests
    {
        private FakeSceneNode MakeValidLesson(string id = "LSN_Test")
        {
            var page = new FakeSceneNode().Set("text", "Hello world");
            return new FakeSceneNode()
                .Set("id", id)
                .Set("title", "Test Lesson")
                .Set("category", "Basics")
                .AddNode("PAGE", page);
        }

        [Fact]
        public void Load_ParsesValidLesson()
        {
            var n = MakeValidLesson();
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal("LSN_Test", lesson.Id);
            Assert.Equal("Test Lesson", lesson.Title);
            Assert.Equal("Basics", lesson.Category);
            Assert.Single(lesson.Pages);
        }

        [Fact]
        public void Load_RejectsMissingId()
        {
            var page = new FakeSceneNode().Set("text", "Hello");
            var n = new FakeSceneNode()
                .Set("title", "No Id")
                .AddNode("PAGE", page);
            Assert.Null(LessonLoader.Load(n));
        }

        [Fact]
        public void Load_RejectsNoPages()
        {
            var n = new FakeSceneNode()
                .Set("id", "LSN_Empty")
                .Set("title", "Empty");
            Assert.Null(LessonLoader.Load(n));
        }

        [Fact]
        public void Load_UsesTitleFallback()
        {
            var page = new FakeSceneNode().Set("text", "Content");
            var n = new FakeSceneNode()
                .Set("id", "LSN_NoTitle")
                .AddNode("PAGE", page);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal("LSN_NoTitle", lesson.Title);
        }

        [Fact]
        public void Load_ParsesTags()
        {
            var page = new FakeSceneNode().Set("text", "Content");
            var n = new FakeSceneNode()
                .Set("id", "LSN_Tags")
                .Set("tags", "basics, rockets, tutorial")
                .AddNode("PAGE", page);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal(3, lesson.Tags.Count);
            Assert.Contains("basics", lesson.Tags);
            Assert.Contains("rockets", lesson.Tags);
            Assert.Contains("tutorial", lesson.Tags);
        }

        [Fact]
        public void Load_ParsesSortOrder()
        {
            var page = new FakeSceneNode().Set("text", "Content");
            var n = new FakeSceneNode()
                .Set("id", "LSN_Sort")
                .Set("sortOrder", "42")
                .AddNode("PAGE", page);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal(42, lesson.SortOrder);
        }

        [Fact]
        public void Load_DefaultSortOrderIsZero()
        {
            var n = MakeValidLesson();
            var lesson = LessonLoader.Load(n);
            Assert.Equal(0, lesson.SortOrder);
        }

        [Fact]
        public void Load_ParsesVisibleIf()
        {
            var page = new FakeSceneNode().Set("text", "Content");
            var n = new FakeSceneNode()
                .Set("id", "LSN_Vis")
                .Set("visibleIf", "LSN_Basics.unlocked")
                .AddNode("PAGE", page);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal("LSN_Basics.unlocked", lesson.VisibleIfRaw);
        }

        [Fact]
        public void Load_SkipsInvalidPagesButKeepsValid()
        {
            var goodPage = new FakeSceneNode().Set("text", "Good");
            var badPage = new FakeSceneNode().Set("title", "Empty");
            var n = new FakeSceneNode()
                .Set("id", "LSN_Mixed")
                .AddNode("PAGE", goodPage)
                .AddNode("PAGE", badPage);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Single(lesson.Pages);
        }

        [Fact]
        public void Load_RejectsNullNode()
        {
            Assert.Null(LessonLoader.Load(null));
        }

        [Fact]
        public void Load_MultiplePages()
        {
            var page1 = new FakeSceneNode().Set("text", "Page 1");
            var page2 = new FakeSceneNode().Set("text", "Page 2");
            var page3 = new FakeSceneNode().Set("text", "Page 3");
            var n = new FakeSceneNode()
                .Set("id", "LSN_Multi")
                .AddNode("PAGE", page1)
                .AddNode("PAGE", page2)
                .AddNode("PAGE", page3);
            var lesson = LessonLoader.Load(n);
            Assert.NotNull(lesson);
            Assert.Equal(3, lesson.Pages.Count);
        }
    }
}
