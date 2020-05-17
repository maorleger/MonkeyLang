using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public class BuiltIn : IObject
    {
        public BuiltIn(Func<IObject[], IObject> fn)
        {
            Fn = fn;
        }

        public ObjectType Type => ObjectType.BuiltIn;

        public Func<IObject[], IObject> Fn { get; }

        public string Inspect() => "builtin function";
        
        public static IObject BuiltInLen(IEnumerable<IObject> args)
        {
            if (args.Count() != 1)
            {
                throw new EvaluatorException($"wrong number of arguments. got={args.Count()}, want=1");
            }

            return args.First() switch
            {
                StringObject strObj => new IntegerObject(strObj.Value.Length),
                ArrayObject arrObj => new IntegerObject(arrObj.Elements.Count),
                _ => throw new EvaluatorException($"argument to \"len\" not supported, got {args.First().Type}")
            };
        }

        public static IObject BuiltInFirst(IEnumerable<IObject> args)
        {
            if (args.Count() != 1)
            {
                throw new EvaluatorException($"wrong number of arguments. got={args.Count()}, want=1");
            }

            return args.First() switch
            {
                ArrayObject arrObj => arrObj.Elements.FirstOrDefault() ?? NullObject.Null,
                _ => throw new EvaluatorException($"argument to \"first\" not supported, got {args.First().Type}")
            };
        }

        public static IObject BuiltInLast(IEnumerable<IObject> args)
        {
            if (args.Count() != 1)
            {
                throw new EvaluatorException($"wrong number of arguments. got={args.Count()}, want=1");
            }

            return args.First() switch
            {
                ArrayObject arrObj => arrObj.Elements.LastOrDefault() ?? NullObject.Null,
                _ => throw new EvaluatorException($"argument to \"last\" not supported, got {args.First().Type}")
            };
        }

        public static IObject BuiltInRest(IEnumerable<IObject> args)
        {
            if (args.Count() != 1)
            {
                throw new EvaluatorException($"wrong number of arguments. got={args.Count()}, want=1");
            }

            return args.First() switch
            {
                ArrayObject arrObj => new ArrayObject(arrObj.Elements.Skip(1)),
                _ => throw new EvaluatorException($"argument to \"rest\" not supported, got {args.First().Type}")
            };
        }

        public static IObject BuiltInPush(IEnumerable<IObject> args)
        {
            if (args.Count() != 2)
            {
                throw new EvaluatorException($"wrong number of arguments. got={args.Count()}, want=2");
            }

            return args.First() switch
            {
                ArrayObject arrObj => new ArrayObject(arrObj.Elements.Append(args.ElementAt(1))),
                _ => throw new EvaluatorException($"argument to \"push\" not supported, got {args.First().Type}")
            };
        }

        public static IObject BuiltInPuts(IEnumerable<IObject> args)
        {
            foreach (var item in args)
            {
                Console.WriteLine(item.Inspect());
            }

            return NullObject.Null;
        }
    }
}
