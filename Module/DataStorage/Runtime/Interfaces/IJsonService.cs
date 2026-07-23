namespace VirtueSky.DataStorage
{
    public interface IJsonService
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string json);
    }
}