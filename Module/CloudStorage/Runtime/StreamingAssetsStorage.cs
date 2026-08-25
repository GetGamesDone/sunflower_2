using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace VirtueSky.CloudStorage
{
    /// Reads files bundled into the build under StreamingAssets using the same path layout
    /// as the remote bucket, so it can be stacked in front of FirebaseCloudStorage as a
    /// zero-network seed layer (see CompositeStorage).
    ///
    /// Uses UnityWebRequest rather than System.IO: StreamingAssets lives inside the
    /// compressed APK/AAB on Android (a plain file path can't read into it), while it's an
    /// ordinary file on iOS - UnityWebRequest is the one API Unity supports identically on
    /// both.
    public class StreamingAssetsStorage : IRemoteStorage
    {
        private readonly string _rootUrl;

        /// <param name="rootFolder">StreamingAssets-relative folder, e.g.
        /// "Levels/ZooTrail". Empty means the StreamingAssets root.</param>
        public StreamingAssetsStorage(string rootFolder)
        {
            var trimmed = string.IsNullOrEmpty(rootFolder) ? string.Empty : rootFolder.Trim('/');
            _rootUrl = string.IsNullOrEmpty(trimmed)
                ? Application.streamingAssetsPath
                : $"{Application.streamingAssetsPath}/{trimmed}";
        }

        public async UniTask<byte[]> DownloadBytesAsync(string path)
        {
            using var request = UnityWebRequest.Get($"{_rootUrl}/{path?.TrimStart('/')}");
            try
            {
                await request.SendWebRequest().ToUniTask();
            }
            catch (Exception)
            {
                // Not present in the bundled seed (e.g. content published after this build
                // shipped) - CompositeStorage falls through to the next layer.
                return null;
            }

            return request.result == UnityWebRequest.Result.Success ? request.downloadHandler.data : null;
        }
    }
}
