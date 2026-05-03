using System;
using System.Collections.Generic;

namespace KerbalInstructionsKit.Util.Expression
{
    public static class ExpressionParser
    {
        public static IExpression Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                var tokens = Tokenize(raw);
                int pos = 0;
                var expr = ParseOr(tokens, ref pos);
                if (pos != tokens.Count) return null;
                return expr;
            }
            catch
            {
                return null;
            }
        }

        private static IExpression ParseOr(List<string> tokens, ref int pos)
        {
            var left = ParseAnd(tokens, ref pos);
            while (pos < tokens.Count && tokens[pos] == "||")
            {
                pos++;
                var right = ParseAnd(tokens, ref pos);
                left = new OrExpression(left, right);
            }
            return left;
        }

        private static IExpression ParseAnd(List<string> tokens, ref int pos)
        {
            var left = ParseUnary(tokens, ref pos);
            while (pos < tokens.Count && tokens[pos] == "&&")
            {
                pos++;
                var right = ParseUnary(tokens, ref pos);
                left = new AndExpression(left, right);
            }
            return left;
        }

        private static IExpression ParseUnary(List<string> tokens, ref int pos)
        {
            if (pos < tokens.Count && tokens[pos] == "!")
            {
                pos++;
                var inner = ParseUnary(tokens, ref pos);
                return new NotExpression(inner);
            }
            return ParseAtom(tokens, ref pos);
        }

        private static IExpression ParseAtom(List<string> tokens, ref int pos)
        {
            if (pos >= tokens.Count)
                throw new InvalidOperationException("Unexpected end of expression");

            var token = tokens[pos];

            if (token == "(")
            {
                pos++;
                var expr = ParseOr(tokens, ref pos);
                if (pos >= tokens.Count || tokens[pos] != ")")
                    throw new InvalidOperationException("Missing closing parenthesis");
                pos++;
                return expr;
            }

            // Must be an identifier with dot-property: ID.unlocked or ID.set
            int dotIndex = token.LastIndexOf('.');
            if (dotIndex < 0 || dotIndex == 0 || dotIndex == token.Length - 1)
                throw new InvalidOperationException($"Invalid atom: '{token}'");

            var name = token.Substring(0, dotIndex);
            var prop = token.Substring(dotIndex + 1).ToLowerInvariant();
            pos++;

            switch (prop)
            {
                case "unlocked":
                    return new UnlockedExpression(name);
                case "set":
                    return new FlagExpression(name);
                default:
                    throw new InvalidOperationException($"Unknown property '.{prop}' on '{name}'");
            }
        }

        private static List<string> Tokenize(string raw)
        {
            var tokens = new List<string>();
            int i = 0;
            while (i < raw.Length)
            {
                char c = raw[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                if (c == '&' && i + 1 < raw.Length && raw[i + 1] == '&')
                {
                    tokens.Add("&&"); i += 2; continue;
                }
                if (c == '|' && i + 1 < raw.Length && raw[i + 1] == '|')
                {
                    tokens.Add("||"); i += 2; continue;
                }
                if (c == '!') { tokens.Add("!"); i++; continue; }
                if (c == '(') { tokens.Add("("); i++; continue; }
                if (c == ')') { tokens.Add(")"); i++; continue; }

                // Identifier (including dots)
                int start = i;
                while (i < raw.Length && !char.IsWhiteSpace(raw[i])
                    && raw[i] != '&' && raw[i] != '|' && raw[i] != '!'
                    && raw[i] != '(' && raw[i] != ')')
                {
                    i++;
                }
                tokens.Add(raw.Substring(start, i - start));
            }
            return tokens;
        }
    }

    internal sealed class UnlockedExpression : IExpression
    {
        private readonly string lessonId;
        public UnlockedExpression(string lessonId) { this.lessonId = lessonId; }
        public bool Evaluate(IExpressionContext ctx) => ctx.IsLessonUnlocked(lessonId);
    }

    internal sealed class FlagExpression : IExpression
    {
        private readonly string flagName;
        public FlagExpression(string flagName) { this.flagName = flagName; }
        public bool Evaluate(IExpressionContext ctx) => ctx.GetFlag(flagName);
    }

    internal sealed class AndExpression : IExpression
    {
        private readonly IExpression left, right;
        public AndExpression(IExpression left, IExpression right) { this.left = left; this.right = right; }
        public bool Evaluate(IExpressionContext ctx) => left.Evaluate(ctx) && right.Evaluate(ctx);
    }

    internal sealed class OrExpression : IExpression
    {
        private readonly IExpression left, right;
        public OrExpression(IExpression left, IExpression right) { this.left = left; this.right = right; }
        public bool Evaluate(IExpressionContext ctx) => left.Evaluate(ctx) || right.Evaluate(ctx);
    }

    internal sealed class NotExpression : IExpression
    {
        private readonly IExpression inner;
        public NotExpression(IExpression inner) { this.inner = inner; }
        public bool Evaluate(IExpressionContext ctx) => !inner.Evaluate(ctx);
    }
}
