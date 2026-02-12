using System.Collections.Concurrent;

namespace DataGen_v1.Services
{
    // Simple model for display
    public record LogEntry(string Time, string GeneratorName, string Target, string Status, string Message);

    public class SystemLogService
    {
        // Thread-safe queue to hold logs
        private readonly ConcurrentQueue<LogEntry> _logs = new();
        private readonly int _maxLogs = 1000; // Keep only last 1000 messages to save RAM

        // Event to notify UI when a new log arrives
        public event Action? OnLogAdded;

        public void AddLog(string genName, string target, string status, string message)
        {
            var entry = new LogEntry(
                DateTime.Now.ToString("HH:mm:ss"),
                genName,
                target,
                status,
                message
            );

            _logs.Enqueue(entry);

            // Cleanup old logs if we exceed limit
            if (_logs.Count > _maxLogs)
            {
                _logs.TryDequeue(out _);
            }

            // Notify the UI to refresh
            OnLogAdded?.Invoke();
        }

        public List<LogEntry> GetLogs()
        {
            // Return latest first
            return _logs.Reverse().ToList();
        }

        public void Clear()
        {
            _logs.Clear();
            OnLogAdded?.Invoke();
        }
    }
}