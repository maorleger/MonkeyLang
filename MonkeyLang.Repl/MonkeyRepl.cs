using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace MonkeyLang.Repl
{
    [Export(typeof(MonkeyRepl))]
    public class MonkeyRepl
    {
        [ImportingConstructor]
        public MonkeyRepl([Import] Evaluator evaluator, [Import] Parser parser)
        {
            Evaluator = evaluator;
            Parser = parser; //TODO: get rid of this when evaluator can display errors
        }

        public Evaluator Evaluator { get; }
        public Parser Parser { get; }

        public void Start()
        {
            Console.WriteLine("Hello {0}! This is the Monkey programming language!", System.Environment.UserName);
            Console.WriteLine("Feel free to type in commands");

            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                Console.WriteLine(Evaluator.Evaluate(line).Inspect());
            }
        }

        private void PrintParserErrors(IImmutableList<MonkeyParseException> errors)
        {
            Console.WriteLine("Woops! We ran into some monkey business here!");
            Console.WriteLine("Parser errors: ");

            foreach (var error in errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }
}
