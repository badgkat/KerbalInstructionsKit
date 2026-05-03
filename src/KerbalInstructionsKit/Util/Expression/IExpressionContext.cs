namespace KerbalInstructionsKit.Util.Expression
{
    public interface IExpressionContext
    {
        bool IsLessonUnlocked(string id);
        bool GetFlag(string name);
    }
}
