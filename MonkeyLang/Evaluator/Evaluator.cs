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
        }

        private Parser Parser { get; }

        public IObject Evaluate(string input, RuntimeEnvironment environment)
        {
            AST result = Parser.ParseProgram(input);
            if (result.HasErrors)
            {
                return new ErrorObject(result.Errors.Select(m => m.Message));
            }

            environment.Set("len", new BuiltIn(BuiltIn.BuiltInLen));
            environment.Set("first", new BuiltIn(BuiltIn.BuiltInFirst));
            environment.Set("last", new BuiltIn(BuiltIn.BuiltInLast));
            environment.Set("rest", new BuiltIn(BuiltIn.BuiltInRest));
            environment.Set("push", new BuiltIn(BuiltIn.BuiltInPush));
            environment.Set("puts", new BuiltIn(BuiltIn.BuiltInPuts));
            return Evaluate(result.Program, environment);
        }

        private IObject Evaluate(INode node, RuntimeEnvironment environment)
        {
            try
            {
                return node switch
                {
                    Program program => EvaluateStatements(program.Statements, environment, unwrapReturn: true),
                    ExpressionStatement exprStatement => Evaluate(exprStatement.Expression, environment),
                    IntegerLiteral intLiteral => new IntegerObject(intLiteral.Value),
                    StringLiteral strLiteral => new StringObject(strLiteral.Value),
                    Boolean boolean => BooleanObject.FromNative(boolean.Value),
                    PrefixExpression prefixExpr => EvaluatePrefix(Evaluate(prefixExpr.Right, environment), prefixExpr.Operator),
                    InfixExpression infixExpr => EvaluateInfix(Evaluate(infixExpr.Left, environment), infixExpr.Operator, Evaluate(infixExpr.Right, environment)),
                    IfExpression ifExpr => EvaluateConditional(Evaluate(ifExpr.Condition, environment), ifExpr.Consequence, ifExpr.Alternative, environment),
                    ReturnStatement returnStmt => new ReturnValue(Evaluate(returnStmt.ReturnValue, environment)),
                    LetStatement letStmt => EvaluateLet(letStmt.Name, Evaluate(letStmt.Value, environment), environment),
                    Identifier ident => EvaluateIdentifier(ident, environment),
                    FunctionLiteral fnLiteral => new FunctionObject(fnLiteral.Parameters, fnLiteral.Body, environment),
                    CallExpression callExpr => EvaluateCallExpression(callExpr.Function, callExpr.Arguments, environment),
                    BlockStatement blockStmt => EvaluateStatements(blockStmt.Statements, environment, unwrapReturn: true),
                    ArrayLiteral arrayLiteral => new ArrayObject(arrayLiteral.Elements.Select(e => Evaluate(e, environment))),
                    IndexExpression indexExpression => EvaluateIndexExpression(Evaluate(indexExpression.Left, environment), Evaluate(indexExpression.Index, environment)),
                    HashLiteral hashLiteral => EvaluateHashLiteral(hashLiteral, environment),
                    _ => NullObject.Null
                };
            }
            catch (EvaluatorException ex)
            {
                return new ErrorObject(new[] { ex.Message });
            }
        }

        private IObject EvaluateIdentifier(Identifier ident, RuntimeEnvironment environment)
        {
            var result = environment.Get(ident.Value);

            if (result == null)
            {
                throw new EvaluatorException($"identifier not found: {ident.StringValue}");
            }

            return result;
        }

        private IObject EvaluateLet(Identifier name, IObject value, RuntimeEnvironment environment)
        {
            environment.Set(name.Value, value);
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

        private IObject EvaluateConditional(IObject condition, BlockStatement consequence, BlockStatement? alternative, RuntimeEnvironment environment)
        {
            if (IsTruthy(condition))
            {
                return EvaluateStatements(consequence.Statements, environment);
            }

            if (alternative != null)
            {
                return EvaluateStatements(alternative.Statements, environment);
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

        private IObject EvaluateCallExpression(IExpression fn, IImmutableList<IExpression> arguments, RuntimeEnvironment environment)
        {
            IObject value = Evaluate(fn, environment);
            var resolvedArguments = arguments.Select(a => Evaluate(a, environment)).ToArray();

            if (value is FunctionObject fnObject)
            {
                var extendedEnv = fnObject.Environment.Extend();

                var bindings = fnObject.Parameters.Zip(resolvedArguments);
                foreach ((Identifier parameter, IObject argument) in bindings)
                {
                    extendedEnv.Set(parameter.Value, argument);
                }

                IObject result = Evaluate(fnObject.Body, extendedEnv);

                return result;
            }
            else if (value is BuiltIn builtInObject)
            {
                return builtInObject.Fn(resolvedArguments);
            }
            throw new EvaluatorException($"undefined local variable or method {fn.StringValue}");
        }

        private IObject EvaluateIndexExpression(IObject left, IObject index)
        {
            return (left, index) switch
            {
                (ArrayObject arr, IntegerObject idx) => EvaluateArrayIndexExpression(arr, idx),
                (HashObject hash, _) => EvaluateHashIndexExpression(hash, index),
                _ => throw new EvaluatorException($"index operator not supported: {left.Type.GetDescription()}")
            };
        }

        private IObject EvaluateHashIndexExpression(HashObject hash, IObject key)
        {
            return hash.Pairs.GetValueOrDefault(key) ?? NullObject.Null;
        }

        private IObject EvaluateArrayIndexExpression(ArrayObject arr, IntegerObject idx)
        {
            if (idx.Value < 0 || idx.Value > (arr.Elements.Count - 1))
            {
                return NullObject.Null;
            }

            return arr.Elements[idx.Value];
        }

        private IObject EvaluateHashLiteral(HashLiteral hashLiteral, RuntimeEnvironment environment)
        {
            Dictionary<IObject, IObject> evaluatedPairs = new Dictionary<IObject, IObject>();
            foreach (var item in hashLiteral.Pairs)
            {
                var key = Evaluate(item.Key, environment);

                if (!(key is IEquatable<IObject?>))
                {
                    throw new EvaluatorException($"unusable as hash key: {key.Type}");
                }

                var value = Evaluate(item.Value, environment);

                evaluatedPairs[key] = Evaluate(item.Value, environment);
            }

            return new HashObject(evaluatedPairs);
        }

        private IObject EvaluateStatements(IImmutableList<IStatement> statements, RuntimeEnvironment environment, bool unwrapReturn = false)
        {
            IObject result = NullObject.Null;

            foreach (var item in statements)
            {
                result = Evaluate(item, environment);

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
