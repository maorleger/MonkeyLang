namespace MonkeyLang
{
    public class NullObject : IObject
    {
        public static NullObject Null = new NullObject();

        private NullObject()
        {
        }

        public ObjectType Type => ObjectType.Null;

        public string Inspect() => "null";
    }
}
