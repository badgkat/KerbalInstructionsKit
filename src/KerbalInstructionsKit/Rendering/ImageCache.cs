using System.Collections.Generic;
using UnityEngine;

namespace KerbalInstructionsKit.Rendering
{
    public static class ImageCache
    {
        private static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        public static Texture2D Get(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (cache.TryGetValue(path, out var tex)) return tex;
            tex = GameDatabase.Instance.GetTexture(path, false);
            cache[path] = tex;
            return tex;
        }
    }
}
