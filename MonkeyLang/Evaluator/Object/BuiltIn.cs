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
                _ => throw new EvaluatorException($"argument to \"len\" not supported, got {args.First().Type}")
            };
        }
    }
}
