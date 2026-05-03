using KerbalInstructionsKit.Tests.TestHelpers;
using KerbalInstructionsKit.Util.Expression;
using Xunit;

namespace KerbalInstructionsKit.Tests
{
    public class ExpressionParserTests
    {
        private readonly FakeExpressionContext ctx;

        public ExpressionParserTests()
        {
            ctx = new FakeExpressionContext();
        }

        [Fact]
        public void Parse_UnlockedExpression_True()
        {
            ctx.UnlockLesson("LSN_Basics");
            var expr = ExpressionParser.Parse("LSN_Basics.unlocked");
            Assert.NotNull(expr);
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_UnlockedExpression_False()
        {
            var expr = ExpressionParser.Parse("LSN_Basics.unlocked");
            Assert.NotNull(expr);
            Assert.False(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_FlagExpression_True()
        {
            ctx.SetFlag("advanced_mode", true);
            var expr = ExpressionParser.Parse("advanced_mode.set");
            Assert.NotNull(expr);
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_FlagExpression_False()
        {
            var expr = ExpressionParser.Parse("advanced_mode.set");
            Assert.NotNull(expr);
            Assert.False(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_And_BothTrue()
        {
            ctx.UnlockLesson("A").UnlockLesson("B");
            var expr = ExpressionParser.Parse("A.unlocked && B.unlocked");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_And_OneFalse()
        {
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("A.unlocked && B.unlocked");
            Assert.False(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Or_OneTrue()
        {
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("A.unlocked || B.unlocked");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Or_BothFalse()
        {
            var expr = ExpressionParser.Parse("A.unlocked || B.unlocked");
            Assert.False(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Not_InvertsTrue()
        {
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("!A.unlocked");
            Assert.False(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Not_InvertsFalse()
        {
            var expr = ExpressionParser.Parse("!A.unlocked");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_ComplexExpression()
        {
            ctx.UnlockLesson("LSN_A").SetFlag("mode", true);
            var expr = ExpressionParser.Parse("LSN_A.unlocked && mode.set");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_ComplexExpression_Mixed()
        {
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("A.unlocked && !B.unlocked");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Precedence_AndBeforeOr()
        {
            // A || B && C should be A || (B && C)
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("A.unlocked || B.unlocked && C.unlocked");
            Assert.True(expr.Evaluate(ctx)); // A is true, so OR is true
        }

        [Fact]
        public void Parse_Precedence_AndBeforeOr_RightSide()
        {
            // Without A, B && C must both be true for OR to succeed
            ctx.UnlockLesson("B").UnlockLesson("C");
            var expr = ExpressionParser.Parse("A.unlocked || B.unlocked && C.unlocked");
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_EmptyString_ReturnsNull()
        {
            Assert.Null(ExpressionParser.Parse(""));
        }

        [Fact]
        public void Parse_WhitespaceOnly_ReturnsNull()
        {
            Assert.Null(ExpressionParser.Parse("   "));
        }

        [Fact]
        public void Parse_Null_ReturnsNull()
        {
            Assert.Null(ExpressionParser.Parse(null));
        }

        [Fact]
        public void Parse_InvalidExpression_ReturnsNull()
        {
            Assert.Null(ExpressionParser.Parse("garbage without dots"));
        }

        [Fact]
        public void Parse_UnknownProperty_ReturnsNull()
        {
            Assert.Null(ExpressionParser.Parse("LSN_A.bananas"));
        }

        [Fact]
        public void Parse_Parentheses()
        {
            // (A || B) && C -- with only A and C true
            ctx.UnlockLesson("A").UnlockLesson("C");
            var expr = ExpressionParser.Parse("(A.unlocked || B.unlocked) && C.unlocked");
            Assert.NotNull(expr);
            Assert.True(expr.Evaluate(ctx));
        }

        [Fact]
        public void Parse_Parentheses_AllFalse()
        {
            // (A || B) && C -- C is false
            ctx.UnlockLesson("A");
            var expr = ExpressionParser.Parse("(A.unlocked || B.unlocked) && C.unlocked");
            Assert.False(expr.Evaluate(ctx));
        }
    }
}
