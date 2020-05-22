using Xunit;

namespace MonkeyLang.Tests
{
    public class EnvironmentTests
    {
        [Fact]
        public void Get_WhenValuePresent_CanReturnValue()
        {
            IObject obj = new IntegerObject(5);
            var subject = new RuntimeEnvironment();
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueIsNotPresent_SearchesParents()
        {
            IObject obj = new IntegerObject(5);
            var subject = new RuntimeEnvironment();
            subject.Set("a", obj);
            subject = subject.Extend();
            subject = subject.Extend();
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenShadowingOuterValue_ReturnsCorrectValue()
        {
            IObject obj = new IntegerObject(10);
            var subject = new RuntimeEnvironment();
            subject.Set("a", new IntegerObject(5));
            subject = subject.Extend();
            subject = subject.Extend();
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueNotPresent_ReturnsNull()
        {
            var subject = new RuntimeEnvironment();
            subject = subject.Extend();
            subject = subject.Extend();
            Assert.Null(subject.Get("a"));
        }
    }
}
