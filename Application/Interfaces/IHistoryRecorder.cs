using TheEye.Application.DTOs;

namespace TheEye.Application.Interfaces
{
    /// <summary>
    /// Records Eye snapshots to a persistent store (file, db, etc.).
    /// Implementations should be side-effecting (append-only) and thread-safe.
    /// </summary>
    public interface IHistoryRecorder
    {
        /// <summary>
        /// Record a snapshot asynchronously. Implementations should append to storage.
        /// </summary>
        Task RecordAsync(EyeSnapshotDto snapshot);
    }
}
