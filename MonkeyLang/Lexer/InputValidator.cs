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

        public string ExtractString()
        {
            string returnVal = Input.ToString();
            Input.Clear();
            Lexer.Clear();
            return returnVal;
        }

        public void AppendLine(string line)
        {
            Input.Append(line);
            Lexer.Tokenize(line);
        }
    }
}
