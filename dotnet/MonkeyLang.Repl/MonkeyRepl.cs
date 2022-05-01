using System;
using System.ComponentModel.Composition;

namespace MonkeyLang.Repl
{
    [Export(typeof(MonkeyRepl))]
    public class MonkeyRepl
    {
        [ImportingConstructor]
        public MonkeyRepl(
            [Import] Evaluator evaluator,
            [Import] InputValidator validator,
            [Import] RuntimeEnvironment runtimeEnvironment)
        {
            this.Evaluator = evaluator;
            this.Environment = runtimeEnvironment;
            this.Validator = validator;
        }

        private Evaluator Evaluator { get; }
        private RuntimeEnvironment Environment { get; }
        private InputValidator Validator { get; }

        public void Start()
        {
            Console.WriteLine("Hello {0}! This is the Monkey programming language!", System.Environment.UserName);
            Console.WriteLine("Feel free to type in commands");

            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                this.Validator.AppendLine(line);
                if (this.Validator.ShouldParse())
                {
                    string userInput = this.Validator.GetInput();
                    this.Validator.Clear();

                    Console.WriteLine(this.Evaluator.Evaluate(userInput, this.Environment).Inspect());
                }
            }
        }
    }
}
