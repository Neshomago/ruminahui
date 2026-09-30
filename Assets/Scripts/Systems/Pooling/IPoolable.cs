// Implements: 11-camera-performance.md, Part B Step 2 (object pooling from day one).
namespace Ruminahui
{
    /// <summary>Components on pooled objects implement this to reset themselves. Called on every component in the hierarchy.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
