namespace PoeHUD.Poe.Elements
{
    using RemoteMemoryObjects;

    public class EntityLabel : Element
    {
        public string Text => NativeStringReader.ReadString(Address + 0xC88);
    }
}