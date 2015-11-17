namespace Orchestrate.Io.Extensibility
{
    public interface IJsonSerializer
    {
        string SerializeObject(object item);
        T DeserializeObject<T>(string json);
    }
}