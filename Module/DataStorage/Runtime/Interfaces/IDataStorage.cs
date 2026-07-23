namespace VirtueSky.DataStorage
{
    public interface IDataStorage
    {
        public int GetInt(string key, int defaultValue);
        public void SetInt(string key, int value);
        public string GetString(string key, string defaultValue);
        public void SetString(string key, string value);
        public bool GetBool(string key, bool defaultValue);
        public void SetBool(string key, bool value);
        public float GetFloat(string key, float defaultValue);
        public void SetFloat(string key, float value);
        public T GetObject<T>(string key, T defaultValue);
        public void SetObject<T>(string key, T value);
        public void Save();
        public void Load();
        public void Clear();
        public void DeleteKey(string key);
        public bool ContainsKey(string key);
    }
}