using System.Collections.Generic;
using System.Linq;
using KerbalInstructionsKit.Util;
using KerbalInstructionsKit.Util.Expression;

namespace KerbalInstructionsKit.Core
{
    public static class ArchiveFilter
    {
        private static readonly Dictionary<string, IExpression> visibleIfCache =
            new Dictionary<string, IExpression>();
        private static readonly HashSet<string> visibleIfFailed =
            new HashSet<string>();

        internal static void ResetCache()
        {
            visibleIfCache.Clear();
            visibleIfFailed.Clear();
        }

        private static bool IsVisible(Lesson l, IExpressionContext ctx)
        {
            if (string.IsNullOrEmpty(l.VisibleIfRaw)) return true;
            if (!visibleIfCache.TryGetValue(l.Id, out var expr))
            {
                expr = ExpressionParser.Parse(l.VisibleIfRaw);
                visibleIfCache[l.Id] = expr;
                if (expr == null)
                {
                    KikLog.Warn(
                        $"[KIK] visibleIf parse error on lesson '{l.Id}': '{l.VisibleIfRaw}'. Treated as true.");
                }
            }
            if (expr == null) return true;
            try { return expr.Evaluate(ctx); }
            catch
            {
                if (visibleIfFailed.Add(l.Id))
                    KikLog.Warn(
                        $"[KIK] visibleIf runtime error on lesson '{l.Id}'. Treated as false.");
                return false;
            }
        }

        public static List<ArchiveGroup> Apply(
            LessonRegistry reg, IExpressionContext ctx, ArchiveQuery query)
        {
            var search = (query.Search ?? "").Trim().ToLowerInvariant();
            var tags = query.SelectedTags ?? new List<string>();

            var visible = reg.All
                .Where(l => ctx.IsLessonUnlocked(l.Id))
                .Where(l => IsVisible(l, ctx))
                .Where(l => MatchesSearch(l, search))
                .Where(l => MatchesTags(l, tags))
                .ToList();

            return visible
                .GroupBy(l => string.IsNullOrEmpty(l.Category) ? "Misc" : l.Category)
                .Select(g => new ArchiveGroup
                {
                    Category = g.Key,
                    Lessons = g.OrderBy(l => l.SortOrder)
                               .ThenBy(l => l.Title)
                               .ToList(),
                })
                .OrderBy(g => g.Category == "Misc" ? 1 : 0)
                .ThenBy(g => g.Category)
                .ToList();
        }

        public static List<string> AllVisibleTags(
            LessonRegistry reg, IExpressionContext ctx)
        {
            return reg.All
                .Where(l => ctx.IsLessonUnlocked(l.Id))
                .Where(l => IsVisible(l, ctx))
                .SelectMany(l => l.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
        }

        private static bool MatchesSearch(Lesson l, string search)
        {
            if (string.IsNullOrEmpty(search)) return true;
            if ((l.Title ?? "").ToLowerInvariant().Contains(search)) return true;
            if ((l.Category ?? "").ToLowerInvariant().Contains(search)) return true;
            foreach (var t in l.Tags)
                if (t.ToLowerInvariant().Contains(search)) return true;
            return false;
        }

        private static bool MatchesTags(Lesson l, List<string> tags)
        {
            if (tags.Count == 0) return true;
            foreach (var t in tags)
                if (l.Tags.Contains(t)) return true;
            return false;
        }
    }
}
