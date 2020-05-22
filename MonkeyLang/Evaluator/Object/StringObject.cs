using System;
using System.Collections.Generic;

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

        public string Inspect() => this.Value;

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as StringObject);
        }

        public bool Equals(IObject? other) => this.Equals(other as StringObject);

        public bool Equals(StringObject? other)
        {
            return other != null && this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Value);
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
