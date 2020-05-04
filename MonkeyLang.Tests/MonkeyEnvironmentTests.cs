using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MonkeyLang.Tests
{
    public class MonkeyEnvironmentTests
    {
        [Fact]
        public void Get_WhenValuePresent_CanReturnValue()
        {
            IObject obj = new IntegerObject(5);
            MonkeyEnvironment subject = new MonkeyEnvironment();
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueIsNotPresent_SearchesParents()
        {
            IObject obj = new IntegerObject(5);
            MonkeyEnvironment subject = new MonkeyEnvironment();
            subject.Set("a", obj);
            subject = new MonkeyEnvironment(subject);
            subject = new MonkeyEnvironment(subject);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenShadowingOuterValue_ReturnsCorrectValue()
        {
            IObject obj = new IntegerObject(10);
            MonkeyEnvironment subject = new MonkeyEnvironment();
            subject.Set("a", new IntegerObject(5));
            subject = new MonkeyEnvironment(subject);
            subject = new MonkeyEnvironment(subject);
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueNotPresent_ReturnsNull()
        {
            MonkeyEnvironment subject = new MonkeyEnvironment();
            subject = new MonkeyEnvironment(subject);
            subject = new MonkeyEnvironment(subject);
            Assert.Null(subject.Get("a"));
        }
    }
}
