using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Rendering;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class PanelStateMachineTests
    {
        private LessonRegistry reg;
        private PanelStateMachine sm;

        public PanelStateMachineTests()
        {
            reg = new LessonRegistry();
            reg.Register(new Lesson
            {
                Id = "LSN_A",
                Title = "Lesson A",
                Pages = { new Page { Text = "Page 1" }, new Page { Text = "Page 2" }, new Page { Text = "Page 3" } }
            });
            reg.Register(new Lesson
            {
                Id = "LSN_B",
                Title = "Lesson B",
                Pages = { new Page { Text = "B1" } }
            });
            reg.Register(new Lesson
            {
                Id = "LSN_C",
                Title = "Lesson C",
                Pages = { new Page { Text = "C1" }, new Page { Text = "C2" } }
            });
            sm = new PanelStateMachine(reg);
        }

        [Fact]
        public void InitialState_IsArchive()
        {
            Assert.Equal(PanelView.Archive, sm.View);
            Assert.Null(sm.CurrentLesson);
        }

        [Fact]
        public void OpenLesson_SetsLessonView()
        {
            sm.OpenLesson("LSN_A");
            Assert.Equal(PanelView.Lesson, sm.View);
            Assert.Equal("LSN_A", sm.CurrentLesson.Id);
            Assert.Equal(0, sm.CurrentPage);
        }

        [Fact]
        public void OpenLesson_NonExistent_DoesNothing()
        {
            sm.OpenLesson("NOPE");
            Assert.Equal(PanelView.Archive, sm.View);
            Assert.Null(sm.CurrentLesson);
        }

        [Fact]
        public void OpenArchive_SetsArchiveView()
        {
            sm.OpenLesson("LSN_A");
            sm.OpenArchive();
            Assert.Equal(PanelView.Archive, sm.View);
            // CurrentLesson is preserved for "back to lesson"
            Assert.NotNull(sm.CurrentLesson);
        }

        [Fact]
        public void BackToLesson_ReturnsToLessonView()
        {
            sm.OpenLesson("LSN_A");
            sm.OpenArchive();
            sm.BackToLesson();
            Assert.Equal(PanelView.Lesson, sm.View);
            Assert.Equal("LSN_A", sm.CurrentLesson.Id);
        }

        [Fact]
        public void NextPage_AdvancesPage()
        {
            sm.OpenLesson("LSN_A");
            sm.NextPage();
            Assert.Equal(1, sm.CurrentPage);
            sm.NextPage();
            Assert.Equal(2, sm.CurrentPage);
        }

        [Fact]
        public void NextPage_StopsAtLastPage()
        {
            sm.OpenLesson("LSN_A");
            sm.NextPage();
            sm.NextPage();
            sm.NextPage(); // Should not go past page 2
            Assert.Equal(2, sm.CurrentPage);
        }

        [Fact]
        public void PrevPage_GoesBack()
        {
            sm.OpenLesson("LSN_A");
            sm.NextPage();
            sm.NextPage();
            sm.PrevPage();
            Assert.Equal(1, sm.CurrentPage);
        }

        [Fact]
        public void PrevPage_StopsAtFirstPage()
        {
            sm.OpenLesson("LSN_A");
            sm.PrevPage();
            Assert.Equal(0, sm.CurrentPage);
        }

        [Fact]
        public void JumpToPage_Valid()
        {
            sm.OpenLesson("LSN_A");
            sm.JumpToPage(2);
            Assert.Equal(2, sm.CurrentPage);
        }

        [Fact]
        public void JumpToPage_ClampsTooHigh()
        {
            sm.OpenLesson("LSN_A");
            sm.JumpToPage(100);
            Assert.Equal(2, sm.CurrentPage); // 3 pages, max index 2
        }

        [Fact]
        public void JumpToPage_ClampsNegative()
        {
            sm.OpenLesson("LSN_A");
            sm.JumpToPage(-5);
            Assert.Equal(0, sm.CurrentPage);
        }

        [Fact]
        public void NavigateToLessonViaLink_PushesHistory()
        {
            sm.OpenLesson("LSN_A");
            sm.NavigateToLessonViaLink("LSN_B");
            Assert.Equal("LSN_B", sm.CurrentLesson.Id);
            Assert.True(sm.CanGoBack);
        }

        [Fact]
        public void GoBack_RestoresPreviousLesson()
        {
            sm.OpenLesson("LSN_A");
            sm.NavigateToLessonViaLink("LSN_B");
            sm.GoBack();
            Assert.Equal("LSN_A", sm.CurrentLesson.Id);
            Assert.False(sm.CanGoBack);
        }

        [Fact]
        public void GoBack_WithEmptyHistory_DoesNothing()
        {
            sm.OpenLesson("LSN_A");
            Assert.False(sm.CanGoBack);
            sm.GoBack();
            Assert.Equal("LSN_A", sm.CurrentLesson.Id);
        }

        [Fact]
        public void NavigateToLessonViaLink_NonExistent_DoesNothing()
        {
            sm.OpenLesson("LSN_A");
            sm.NavigateToLessonViaLink("NOPE");
            Assert.Equal("LSN_A", sm.CurrentLesson.Id);
            Assert.False(sm.CanGoBack);
        }

        [Fact]
        public void OpenLesson_ClearsHistory()
        {
            sm.OpenLesson("LSN_A");
            sm.NavigateToLessonViaLink("LSN_B");
            sm.OpenLesson("LSN_C");
            Assert.False(sm.CanGoBack);
        }

        [Fact]
        public void OpenLesson_ResetsPageToZero()
        {
            sm.OpenLesson("LSN_A");
            sm.NextPage();
            sm.OpenLesson("LSN_A");
            Assert.Equal(0, sm.CurrentPage);
        }
    }
}
