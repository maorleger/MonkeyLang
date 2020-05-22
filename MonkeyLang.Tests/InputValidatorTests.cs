using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MonkeyLang.Tests
{
    public class InputValidatorTests
    {
        private readonly InputValidator subject;

        public InputValidatorTests()
        {
            subject = new InputValidator(new Lexer());
        }

        [Fact]
        public void ShouldParse_BasicTests()
        {
            subject.AppendLine("hello");
            subject.AppendLine("world");
            Assert.True(subject.ShouldParse());

            subject.AppendLine("[");
            Assert.False(subject.ShouldParse());
        }

        [Fact]
        public void ShouldParse_WhenStringIsMalformed_ReturnsTrue()
        {
            subject.AppendLine("]");
            Assert.True(subject.ShouldParse());
        }

        [Fact]
        public void GetInput_ReturnsTheCompleteString()
        {
            subject.AppendLine("[");
            Assert.Equal("[", subject.GetInput());
            subject.AppendLine("abc");
            Assert.Equal("[abc", subject.GetInput());
        }

        [Fact]
        public void Clear_ResetsInternalState()
        {
            subject.AppendLine("[");
            subject.Clear();
            Assert.Equal(string.Empty, subject.GetInput());
        }
    }
}
