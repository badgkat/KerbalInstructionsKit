using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Util;

namespace KerbalInstructionsKit.Config
{
    public static class PageLoader
    {
        public static Page Load(ISceneNode node)
        {
            if (node == null) return null;
            var text = node.GetValue("text");
            var image = node.GetValue("image");
            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(image))
            {
                KikLog.Warn("[KIK] PAGE has neither text nor image, skipping");
                return null;
            }

            var page = new Page
            {
                Title = node.GetValue("title"),
                Text = text,
                Image = image,
                Caption = node.GetValue("caption"),
            };

            foreach (var linkNode in node.GetNodes("LINK"))
            {
                var link = LinkLoader.Load(linkNode);
                if (link != null) page.Links.Add(link);
            }

            return page;
        }
    }
}
