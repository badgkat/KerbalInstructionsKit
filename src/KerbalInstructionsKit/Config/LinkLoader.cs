using KerbalInstructionsKit.Core;
using KerbalInstructionsKit.Util;

namespace KerbalInstructionsKit.Config
{
    public static class LinkLoader
    {
        public static Link Load(ISceneNode node)
        {
            if (node == null) return null;
            var typeStr = node.GetValue("type");
            var target = node.GetValue("target");
            if (string.IsNullOrEmpty(typeStr))
            {
                KikLog.Warn("[KIK] LINK missing 'type', skipping");
                return null;
            }
            if (string.IsNullOrEmpty(target))
            {
                KikLog.Warn("[KIK] LINK missing 'target', skipping");
                return null;
            }

            LinkType type;
            switch (typeStr.ToLowerInvariant())
            {
                case "lesson":  type = LinkType.Lesson; break;
                case "kspedia": type = LinkType.Kspedia; break;
                case "url":     type = LinkType.Url; break;
                default:
                    KikLog.Warn($"[KIK] LINK has unknown type '{typeStr}', skipping");
                    return null;
            }

            if (type == LinkType.Url)
            {
                // KSP cfg treats "//" as a comment, so authors write URLs with
                // backslashes: https:\example.com\path. Normalize to real URL.
                if (target.StartsWith("https:\\"))
                    target = "https://" + target.Substring(7).Replace('\\', '/');
                else if (target.StartsWith("http:\\"))
                    target = "http://" + target.Substring(6).Replace('\\', '/');

                if (!target.StartsWith("http://") && !target.StartsWith("https://"))
                {
                    KikLog.Warn($"[KIK] LINK url target '{target}' is not http(s), skipping");
                    return null;
                }
            }

            return new Link
            {
                Type = type,
                Target = target,
                Label = node.GetValue("label") ?? target,
            };
        }
    }
}
