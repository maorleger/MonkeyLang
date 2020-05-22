using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;

namespace MonkeyLang
{
    [Export]
    public class InputValidator
    {
        [ImportingConstructor]
        public InputValidator([Import] Lexer lexer)
        {
            Lexer = lexer;
            this.Input = new StringBuilder();
        }

        private Lexer Lexer { get; }
        private StringBuilder Input { get; set; }

        public bool ShouldParse() => Lexer.ShouldParse();

        public string GetInput() => Input.ToString();

        public void Clear()
        {
            Input.Clear();
            Lexer.Clear();
        }

        public void AppendLine(string line)
        {
            Input.Append(line);
            Lexer.Tokenize(line);
        }
    }
}
