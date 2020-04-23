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

        public IObject? Evaluate(string input)
        {
            AST result = Parser.ParseProgram(input);
            // TODO: what to do with errors?

            return Evaluate(result.Program);
        }

        public IObject? Evaluate(INode node)
        {
            return node switch
            {
                Program program => EvaluateStatements(program.Statements),
                ExpressionStatement exprStatement => Evaluate(exprStatement.Expression),
                IntegerLiteral intLiteral => new IntegerObject(intLiteral.Value),
                Boolean boolean => new BooleanObject(boolean.Value),
                _ => throw new ArgumentException("why?"),
            };
        }

        private IObject? EvaluateStatements(IImmutableList<IStatement> statements)
        {
            IObject? result = null;

            foreach (var item in statements)
            {
                result = Evaluate(item);
            }

            return result;
        }
    }
}
