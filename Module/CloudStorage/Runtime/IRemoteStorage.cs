using Cysharp.Threading.Tasks;

namespace VirtueSky.CloudStorage
{
    /// Abstraction over a remote blob store, so features that consume downloaded content
    /// (remote levels, leaderboard snapshots, remote sprites, config bundles, ...) never
    /// reference a concrete backend - swap in a fake for edit-mode/unit tests, or another
    /// backend later, without touching the feature code.
    public interface IRemoteStorage
    {
        /// Downloads the raw bytes at <paramref name="path"/>, relative to this storage's
        /// root (e.g. "configs/cfg_003.cfg"). Returns null on failure or when missing.
        UniTask<byte[]> DownloadBytesAsync(string path);
    }
}
