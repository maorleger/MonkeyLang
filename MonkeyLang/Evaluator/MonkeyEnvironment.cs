using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    public interface IEnvironment
    {
        IObject? Get(string name);
        IObject Set(string name, IObject value);
        IEnvironment Extend();
        string Inspect();
    }

    [Export(typeof(IEnvironment))]
    public class MonkeyEnvironment : IEnvironment
    {
        public MonkeyEnvironment() : this(null) { }

        public MonkeyEnvironment(IEnvironment? parent)
        {
            this.Parent = parent;
            this.Store = new Dictionary<string, IObject>();
        }

        private IEnvironment? Parent { get; }
        private Dictionary<string, IObject> Store { get; }

        public IObject Set(string name, IObject value)
        {
            Store[name] = value;
            return value;
        } 

        public IObject? Get(string name)
        {
            Store.TryGetValue(name, out IObject? value);
            value ??= Parent?.Get(name);
            return value;
        }

        public IEnvironment Extend()
        {
            var extendedEnv = new MonkeyEnvironment(this);
            foreach (var boundItem in Store)
            {
                extendedEnv.Set(boundItem.Key, boundItem.Value);
            }
            return extendedEnv;
        }

        public string Inspect()
        {
            StringBuilder sb = new StringBuilder("Environment: ");
            sb.AppendLine();
            sb.AppendJoin(Environment.NewLine, Store.Select(kv => $"[{kv.Key}={kv.Value.Inspect()}]"));
            sb.AppendLine();
            if (Parent != null)
            {
                sb.AppendLine(Parent.Inspect());
            }
            return sb.ToString();
        }
    }
}
