using System;
using System.Collections.Generic;

namespace MonkeyLang
{
    public class IntegerObject : IObject, IEquatable<IntegerObject?>
    {
        public IntegerObject(int value)
        {
            this.Value = value;
        }

        public int Value { get; }

        public ObjectType Type => ObjectType.Integer;

        public string Inspect() => Value.ToString();

        public override bool Equals(object? obj)
        {
            return Equals(obj as IntegerObject);
        }

        public bool Equals(IntegerObject? other)
        {
            return other != null &&
                   Value == other.Value &&
                   Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Type);
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
