namespace MonkeyLang
{
    public class ReturnValue : IObject
    {
        public ReturnValue(IObject value)
        {
            this.Value = value;
        }

        public IObject Value { get; }

        public ObjectType Type => ObjectType.Return;

        public string Inspect() => this.Value.Inspect();
    }
}
