namespace VirtueSky.DataStorage
{
    public class SecureStorage: IDataStorage
    {
        private readonly IJsonService _jsonService;

        public SecureStorage(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public int GetInt(string key, int defaultValue = 0) => SecurityPlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => SecurityPlayerPrefs.SetInt(key, value);

        public float GetFloat(string key, float defaultValue = 0f) => SecurityPlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => SecurityPlayerPrefs.SetFloat(key, value);

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = SecurityPlayerPrefs.GetInt(key, -1);
            if (value == -1) return defaultValue;
            return value > 0;
        }

        public void SetBool(string key, bool value) => SecurityPlayerPrefs.SetInt(key, value ? 1 : 0);

        public string GetString(string key, string defaultValue = "") => SecurityPlayerPrefs.GetString(key, defaultValue);
        public void SetString(string key, string value) => SecurityPlayerPrefs.SetString(key, value);

        public T GetObject<T>(string key, T defaultValue = default)
        {
            string json = GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return defaultValue;
            return _jsonService.Deserialize<T>(json);
        }

        public void SetObject<T>(string key, T value) => SetString(key, _jsonService.Serialize(value));
        public void Save() => SecurityPlayerPrefs.Save();

        public void Load()
        {
        }

        public void Clear() => SecurityPlayerPrefs.DeleteAll();
        public void DeleteKey(string key) => SecurityPlayerPrefs.DeleteKey(key);
        public bool ContainsKey(string key) => SecurityPlayerPrefs.HasKey(key);
    }
}