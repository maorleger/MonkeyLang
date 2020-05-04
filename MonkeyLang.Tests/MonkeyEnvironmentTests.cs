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
            Environment subject = new Environment();
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueIsNotPresent_SearchesParents()
        {
            IObject obj = new IntegerObject(5);
            Environment subject = new Environment();
            subject.Set("a", obj);
            subject = new Environment(subject);
            subject = new Environment(subject);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenShadowingOuterValue_ReturnsCorrectValue()
        {
            IObject obj = new IntegerObject(10);
            Environment subject = new Environment();
            subject.Set("a", new IntegerObject(5));
            subject = new Environment(subject);
            subject = new Environment(subject);
            subject.Set("a", obj);
            Assert.Equal(obj, subject.Get("a"));
        }

        [Fact]
        public void Get_WhenValueNotPresent_ReturnsNull()
        {
            Environment subject = new Environment();
            subject = new Environment(subject);
            subject = new Environment(subject);
            Assert.Null(subject.Get("a"));
        }
    }
}
