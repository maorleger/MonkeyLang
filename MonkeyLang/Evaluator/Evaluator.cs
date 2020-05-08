using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection.Metadata;
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
            Environments = new Stack<RuntimeEnvironment>();
            Environments.Push(new RuntimeEnvironment());
            CurrentEnvironment.Set("len", new BuiltIn(BuiltIn.BuiltInLen));
        }

        private Parser Parser { get; }
        private Stack<RuntimeEnvironment> Environments { get; }

        private RuntimeEnvironment CurrentEnvironment => Environments.Peek();

        public IObject Evaluate(string input)
        {
            AST result = Parser.ParseProgram(input);
            if (result.HasErrors)
            {
                return new ErrorObject(result.Errors.Select(m => m.Message));
            }

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
                    StringLiteral strLiteral => new StringObject(strLiteral.Value),
                    Boolean boolean => BooleanObject.FromNative(boolean.Value),
                    PrefixExpression prefixExpr => EvaluatePrefix(Evaluate(prefixExpr.Right), prefixExpr.Operator),
                    InfixExpression infixExpr => EvaluateInfix(Evaluate(infixExpr.Left), infixExpr.Operator, Evaluate(infixExpr.Right)),
                    IfExpression ifExpr => EvaluateConditional(Evaluate(ifExpr.Condition), ifExpr.Consequence, ifExpr.Alternative),
                    ReturnStatement returnStmt => new ReturnValue(Evaluate(returnStmt.ReturnValue)),
                    LetStatement letStmt => EvaluateLet(letStmt.Name, Evaluate(letStmt.Value)),
                    Identifier ident => EvaluateIdentifier(ident),
                    FunctionLiteral fnLiteral => new FunctionObject(fnLiteral.Parameters, fnLiteral.Body, CurrentEnvironment),
                    CallExpression callExpr => EvaluateCallExpression(callExpr.Function, callExpr.Arguments),
                    BlockStatement blockStmt => EvaluateStatements(blockStmt.Statements, unwrapReturn: true),
                    _ => NullObject.Null
                };
            }
            catch (EvaluatorException ex)
            {
                return new ErrorObject(new[] { ex.Message });
            }
        }

        private IObject EvaluateIdentifier(Identifier ident)
        {
            var result = CurrentEnvironment.Get(ident.Value);

            if (result == null)
            {
                throw new EvaluatorException($"identifier not found: {ident.StringValue}");
            }

            return result;
        }

        private IObject EvaluateLet(Identifier name, IObject value)
        {
            CurrentEnvironment.Set(name.Value, value);
            return value;
        }

        private IObject EvaluateInfix(IObject left, TokenType op, IObject right)
        {
            return (left, right) switch
            {
                (IntegerObject li, IntegerObject ri) => EvaluateIntegerInfix(li, op, ri),
                (BooleanObject lb, BooleanObject rb) => EvaluateBooleanInfix(lb, op, rb),
                (StringObject ls, StringObject rs) => EvaluateStringInfix(ls, op, rs),
                _ => throw new EvaluatorException($"type mismatch: {left.Type} {op.GetDescription()} {right.Type}")
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
                _ => throw new EvaluatorException($"unknown operator: {l.Type} {op.GetDescription()} {r.Type}")
            };
        }

        private IObject EvaluateBooleanInfix(BooleanObject l, TokenType op, BooleanObject r)
        {
            return op switch
            {
                TokenType.Eq => BooleanObject.FromNative(l.Value == r.Value),
                TokenType.Not_Eq => BooleanObject.FromNative(l.Value != r.Value),
                _ => throw new EvaluatorException($"unknown operator: {l.Type} {op.GetDescription()} {r.Type}")
            };
        }

        private IObject EvaluateStringInfix(StringObject l, TokenType op, StringObject r)
        {
            if (op == TokenType.Plus)
            {
                return new StringObject(l.Value + r.Value);
            }

            throw new EvaluatorException($"unknown operator: {l.Type} {op.GetDescription()} {r.Type}");
        }

        private IObject EvaluatePrefix(IObject right, TokenType op)
        {
            return op switch
            {
                TokenType.Bang => EvaluateUnaryNot(right),
                TokenType.Minus => EvaluateUnaryMinus(right),
                _ => throw new EvaluatorException($"unknown operator: {op.GetDescription()}{right.Type}")
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
            throw new EvaluatorException($"unknown operator: -{right.Type}");
        }

        private IObject EvaluateUnaryNot(IObject right)
        {
            return IsTruthy(right) ? BooleanObject.False : BooleanObject.True;
        }

        private IObject EvaluateCallExpression(IExpression fn, IImmutableList<IExpression> arguments)
        {
            IObject value = Evaluate(fn);

            if (value is FunctionObject fnObject)
            {
                Environments.Push(fnObject.Environment.Extend());

                var bindings = fnObject.Parameters.Zip(arguments.Select(Evaluate));
                foreach ((Identifier parameter, IObject argument) in bindings)
                {
                    CurrentEnvironment.Set(parameter.Value, argument);
                }

                IObject result = Evaluate(fnObject.Body);

                Environments.Pop();
                return result;
            }
            else if (value is BuiltIn builtInObject)
            {
                return builtInObject.Fn(arguments.Select(Evaluate).ToArray());
            }
            throw new EvaluatorException($"undefined local variable or method {fn.StringValue}");
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
