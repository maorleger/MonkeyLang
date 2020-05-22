using Xunit;

namespace MonkeyLang.Tests
{
    public class InputValidatorTests
    {
        private readonly InputValidator subject;

        public InputValidatorTests()
        {
            this.subject = new InputValidator(new Lexer());
        }

        [Fact]
        public void ShouldParse_BasicTests()
        {
            this.subject.AppendLine("hello");
            this.subject.AppendLine("world");
            Assert.True(this.subject.ShouldParse());

            this.subject.AppendLine("[");
            Assert.False(this.subject.ShouldParse());
        }

        [Fact]
        public void ShouldParse_WhenStringIsMalformed_ReturnsTrue()
        {
            this.subject.AppendLine("]");
            Assert.True(this.subject.ShouldParse());
        }

        [Fact]
        public void GetInput_ReturnsTheCompleteString()
        {
            this.subject.AppendLine("[");
            Assert.Equal("[", this.subject.GetInput());
            this.subject.AppendLine("abc");
            Assert.Equal("[abc", this.subject.GetInput());
        }

        [Fact]
        public void Clear_ResetsInternalState()
        {
            this.subject.AppendLine("[");
            this.subject.Clear();
            Assert.Equal(string.Empty, this.subject.GetInput());
        }
    }
}
