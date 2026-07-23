using Sabresaurus.UnityPrefsUtilities;
using UnityEngine;

namespace VirtueSky.DataStorage
{
    public class SecureStorageUtil: IDataStorage
    {
        private readonly IJsonService _jsonService;

        public SecureStorageUtil(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }

        public int GetInt(string key, int defaultValue = 0) => PlayerPrefsUtility.GetEncryptedInt(key, defaultValue);
        public void SetInt(string key, int value) => PlayerPrefsUtility.SetEncryptedInt(key, value);

        public float GetFloat(string key, float defaultValue = 0f) => PlayerPrefsUtility.GetEncryptedFloat(key, defaultValue);
        public void SetFloat(string key, float value) => PlayerPrefsUtility.SetEncryptedFloat(key, value);

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = GetInt(key, -1);
            if (value == -1) return defaultValue;
            return value > 0;
        }

        public void SetBool(string key, bool value) => SetInt(key, value ? 1 : 0);

        public string GetString(string key, string defaultValue = "") => PlayerPrefsUtility.GetEncryptedString(key, defaultValue);
        public void SetString(string key, string value) => PlayerPrefsUtility.SetEncryptedString(key, value);

        public T GetObject<T>(string key, T defaultValue = default)
        {
            string json = GetString(key, string.Empty);
            if (string.IsNullOrEmpty(json)) return defaultValue;
            return _jsonService.Deserialize<T>(json);
        }

        public void SetObject<T>(string key, T value) => SetString(key, _jsonService.Serialize(value));
        public void Save() => PlayerPrefs.Save();

        public void Load()
        {
        }

        public void Clear() => PlayerPrefs.DeleteAll();
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public bool ContainsKey(string key) => PlayerPrefs.HasKey(key);
    }
}