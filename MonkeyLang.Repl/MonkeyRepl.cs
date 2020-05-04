using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace MonkeyLang.Repl
{
    [Export(typeof(MonkeyRepl))]
    public class MonkeyRepl
    {
        [ImportingConstructor]
        public MonkeyRepl([Import] Evaluator evaluator)
        {
            Evaluator = evaluator;
        }

        public Evaluator Evaluator { get; }

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
    }
}
