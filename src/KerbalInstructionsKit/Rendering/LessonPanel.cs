using System;
using ClickThroughFix;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Runtime;
using KerbalInstructionsKit.Util;
using UnityEngine;

namespace KerbalInstructionsKit.Rendering
{
    public sealed class LessonPanel : IDisposable
    {
        private const int WindowId = 0x4B49_4B00;

        private readonly KerbalInstructionsKitScenario scenario;
        private readonly IPauseController pause;
        private readonly PanelStateMachine stateMachine;
        private readonly ArchiveQuery query = new ArchiveQuery();
        private readonly LinkRouter linkRouter;

        private bool visible;
        private Rect window = new Rect(60, 60, 900, 640);
        private Vector2 lessonScroll;
        private Vector2 archiveScroll;

        private bool wasPausedOnOpen;
        private bool kikPausedFlag;

        public LessonPanel(KerbalInstructionsKitScenario scenario, IPauseController pause)
        {
            this.scenario = scenario;
            this.pause = pause;
            stateMachine = new PanelStateMachine(scenario.Lessons);
            linkRouter = new LinkRouter(stateMachine.NavigateToLessonViaLink);
        }

        public void Toggle()
        {
            if (visible) Close();
            else OpenDefault();
        }

        public void OpenDefault()
        {
            if (!string.IsNullOrEmpty(scenario.State.LastViewedLesson)
                && scenario.Lessons.Contains(scenario.State.LastViewedLesson))
            {
                stateMachine.OpenLesson(scenario.State.LastViewedLesson);
                stateMachine.JumpToPage(scenario.State.LastViewedPage);
            }
            else
            {
                stateMachine.OpenArchive();
            }
            Show();
        }

        public void OpenLesson(string id)
        {
            stateMachine.OpenLesson(id);
            Show();
        }

        public void OpenArchive()
        {
            stateMachine.OpenArchive();
            Show();
        }

        private void Show()
        {
            wasPausedOnOpen = pause.IsAvailable && pause.IsPaused;
            kikPausedFlag = false;
            visible = true;
        }

        public void Close()
        {
            if (kikPausedFlag && pause.IsAvailable)
                pause.SetPaused(wasPausedOnOpen);
            visible = false;

            if (stateMachine.CurrentLesson != null)
            {
                scenario.State.LastViewedLesson = stateMachine.CurrentLesson.Id;
                scenario.State.LastViewedPage = stateMachine.CurrentPage;
            }
        }

        public void Draw()
        {
            if (!visible) return;
            PanelStyles.Init();
            window = ClickThruBlocker.GUILayoutWindow(WindowId, window, DrawWindow, "Kerbal Instructions Kit");
        }

        private void DrawWindow(int id)
        {
            if (stateMachine.View == PanelView.Lesson)
                DrawLessonView();
            else
                DrawArchiveView();
            GUI.DragWindow(new Rect(0, 0, window.width, 24));
        }

        private void DrawLessonView()
        {
            var lesson = stateMachine.CurrentLesson;
            if (lesson == null)
            {
                stateMachine.OpenArchive();
                return;
            }

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("All Lessons", GUILayout.Width(90)))
                stateMachine.OpenArchive();
            if (stateMachine.CanGoBack)
            {
                if (GUILayout.Button("← Back", GUILayout.Width(70)))
                    stateMachine.GoBack();
            }
            GUILayout.Label(lesson.Title, PanelStyles.Title);
            GUILayout.FlexibleSpace();
            if (pause.IsAvailable)
            {
                var label = pause.IsPaused && kikPausedFlag ? "▶ Resume" : "⏸ Pause";
                if (GUILayout.Button(label, GUILayout.Width(100)))
                {
                    if (kikPausedFlag) { pause.SetPaused(false); kikPausedFlag = false; }
                    else               { pause.SetPaused(true);  kikPausedFlag = true;  }
                }
            }
            if (GUILayout.Button("✕", GUILayout.Width(28))) Close();
            GUILayout.EndHorizontal();

            var page = lesson.Pages[stateMachine.CurrentPage];

            if (!string.IsNullOrEmpty(page.Title))
                GUILayout.Label(page.Title, PanelStyles.PageTitle);

            lessonScroll = GUILayout.BeginScrollView(lessonScroll);

            if (!string.IsNullOrEmpty(page.Image))
            {
                var tex = ImageCache.Get(page.Image);
                if (tex != null)
                {
                    float w = Mathf.Min(600, tex.width);
                    float h = w * tex.height / Mathf.Max(1, tex.width);
                    var r = GUILayoutUtility.GetRect(w, h, GUILayout.Width(w), GUILayout.Height(h));
                    r.x = (window.width - w) / 2;
                    GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                }
                if (!string.IsNullOrEmpty(page.Caption))
                    GUILayout.Label(page.Caption, PanelStyles.Caption);
            }

            if (!string.IsNullOrEmpty(page.Text))
                GUILayout.Label(page.Text.Replace("\\n", "\n"), PanelStyles.RichLabel);

            if (page.Links.Count > 0)
            {
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                foreach (var link in page.Links)
                {
                    string label = LabelFor(link);
                    bool enabled = IsLinkEnabled(link);
                    GUI.enabled = enabled;
                    if (GUILayout.Button(label, PanelStyles.LinkButton))
                        linkRouter.Activate(link);
                    GUI.enabled = true;
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUI.enabled = stateMachine.CurrentPage > 0;
            if (GUILayout.Button("◀ Prev", GUILayout.Width(100))) stateMachine.PrevPage();
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"Page {stateMachine.CurrentPage + 1} of {lesson.Pages.Count}");
            GUILayout.FlexibleSpace();
            GUI.enabled = stateMachine.CurrentPage < lesson.Pages.Count - 1;
            if (GUILayout.Button("Next ▶", GUILayout.Width(100))) stateMachine.NextPage();
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private string LabelFor(Link link)
        {
            var glyph = link.Type == LinkType.Lesson ? "→ "
                      : link.Type == LinkType.Kspedia ? "↗ "
                      : "↗ ";
            var arrow = link.Type == LinkType.Url ? " ↗" : "";
            return glyph + (link.Label ?? link.Target) + arrow;
        }

        private bool IsLinkEnabled(Link link)
        {
            if (link.Type != LinkType.Lesson) return true;
            return scenario.Lessons.Contains(link.Target)
                && scenario.State.IsUnlocked(link.Target);
        }

        private void DrawArchiveView()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (stateMachine.CurrentLesson != null)
            {
                if (GUILayout.Button("← Back to Lesson", GUILayout.Width(130)))
                    stateMachine.BackToLesson();
            }
            GUILayout.Label("All Lessons", PanelStyles.Title);
            GUILayout.FlexibleSpace();
            if (pause.IsAvailable)
            {
                var label = pause.IsPaused && kikPausedFlag ? "▶ Resume" : "⏸ Pause";
                if (GUILayout.Button(label, GUILayout.Width(100)))
                {
                    if (kikPausedFlag) { pause.SetPaused(false); kikPausedFlag = false; }
                    else               { pause.SetPaused(true);  kikPausedFlag = true;  }
                }
            }
            if (GUILayout.Button("✕", GUILayout.Width(28))) Close();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            query.Search = GUILayout.TextField(query.Search ?? "");
            if (GUILayout.Button("Clear", GUILayout.Width(60))) query.Search = "";
            GUILayout.EndHorizontal();

            archiveScroll = GUILayout.BeginScrollView(archiveScroll);
            var groups = ArchiveFilter.Apply(scenario.Lessons, scenario.ExpressionContext, query);

            if (groups.Count == 0)
            {
                GUILayout.Label("No lessons unlocked yet — keep playing!", PanelStyles.RichLabel);
            }
            else
            {
                foreach (var group in groups)
                {
                    GUILayout.Space(4);
                    GUILayout.Label(group.Category, PanelStyles.CategoryHeader);
                    foreach (var lesson in group.Lessons)
                    {
                        bool isLast = stateMachine.CurrentLesson?.Id == lesson.Id;
                        var label = isLast ? "  ● " + lesson.Title + "  ◀ last viewed"
                                           : "  · " + lesson.Title;
                        if (GUILayout.Button(label, isLast ? PanelStyles.TocItemActive : PanelStyles.TocItem,
                                             GUILayout.ExpandWidth(true)))
                        {
                            stateMachine.OpenLesson(lesson.Id);
                        }
                    }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        public void Dispose() { visible = false; }
    }
}
