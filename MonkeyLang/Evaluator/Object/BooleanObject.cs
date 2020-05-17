using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MonkeyLang
{
    public class BooleanObject : IObject, IEquatable<IObject?>
    {
        public static BooleanObject True = new BooleanObject(true);
        public static BooleanObject False = new BooleanObject(false);

        private BooleanObject(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; }

        public ObjectType Type => ObjectType.Boolean;

        public string Inspect() => Value.ToString();

        internal static BooleanObject FromNative(bool value) => value ? True : False;

        public override bool Equals(object? obj)
        {
            return Equals(obj as BooleanObject);
        }

        public bool Equals(IObject? other) => Equals(other as BooleanObject);

        public bool Equals(BooleanObject? other)
        {
            return other != null &&
                   Value == other.Value &&
                   Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Type);
        }

        public static bool operator ==(BooleanObject? left, BooleanObject? right)
        {
            return EqualityComparer<BooleanObject>.Default.Equals(left, right);
        }

        public static bool operator !=(BooleanObject? left, BooleanObject? right)
        {
            return !(left == right);
        }
    }
}
