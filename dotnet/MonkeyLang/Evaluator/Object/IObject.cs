namespace MonkeyLang
{
    public interface IObject
    {
        ObjectType Type { get; }
        string Inspect();
    }
}
