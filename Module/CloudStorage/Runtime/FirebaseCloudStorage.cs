using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if VIRTUESKY_FIREBASE_STORAGE
using Firebase.Storage;
#endif

namespace VirtueSky.CloudStorage
{
    /// Firebase Storage-backed IRemoteStorage, shared by every feature that pulls files
    /// from the project's Storage bucket. Each feature constructs its own instance with
    /// its own root path (e.g. "levels/ZooTrail", "leaderboards") instead of duplicating
    /// the SDK call, the size cap and the error handling.
    ///
    /// The Firebase Unity SDK talks to the same backend on both Android and iOS, so there
    /// is no platform-specific code here - platform differences live inside the SDK's
    /// native plugins.
    ///
    /// Requires the "VIRTUESKY_FIREBASE_STORAGE" scripting define (alongside the existing
    /// "VIRTUESKY_FIREBASE") and FirebaseApp to already be initialized elsewhere in the
    /// boot sequence - FirebaseRemoteConfigManager.Init already calls
    /// FirebaseApp.CheckAndFixDependenciesAsync for Analytics/RemoteConfig.
    public class FirebaseCloudStorage : IRemoteStorage
    {
        /// Safety cap per file - GetBytesAsync buffers the whole download in memory, so an
        /// unexpectedly large object would otherwise be an out-of-memory crash on mobile.
        public const long DefaultMaxDownloadBytes = 10 * 1024 * 1024; // 10 MB

        private readonly string _rootPath;
        private readonly long _maxDownloadBytes;

        /// <param name="rootPath">Bucket-relative folder all paths are resolved against.
        /// Empty means the bucket root.</param>
        /// <param name="maxDownloadBytes">Per-file size cap; raise it only for features
        /// that legitimately download bigger blobs.</param>
        public FirebaseCloudStorage(string rootPath = "", long maxDownloadBytes = DefaultMaxDownloadBytes)
        {
            _rootPath = string.IsNullOrEmpty(rootPath) ? string.Empty : rootPath.Trim('/');
            _maxDownloadBytes = maxDownloadBytes;
        }

        public async UniTask<byte[]> DownloadBytesAsync(string path)
        {
#if VIRTUESKY_FIREBASE_STORAGE
            try
            {
                var reference = FirebaseStorage.DefaultInstance.RootReference.Child(FullPath(path));
                return await reference.GetBytesAsync(_maxDownloadBytes).AsUniTask();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseCloudStorage] Failed to download '{FullPath(path)}': {e.Message}");
                return null;
            }
#else
            LogMissingDefine();
            await UniTask.CompletedTask;
            return null;
#endif
        }

        /// Resolves a public https download URL for <paramref name="path"/>, for the cases
        /// where an API needs a URL rather than bytes (UnityWebRequestTexture, VideoPlayer,
        /// ...). Returns null on failure.
        public async UniTask<string> GetDownloadUrlAsync(string path)
        {
#if VIRTUESKY_FIREBASE_STORAGE
            try
            {
                var reference = FirebaseStorage.DefaultInstance.RootReference.Child(FullPath(path));
                var uri = await reference.GetDownloadUrlAsync().AsUniTask();
                return uri?.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseCloudStorage] Failed to resolve URL for '{FullPath(path)}': {e.Message}");
                return null;
            }
#else
            LogMissingDefine();
            await UniTask.CompletedTask;
            return null;
#endif
        }

        private string FullPath(string path)
        {
            var trimmed = path?.TrimStart('/') ?? string.Empty;
            return string.IsNullOrEmpty(_rootPath) ? trimmed : $"{_rootPath}/{trimmed}";
        }

#if !VIRTUESKY_FIREBASE_STORAGE
        private static void LogMissingDefine()
        {
            Debug.LogError("[FirebaseCloudStorage] VIRTUESKY_FIREBASE_STORAGE is not defined - " +
                           "add it from Control Panel > Firebase, or in Player Settings > " +
                           "Scripting Define Symbols alongside VIRTUESKY_FIREBASE.");
        }
#endif
    }
}
