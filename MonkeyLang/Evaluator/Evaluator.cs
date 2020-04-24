using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    [Export(typeof(Evaluator))]
    public class Evaluator
    {
        [ImportingConstructor]
        public Evaluator([Import] Parser parser)
        {
            Parser = parser;
        }

        private Parser Parser { get; }

        public IObject Evaluate(string input)
        {
            AST result = Parser.ParseProgram(input);
            // TODO: what to do with errors?

            return Evaluate(result.Program);
        }

        public IObject Evaluate(INode node)
        {
            return node switch
            {
                Program program => EvaluateStatements(program.Statements),
                ExpressionStatement exprStatement => Evaluate(exprStatement.Expression),
                IntegerLiteral intLiteral => new IntegerObject(intLiteral.Value),
                Boolean boolean => BooleanObject.FromNative(boolean.Value),
                PrefixExpression prefixExpr => EvaluatePrefix(Evaluate(prefixExpr.Right), prefixExpr.Operator),
                _ => throw new ArgumentException("why?"),
            };
        }

        private IObject EvaluatePrefix(IObject right, string op)
        {
            return op switch
            {
                "!" => EvaluateBang(right),
                _ => throw new NotImplementedException()
            };
        }

        private IObject EvaluateBang(IObject right)
        {
            return right switch
            {
                BooleanObject booleanObj => BooleanObject.FromNative(!booleanObj.Value),
                NullObject _ => BooleanObject.True,
                _ => BooleanObject.False
            };
        }

        private IObject EvaluateStatements(IImmutableList<IStatement> statements)
        {
            IObject result = NullObject.Null;

            foreach (var item in statements)
            {
                result = Evaluate(item);
            }

            return result;
        }
    }
}
