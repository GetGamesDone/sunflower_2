using System;
using UnityEngine;
#if VIRTUESKY_FIREBASE_AUTH
using Firebase.Auth;
using Firebase.Extensions;
#endif

namespace VirtueSky.GameService
{
    public static class FirebaseAuthentication
    {
        public const string NotInstalledWarning =
            "[FirebaseAuth] not enabled. Turn on define symbol VIRTUESKY_FIREBASE_AUTH and install package " +
            "com.google.firebase.auth via Magic Panel > Firebase.";

        public static event Action<bool> OnSignInCompleted;

#if VIRTUESKY_FIREBASE_AUTH
        public static bool IsSignedIn => FirebaseAuth.DefaultInstance.CurrentUser != null;
        public static string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

        public static void SignInAnonymously()
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                Debug.Log($"[FirebaseAuth] already signed in: {auth.CurrentUser.UserId}");
                OnSignInCompleted?.Invoke(true);
                return;
            }

            auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"[FirebaseAuth] anonymous sign-in failed: {task.Exception}");
                    OnSignInCompleted?.Invoke(false);
                    return;
                }

                Debug.Log($"[FirebaseAuth] signed in anonymously: {task.Result.User.UserId}");
                OnSignInCompleted?.Invoke(true);
            });
        }

        public static void SignOut()
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
#else
        public static bool IsSignedIn => false;
        public static string UserId => null;

        public static void SignInAnonymously()
        {
            Debug.LogWarning(NotInstalledWarning);
            OnSignInCompleted?.Invoke(false);
        }

        public static void SignOut()
        {
            Debug.LogWarning(NotInstalledWarning);
        }
#endif
    }
}
