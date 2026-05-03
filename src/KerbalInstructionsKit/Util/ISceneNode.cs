using System.Collections.Generic;

namespace KerbalInstructionsKit.Util
{
    public interface ISceneNode
    {
        string GetValue(string name);
        bool HasValue(string name);
        IEnumerable<ISceneNode> GetNodes(string name);
    }
}
