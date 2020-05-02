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
            Environments = new Stack<MonkeyEnvironment>();
        }

        private Parser Parser { get; }
        private Stack<MonkeyEnvironment> Environments { get; }

        public IObject Evaluate(string input)
        {
            AST result = Parser.ParseProgram(input);
            if (result.HasErrors)
            {
                return new ErrorObject(
                    result.Errors.Aggregate(
                        string.Empty, 
                        (acc, st) => $"{acc}{st.Message};"));
            }

            Environments.Push(new MonkeyEnvironment());
            return Evaluate(result.Program);
        }

        private IObject Evaluate(INode node)
        {
            try
            {
                return node switch
                {
                    Program program => EvaluateStatements(program.Statements, unwrapReturn: true),
                    ExpressionStatement exprStatement => Evaluate(exprStatement.Expression),
                    IntegerLiteral intLiteral => new IntegerObject(intLiteral.Value),
                    Boolean boolean => BooleanObject.FromNative(boolean.Value),
                    PrefixExpression prefixExpr => EvaluatePrefix(Evaluate(prefixExpr.Right), prefixExpr.Operator),
                    InfixExpression infixExpr => EvaluateInfix(Evaluate(infixExpr.Left), infixExpr.Operator, Evaluate(infixExpr.Right)),
                    IfExpression ifExpr => EvaluateConditional(Evaluate(ifExpr.Condition), ifExpr.Consequence, ifExpr.Alternative),
                    ReturnStatement returnStmt => new ReturnValue(Evaluate(returnStmt.ReturnValue)),
                    LetStatement letStmt => EvaluateLet(letStmt.Name, Evaluate(letStmt.Value)),
                    Identifier ident => EvaluateIdentifier(ident),
                    _ => NullObject.Null
                };
            }
            catch (MonkeyEvaluatorException ex)
            {
                return new ErrorObject(ex.Message);
            }
        }

        private IObject EvaluateIdentifier(Identifier ident)
        {
            //TODO: should I use Value or implement equality for an identifier?
            var result = Environments.Peek().Get(ident.Value);

            if (result == null)
            {
                throw new MonkeyEvaluatorException($"identifier not found: {ident.StringValue}");
            }

            return result;
        }

        private IObject EvaluateLet(Identifier name, IObject value)
        {
            Environments.Peek().Set(name.Value, value);
            return value;
        }

        private IObject EvaluateInfix(IObject left, TokenType op, IObject right)
        {
            return (left, right) switch
            {
                (IntegerObject li, IntegerObject ri) => EvaluateIntegerInfix(li, op, ri),
                (BooleanObject lb, BooleanObject rb) => EvaluateBooleanInfix(lb, op, rb),
                _ => throw new MonkeyEvaluatorException($"type mismatch: {left.Type} {op.GetDescription()} {right.Type}")
            };
        }

        private IObject EvaluateIntegerInfix(IntegerObject l, TokenType op, IntegerObject r)
        {
            return op switch
            {
                TokenType.Plus => new IntegerObject(l.Value + r.Value),
                TokenType.Minus => new IntegerObject(l.Value - r.Value),
                TokenType.Asterisk => new IntegerObject(l.Value * r.Value),
                TokenType.Slash => new IntegerObject(l.Value / r.Value),
                TokenType.LT => BooleanObject.FromNative(l.Value < r.Value),
                TokenType.GT => BooleanObject.FromNative(l.Value > r.Value),
                TokenType.Eq => BooleanObject.FromNative(l.Value == r.Value),
                TokenType.Not_Eq => BooleanObject.FromNative(l.Value != r.Value),
                _ => throw new MonkeyEvaluatorException($"unknown operator: {l.Type} {op.GetDescription()} {r.Type}")
            };
        }

        private IObject EvaluateBooleanInfix(BooleanObject l, TokenType op, BooleanObject r)
        {
            return op switch
            {
                TokenType.Eq => BooleanObject.FromNative(l.Value == r.Value),
                TokenType.Not_Eq => BooleanObject.FromNative(l.Value != r.Value),
                _ => throw new MonkeyEvaluatorException($"unknown operator: {l.Type} {op.GetDescription()} {r.Type}")
            };
        }

        private IObject EvaluatePrefix(IObject right, TokenType op)
        {
            return op switch
            {
                TokenType.Bang => EvaluateUnaryNot(right),
                TokenType.Minus => EvaluateUnaryMinus(right),
                _ => throw new MonkeyEvaluatorException($"unknown operator: {op.GetDescription()}{right.Type}")
            };
        }

        private IObject EvaluateConditional(IObject condition, BlockStatement consequence, BlockStatement? alternative)
        {
            if (IsTruthy(condition))
            {
                return EvaluateStatements(consequence.Statements);
            }

            if (alternative != null)
            {
                return EvaluateStatements(alternative.Statements);
            }

            return NullObject.Null;
        }

        private bool IsTruthy(IObject condition)
        {
            return condition switch
            {
                BooleanObject booleanObj => booleanObj.Value,
                NullObject _ => false,
                _ => true
            };
        }

        private IObject EvaluateUnaryMinus(IObject right)
        {
            if (right is IntegerObject i) {
                return new IntegerObject(-i.Value);
            }
            throw new MonkeyEvaluatorException($"unknown operator: -{right.Type}");
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

        private IObject EvaluateStatements(IImmutableList<IStatement> statements, bool unwrapReturn = false)
        {
            IObject result = NullObject.Null;

            foreach (var item in statements)
            {
                result = Evaluate(item);

                if (result is ReturnValue returnVal)
                {
                    return unwrapReturn ? returnVal.Value : returnVal;
                }

                if (result is ErrorObject errorObj)
                {
                    return errorObj;
                }
            }

            return result;
        }
    }
}
