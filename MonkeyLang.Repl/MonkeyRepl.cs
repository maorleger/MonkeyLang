using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

namespace MonkeyLang.Repl
{
    [Export(typeof(MonkeyRepl))]
    public class MonkeyRepl
    {
        [ImportingConstructor]
        public MonkeyRepl([Import] Parser parser)
        {
            Parser = parser;
        }

        public Parser Parser { get; }

        public void Start()
        {
            Console.WriteLine("Hello {0}! This is the Monkey programming language!", Environment.UserName);
            Console.WriteLine("Feel free to type in commands");

            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                AST result = Parser.ParseProgram(line);
                if (result.HasErrors)
                {
                    PrintParserErrors(result.Errors);
                } 
                else
                {
                    Console.WriteLine(result.Program.StringValue);
                }
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
