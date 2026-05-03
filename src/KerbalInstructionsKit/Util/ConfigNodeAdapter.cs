using System.Collections.Generic;

namespace KerbalInstructionsKit.Util
{
    public sealed class ConfigNodeAdapter : ISceneNode
    {
        private readonly ConfigNode node;

        public ConfigNodeAdapter(ConfigNode node)
        {
            this.node = node;
        }

        public string GetValue(string name) => node.GetValue(name);

        public bool HasValue(string name) => node.HasValue(name);

        public IEnumerable<ISceneNode> GetNodes(string name)
        {
            foreach (var child in node.GetNodes(name))
                yield return new ConfigNodeAdapter(child);
        }
    }
}
