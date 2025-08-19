using System.Text.Json;
using TheEye.Application.DTOs;
using TheEye.Application.Interfaces;

namespace TheEye.Application.Helpers
{
    /// <summary>
    /// Appends newline-delimited JSON snapshots to a local text file.
    /// Default path: ./logs/eye_history.txt
    /// </summary>
    public class HistoryRecorder : IHistoryRecorder
    {
        readonly string _path;

        public HistoryRecorder(string? path = null)
        {
            _path = string.IsNullOrWhiteSpace(path) ? Path.Combine(AppContext.BaseDirectory, "logs", "eye_history.txt") : path;
            var dir = Path.GetDirectoryName(_path) ?? AppContext.BaseDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        }

        public async Task RecordAsync(EyeSnapshotDto snapshot)
        {
            // serialize as compact JSON per-line
            var options = new JsonSerializerOptions { WriteIndented = false };
            string line = JsonSerializer.Serialize(snapshot, options);
            // append line with newline
            await File.AppendAllTextAsync(_path, line + Environment.NewLine);
        }
    }
}
