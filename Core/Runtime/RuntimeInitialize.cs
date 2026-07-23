using UnityEngine;
using VirtueSky.DataStorage;

namespace VirtueSky.Core
{
    public struct RuntimeInitialize
    {
        public static bool IsInitializedMonoGlobal { get; set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            IsInitializedMonoGlobal = false;
            var app = new GameObject("MonoGlobal");
            App.InitMonoGlobalComponent(app.AddComponent<MonoGlobal>());
            IsInitializedMonoGlobal = true;
            Object.DontDestroyOnLoad(app);
            InitBinding();
        }

        private static void InitBinding()
        {
            var jsonService = new NewtonsoftJsonService();
            var normalDataService = new PlayerPrefStorage(jsonService);
            #if !UNITY_EDITOR
            var secureDataService = new SecureStorage(jsonService);
            #else
            var secureDataService = new SecureStorageUtil(jsonService);
            #endif
            GameData.Init(normalDataService, jsonService);
            GameDataSecure.Init(secureDataService, jsonService);
        }
    }
}