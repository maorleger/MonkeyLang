using System;
using System.Collections.Generic;

namespace MonkeyLang
{
    public class IntegerObject : IObject, IEquatable<IObject?>
    {
        public IntegerObject(int value)
        {
            this.Value = value;
        }

        public int Value { get; }

        public ObjectType Type => ObjectType.Integer;

        public string Inspect() => this.Value.ToString();

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as IntegerObject);
        }

        public bool Equals(IObject? other) => this.Equals(other as IntegerObject);

        public bool Equals(IntegerObject? other)
        {
            return other != null && this.Value == other.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Value);
        }

        public static bool operator ==(IntegerObject? left, IntegerObject? right)
        {
            return EqualityComparer<IntegerObject>.Default.Equals(left, right);
        }

        public static bool operator !=(IntegerObject? left, IntegerObject? right)
        {
            return !(left == right);
        }
    }
}
