using System.Collections.Generic;

namespace KerbalInstructionsKit.Core
{
    public sealed class Page
    {
        public string Title;
        public string Text;
        public string Image;
        public string Caption;
        public List<Link> Links = new List<Link>();
    }
}
