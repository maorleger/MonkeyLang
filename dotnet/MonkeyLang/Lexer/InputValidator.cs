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
            this.Lexer = lexer;
            this.Input = new StringBuilder();
        }

        private Lexer Lexer { get; }
        private StringBuilder Input { get; set; }

        public bool ShouldParse() => this.Lexer.ShouldParse();

        public string GetInput() => this.Input.ToString();

        public void Clear()
        {
            this.Input.Clear();
            this.Lexer.Clear();
        }

        public void AppendLine(string line)
        {
            this.Input.Append(line);
            this.Lexer.Tokenize(line);
        }
    }
}
