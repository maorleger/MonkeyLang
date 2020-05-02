using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyLang
{
    public interface IEnvironment
    {

    }

    public class MonkeyEnvironment : IEnvironment
    {
        public MonkeyEnvironment()
        {
            this.Store = new Dictionary<string, IObject>();
        }

        public Dictionary<string, IObject> Store { get; }

        public IObject Set(string name, IObject value)
        {
            Store[name] = value;
            return value;
        } 

        public IObject? Get(string name)
        {
            Store.TryGetValue(name, out IObject? value);
            return value;
        }
    }
}
