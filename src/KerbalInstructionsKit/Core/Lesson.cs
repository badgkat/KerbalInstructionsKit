using System.Collections.Generic;

namespace KerbalInstructionsKit.Core
{
    public sealed class Lesson
    {
        public string Id;
        public string Title;
        public string Category;
        public string VisibleIfRaw;
        public int SortOrder;
        public List<string> Tags = new List<string>();
        public List<Page> Pages = new List<Page>();
    }
}
