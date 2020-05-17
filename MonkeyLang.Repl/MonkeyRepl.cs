using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace MonkeyLang.Repl
{
    [Export(typeof(MonkeyRepl))]
    public class MonkeyRepl
    {
        [ImportingConstructor]
        public MonkeyRepl([Import] Evaluator evaluator, [Import] RuntimeEnvironment runtimeEnvironment)
        {
            Evaluator = evaluator;
            Environment = runtimeEnvironment;
        }

        private Evaluator Evaluator { get; }
        private RuntimeEnvironment Environment { get; }

        public void Start()
        {
            Console.WriteLine("Hello {0}! This is the Monkey programming language!", System.Environment.UserName);
            Console.WriteLine("Feel free to type in commands");

            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                Console.WriteLine(Evaluator.Evaluate(line, Environment).Inspect());
            }
        }
    }
}
