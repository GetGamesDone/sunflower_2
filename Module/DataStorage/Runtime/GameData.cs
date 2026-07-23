using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace VirtueSky.DataStorage
{
    public static class GameData
    {
        private static bool isInitialized;
        // private static Dictionary<string, byte[]> datas = new();
        private const int INIT_SIZE = 64;
        public static bool IsAutoSave { get; set; } = true;
        public static event Action OnSaveEvent;
        
        private static IDataStorage _storage;

        #region Internal Stuff

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Init(IDataStorage storage)
        {
            if (isInitialized) return;
            isInitialized = true;
            _storage = storage;
            // Load();
        }

        #endregion

        #region Public API

        public static bool IsInitialized => isInitialized;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Save()
        {
            OnSaveEvent?.Invoke();

            _storage.Save();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Load()
        {
            _storage.Load();
            // if (!File.Exists(GetPath))
            // {
            //     var stream = File.Create(GetPath);
            //     stream.Close();
            // }
            //
            // byte[] bytes = File.ReadAllBytes(GetPath);
            // if (bytes.Length == 0)
            // {
            //     datas.Clear();
            //     return;
            // }
            //
            // datas = Deserialize<Dictionary<string, byte[]>>(bytes) ?? new Dictionary<string, byte[]>(INIT_SIZE);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="default">If value of <paramref name="key"/> can not be found or empty! will return the default value of data type!</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(string key, T @default = default)
        {
            return @default switch
            {
                int i => (T)(object)_storage.GetInt(key, i),
                float f => (T)(object)_storage.GetFloat(key, f),
                bool b => (T)(object)_storage.GetBool(key, b),
                string s => (T)(object)_storage.GetString(key, s),
                _ => _storage.GetObject(key, @default)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<T>(string key, T data)
        {
            switch (data)
            {
                case int i: _storage.SetInt(key, i); break;
                case float f: _storage.SetFloat(key, f); break;
                case bool b: _storage.SetBool(key, b); break;
                case string s: _storage.SetString(key, s); break;
                default: _storage.SetObject(key, data); break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasKey(string key) => _storage.ContainsKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteKey(string key) => _storage.DeleteKey(key);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeleteAll() => _storage.Clear();
        #endregion
    }
}