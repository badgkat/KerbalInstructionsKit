using System.Collections.Generic;
using System.Linq;
using KerbalInstructionsKit.Util;

namespace KerbalInstructionsKit.Tests.TestHelpers
{
    public sealed class FakeSceneNode : ISceneNode
    {
        private readonly Dictionary<string, string> values = new Dictionary<string, string>();
        private readonly Dictionary<string, List<ISceneNode>> nodes = new Dictionary<string, List<ISceneNode>>();

        public FakeSceneNode Set(string key, string value)
        {
            values[key] = value;
            return this;
        }

        public FakeSceneNode AddNode(string name, ISceneNode child)
        {
            if (!nodes.TryGetValue(name, out var list))
                nodes[name] = list = new List<ISceneNode>();
            list.Add(child);
            return this;
        }

        public string GetValue(string name) =>
            values.TryGetValue(name, out var v) ? v : null;

        public bool HasValue(string name) => values.ContainsKey(name);

        public IEnumerable<ISceneNode> GetNodes(string name) =>
            nodes.TryGetValue(name, out var list) ? list : Enumerable.Empty<ISceneNode>();
    }
}
