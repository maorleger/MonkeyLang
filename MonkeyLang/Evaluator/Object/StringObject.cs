using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MonkeyLang
{
    public class StringObject : IObject, IEquatable<IObject?>
    {
        public StringObject(string value)
        {
            this.Value = value;
        }

        public string Value { get; }

        public ObjectType Type => ObjectType.String;

        public string Inspect() => Value;

        public override bool Equals(object? obj)
        {
            return Equals(obj as StringObject);
        }

        public bool Equals(IObject? other) => Equals(other as StringObject);

        public bool Equals(StringObject? other)
        {
            return other != null && Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value);
        }

        public static bool operator ==(StringObject? left, StringObject? right)
        {
            return EqualityComparer<StringObject>.Default.Equals(left, right);
        }

        public static bool operator !=(StringObject? left, StringObject? right)
        {
            return !(left == right);
        }
    }
}
