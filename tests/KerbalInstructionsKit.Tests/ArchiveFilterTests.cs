using System.Collections.Generic;
using System.Linq;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Tests.TestHelpers;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class ArchiveFilterTests
    {
        private LessonRegistry reg;
        private FakeExpressionContext ctx;

        public ArchiveFilterTests()
        {
            ArchiveFilter.ResetCache();
            reg = new LessonRegistry();
            ctx = new FakeExpressionContext();
        }

        private Lesson MakeLesson(string id, string category = "Basics", string title = null, List<string> tags = null)
        {
            return new Lesson
            {
                Id = id,
                Title = title ?? id,
                Category = category,
                Tags = tags ?? new List<string>(),
                Pages = { new Page { Text = "content" } }
            };
        }

        [Fact]
        public void Apply_ReturnsOnlyUnlockedLessons()
        {
            reg.Register(MakeLesson("A"));
            reg.Register(MakeLesson("B"));
            ctx.UnlockLesson("A");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
            Assert.Equal("A", all[0].Id);
        }

        [Fact]
        public void Apply_GroupsByCategory()
        {
            reg.Register(MakeLesson("A", "Rockets"));
            reg.Register(MakeLesson("B", "Planes"));
            reg.Register(MakeLesson("C", "Rockets"));
            ctx.UnlockLesson("A").UnlockLesson("B").UnlockLesson("C");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            Assert.Equal(2, groups.Count);
            var rockets = groups.First(g => g.Category == "Rockets");
            Assert.Equal(2, rockets.Lessons.Count);
        }

        [Fact]
        public void Apply_NullCategoryBecomesMisc()
        {
            reg.Register(MakeLesson("A", null));
            ctx.UnlockLesson("A");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            Assert.Single(groups);
            Assert.Equal("Misc", groups[0].Category);
        }

        [Fact]
        public void Apply_MiscSortsLast()
        {
            reg.Register(MakeLesson("A", null));
            reg.Register(MakeLesson("B", "Alpha"));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            Assert.Equal("Alpha", groups[0].Category);
            Assert.Equal("Misc", groups[1].Category);
        }

        [Fact]
        public void Apply_SearchFiltersByTitle()
        {
            reg.Register(MakeLesson("A", "Cat", "Rocket Basics"));
            reg.Register(MakeLesson("B", "Cat", "Orbit Tutorial"));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var query = new ArchiveQuery { Search = "rocket" };
            var groups = ArchiveFilter.Apply(reg, ctx, query);
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
            Assert.Equal("A", all[0].Id);
        }

        [Fact]
        public void Apply_SearchFiltersByCategory()
        {
            reg.Register(MakeLesson("A", "Rockets", "Lesson A"));
            reg.Register(MakeLesson("B", "Planes", "Lesson B"));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var query = new ArchiveQuery { Search = "planes" };
            var groups = ArchiveFilter.Apply(reg, ctx, query);
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
            Assert.Equal("B", all[0].Id);
        }

        [Fact]
        public void Apply_SearchFiltersByTag()
        {
            reg.Register(MakeLesson("A", "Cat", "X", new List<string> { "beginner" }));
            reg.Register(MakeLesson("B", "Cat", "Y", new List<string> { "advanced" }));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var query = new ArchiveQuery { Search = "beginner" };
            var groups = ArchiveFilter.Apply(reg, ctx, query);
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
            Assert.Equal("A", all[0].Id);
        }

        [Fact]
        public void Apply_TagFilterIncludesMatchingLessons()
        {
            reg.Register(MakeLesson("A", "Cat", "X", new List<string> { "rockets" }));
            reg.Register(MakeLesson("B", "Cat", "Y", new List<string> { "planes" }));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var query = new ArchiveQuery { SelectedTags = new List<string> { "rockets" } };
            var groups = ArchiveFilter.Apply(reg, ctx, query);
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
            Assert.Equal("A", all[0].Id);
        }

        [Fact]
        public void Apply_EmptyTagsShowsAll()
        {
            reg.Register(MakeLesson("A", "Cat", "X", new List<string> { "rockets" }));
            reg.Register(MakeLesson("B", "Cat", "Y", new List<string> { "planes" }));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var query = new ArchiveQuery { SelectedTags = new List<string>() };
            var groups = ArchiveFilter.Apply(reg, ctx, query);
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Equal(2, all.Count);
        }

        [Fact]
        public void Apply_VisibleIfGatesLessons()
        {
            var lesson = MakeLesson("A");
            lesson.VisibleIfRaw = "LSN_Prereq.unlocked";
            reg.Register(lesson);
            ctx.UnlockLesson("A"); // A is unlocked but its visibleIf requires LSN_Prereq

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Empty(all);
        }

        [Fact]
        public void Apply_VisibleIfPassesWhenMet()
        {
            var lesson = MakeLesson("A");
            lesson.VisibleIfRaw = "LSN_Prereq.unlocked";
            reg.Register(lesson);
            ctx.UnlockLesson("A").UnlockLesson("LSN_Prereq");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            var all = groups.SelectMany(g => g.Lessons).ToList();
            Assert.Single(all);
        }

        [Fact]
        public void Apply_OrdersBySortOrderThenTitle()
        {
            var l1 = MakeLesson("C", "Cat", "Zulu");
            l1.SortOrder = 2;
            var l2 = MakeLesson("B", "Cat", "Alpha");
            l2.SortOrder = 1;
            var l3 = MakeLesson("A", "Cat", "Beta");
            l3.SortOrder = 1;
            reg.Register(l1);
            reg.Register(l2);
            reg.Register(l3);
            ctx.UnlockLesson("A").UnlockLesson("B").UnlockLesson("C");

            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            var lessons = groups[0].Lessons;
            Assert.Equal("Alpha", lessons[0].Title);
            Assert.Equal("Beta", lessons[1].Title);
            Assert.Equal("Zulu", lessons[2].Title);
        }

        [Fact]
        public void AllVisibleTags_ReturnsDistinctSortedTags()
        {
            reg.Register(MakeLesson("A", "Cat", "X", new List<string> { "rockets", "basics" }));
            reg.Register(MakeLesson("B", "Cat", "Y", new List<string> { "rockets", "advanced" }));
            ctx.UnlockLesson("A").UnlockLesson("B");

            var tags = ArchiveFilter.AllVisibleTags(reg, ctx);
            Assert.Equal(new List<string> { "advanced", "basics", "rockets" }, tags);
        }

        [Fact]
        public void Apply_EmptyRegistryReturnsNoGroups()
        {
            var groups = ArchiveFilter.Apply(reg, ctx, new ArchiveQuery());
            Assert.Empty(groups);
        }
    }
}
