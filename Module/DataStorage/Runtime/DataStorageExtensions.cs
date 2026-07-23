namespace VirtueSky.DataStorage
{
    public static class DataStorageExtensions
    {
        public static T Get<T>(this IDataStorage storage, string key, T defaultValue = default)
        {
            return defaultValue switch
            {
                int i => (T)(object)storage.GetInt(key, i),
                float f => (T)(object)storage.GetFloat(key, f),
                bool b => (T)(object)storage.GetBool(key, b),
                string s => (T)(object)storage.GetString(key, s),
                _ => storage.GetObject(key, defaultValue)
            };
        }

        public static void Set<T>(this IDataStorage storage, string key, T value)
        {
            switch (value)
            {
                case int i: storage.SetInt(key, i); break;
                case float f: storage.SetFloat(key, f); break;
                case bool b: storage.SetBool(key, b); break;
                case string s: storage.SetString(key, s); break;
                default: storage.SetObject(key, value); break;
            }
        }
    }
}
