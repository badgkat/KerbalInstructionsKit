using System.Globalization;
using System.Linq;
using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Util;

namespace KerbalInstructionsKit.Config
{
    public static class LessonLoader
    {
        public static Lesson Load(ISceneNode node)
        {
            if (node == null) return null;
            var id = node.GetValue("id");
            if (string.IsNullOrEmpty(id))
            {
                KikLog.Error("[KIK] INSTRUCTION_LESSON missing 'id', skipping");
                return null;
            }

            var lesson = new Lesson
            {
                Id = id,
                Title = node.GetValue("title") ?? id,
                Category = node.GetValue("category"),
                VisibleIfRaw = node.GetValue("visibleIf"),
            };

            if (node.HasValue("sortOrder") &&
                int.TryParse(node.GetValue("sortOrder"),
                    NumberStyles.Integer, CultureInfo.InvariantCulture, out var so))
            {
                lesson.SortOrder = so;
            }

            var tagsRaw = node.GetValue("tags");
            if (!string.IsNullOrEmpty(tagsRaw))
            {
                lesson.Tags = tagsRaw.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => t.Length > 0)
                    .ToList();
            }

            foreach (var pageNode in node.GetNodes("PAGE"))
            {
                var page = PageLoader.Load(pageNode);
                if (page != null) lesson.Pages.Add(page);
            }

            if (lesson.Pages.Count == 0)
            {
                KikLog.Error($"[KIK] INSTRUCTION_LESSON '{id}' has no valid PAGE blocks, skipping");
                return null;
            }

            return lesson;
        }
    }
}
