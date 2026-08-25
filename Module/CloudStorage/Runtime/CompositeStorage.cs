using Cysharp.Threading.Tasks;

namespace VirtueSky.CloudStorage
{
    /// Tries each IRemoteStorage layer in order, returning the first non-null result -
    /// used to put a bundled StreamingAssets seed (always available, zero network, but
    /// frozen at build time) in front of Firebase Storage (only reachable with network,
    /// but has whatever was published after this build shipped).
    public class CompositeStorage : IRemoteStorage
    {
        private readonly IRemoteStorage[] _layers;

        public CompositeStorage(params IRemoteStorage[] layers)
        {
            _layers = layers;
        }

        public async UniTask<byte[]> DownloadBytesAsync(string path)
        {
            foreach (var layer in _layers)
            {
                if (layer == null) continue;
                var bytes = await layer.DownloadBytesAsync(path);
                if (bytes != null) return bytes;
            }

            return null;
        }
    }
}
