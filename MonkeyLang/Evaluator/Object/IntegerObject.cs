﻿namespace MonkeyLang
{
    public class IntegerObject : IObject
    {
        public IntegerObject(int value)
        {
            this.Value = value;
        }

        public int Value { get; }

        public ObjectType Type => ObjectType.Integer;

        public string Inspect() => Value.ToString();
    }
}