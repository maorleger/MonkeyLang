﻿using System;
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
                InfixExpression infixExpr => EvaluateInfix(Evaluate(infixExpr.Left), infixExpr.Operator, Evaluate(infixExpr.Right)),
                _ => NullObject.Null
            };
        }

        private IObject EvaluateInfix(IObject left, string op, IObject right)
        {
            if (left is IntegerObject li && right is IntegerObject ri)
            {
                return EvaluateIntegerInfix(li, op, ri);
            }

            if (left is BooleanObject lb && right is BooleanObject rb)
            {
                return EvaluateBooleanInfix(lb, op, rb);
            }

            return NullObject.Null;
        }

        private IObject EvaluateIntegerInfix(IntegerObject l, string op, IntegerObject r)
        {
            return op switch
            {
                "+" => new IntegerObject(l.Value + r.Value),
                "-" => new IntegerObject(l.Value - r.Value),
                "*" => new IntegerObject(l.Value * r.Value),
                "/" => new IntegerObject(l.Value / r.Value),
                "<" => BooleanObject.FromNative(l.Value < r.Value),
                ">" => BooleanObject.FromNative(l.Value > r.Value),
                "==" => BooleanObject.FromNative(l.Value == r.Value),
                "!=" => BooleanObject.FromNative(l.Value != r.Value),
                _ => NullObject.Null
            };
        }

        private IObject EvaluateBooleanInfix(BooleanObject l, string op, BooleanObject r)
        {
            return op switch
            {
                "==" => BooleanObject.FromNative(l.Value == r.Value),
                "!=" => BooleanObject.FromNative(l.Value != r.Value),
                _ => NullObject.Null
            };
        }

        private IObject EvaluatePrefix(IObject right, string op)
        {
            return op switch
            {
                "!" => EvaluateUnaryNot(right),
                "-" => EvaluateUnaryMinus(right),
                _ => NullObject.Null
            };
        }

        private IObject EvaluateUnaryMinus(IObject right)
        {
            if (right is IntegerObject i) {
                return new IntegerObject(-i.Value);
            }
            return NullObject.Null;
        }

        private IObject EvaluateUnaryNot(IObject right)
        {
            return right switch
            {
                BooleanObject b => BooleanObject.FromNative(!b.Value),
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
