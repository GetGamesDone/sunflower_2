using Newtonsoft.Json;

namespace VirtueSky.DataStorage
{
    public class NewtonsoftJsonService : IJsonService
    {
        public string Serialize<T>(T value) => JsonConvert.SerializeObject(value);

        public T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
    }
}
