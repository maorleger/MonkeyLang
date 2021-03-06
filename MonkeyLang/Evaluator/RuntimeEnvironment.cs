﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace MonkeyLang
{
    [Export]
    public class RuntimeEnvironment
    {
        public RuntimeEnvironment() : this(null) { }

        private RuntimeEnvironment(RuntimeEnvironment? parent)
        {
            this.Parent = parent;
            this.Store = new Dictionary<string, IObject>();
        }

        private RuntimeEnvironment? Parent { get; }
        private Dictionary<string, IObject> Store { get; }

        public IObject Set(string name, IObject value)
        {
            this.Store[name] = value;
            return value;
        }

        public IObject? Get(string name)
        {
            this.Store.TryGetValue(name, out IObject? value);
            value ??= this.Parent?.Get(name);
            return value;
        }

        public RuntimeEnvironment Extend() => new RuntimeEnvironment(this);

        public string Inspect()
        {
            var sb = new StringBuilder("Environment: ");
            sb.AppendLine();
            sb.AppendJoin(System.Environment.NewLine, this.Store.Select(kv => $"[{kv.Key}={kv.Value.Inspect()}]"));
            sb.AppendLine();
            if (this.Parent != null)
            {
                sb.AppendLine(this.Parent.Inspect());
            }
            return sb.ToString();
        }
    }
}
