namespace KerbalInstructionsKit.Util.Expression
{
    public interface IExpression
    {
        bool Evaluate(IExpressionContext ctx);
    }
}
