using System.Collections.Generic;
using KerbalInstructionsKit.Core;

namespace KerbalInstructionsKit.Rendering
{
    public sealed class PanelStateMachine
    {
        private readonly LessonRegistry registry;
        private readonly Stack<string> history = new Stack<string>();

        public PanelView View { get; private set; }
        public Lesson CurrentLesson { get; private set; }
        public int CurrentPage { get; private set; }
        public bool CanGoBack => history.Count > 0;

        public PanelStateMachine(LessonRegistry registry)
        {
            this.registry = registry;
            View = PanelView.Archive;
        }

        public void OpenLesson(string id)
        {
            var lesson = registry.Get(id);
            if (lesson == null) return;
            history.Clear();
            CurrentLesson = lesson;
            CurrentPage = 0;
            View = PanelView.Lesson;
        }

        public void OpenArchive()
        {
            View = PanelView.Archive;
        }

        public void BackToLesson()
        {
            if (CurrentLesson != null)
                View = PanelView.Lesson;
        }

        public void NavigateToLessonViaLink(string id)
        {
            var lesson = registry.Get(id);
            if (lesson == null) return;
            if (CurrentLesson != null)
                history.Push(CurrentLesson.Id);
            CurrentLesson = lesson;
            CurrentPage = 0;
            View = PanelView.Lesson;
        }

        public void GoBack()
        {
            if (history.Count == 0) return;
            var prevId = history.Pop();
            var prev = registry.Get(prevId);
            if (prev != null)
            {
                CurrentLesson = prev;
                CurrentPage = 0;
                View = PanelView.Lesson;
            }
        }

        public void JumpToPage(int page)
        {
            if (CurrentLesson == null) return;
            if (page < 0) page = 0;
            if (page >= CurrentLesson.Pages.Count) page = CurrentLesson.Pages.Count - 1;
            CurrentPage = page;
        }

        public void NextPage()
        {
            if (CurrentLesson == null) return;
            if (CurrentPage < CurrentLesson.Pages.Count - 1)
                CurrentPage++;
        }

        public void PrevPage()
        {
            if (CurrentLesson == null) return;
            if (CurrentPage > 0)
                CurrentPage--;
        }
    }
}
