using UnityEngine;

namespace VirtueSky.DataStorage
{
    public class PlayerPrefStorage : IDataStorage
    {
        private readonly IJsonService _jsonService;

        public PlayerPrefStorage(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public int GetInt(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = PlayerPrefs.GetInt(key, -1);
            if (value == -1) return defaultValue;
            return value > 0;
        }

        public void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

        public string GetString(string key, string defaultValue = "") => PlayerPrefs.GetString(key, defaultValue);
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public T GetObject<T>(string key, T defaultValue = default)
        {
            if (!PlayerPrefs.HasKey(key)) return defaultValue;
            string json = PlayerPrefs.GetString(key);
            return string.IsNullOrEmpty(json) ? defaultValue : _jsonService.Deserialize<T>(json);
        }

        public void SetObject<T>(string key, T value) => PlayerPrefs.SetString(key, _jsonService.Serialize(value));
        public void Save() => PlayerPrefs.Save();

        public void Load()
        {
        }

        public void Clear() => PlayerPrefs.DeleteAll();
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public bool ContainsKey(string key) => PlayerPrefs.HasKey(key);
    }
}