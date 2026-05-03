using KerbalInstructionsKit.Core;

namespace KerbalInstructionsKit.Util.Expression
{
    public sealed class StateExpressionContext : IExpressionContext
    {
        private readonly LessonState state;

        public StateExpressionContext(LessonState state)
        {
            this.state = state;
        }

        public bool IsLessonUnlocked(string id) => state.IsUnlocked(id);
        public bool GetFlag(string name) => state.GetFlag(name);
    }
}
