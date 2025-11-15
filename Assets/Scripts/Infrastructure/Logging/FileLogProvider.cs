using System.IO;
using System.Text;
using UnityEngine;

namespace Fireclicks.Infrastructure.Logging
{
    public class FileLogProvider : ILogProvider, System.IDisposable
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private StreamWriter _writer;
        private bool _disposed;

        public FileLogProvider()
        {
            _logFilePath = Path.Combine(Application.persistentDataPath, "game_log.txt");
            
            if (File.Exists(_logFilePath))
            {
                var fileInfo = new FileInfo(_logFilePath);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    var backupPath = _logFilePath + ".backup";
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(_logFilePath, backupPath);
                }
            }

            try
            {
                _writer = new StreamWriter(_logFilePath, true, Encoding.UTF8, 4096)
                {
                    AutoFlush = true
                };
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to open log file: {ex.Message}");
                _writer = null;
            }
        }

        public void Log(string message)
        {
            WriteToFile($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            WriteToFile($"[WARNING] {message}");
        }

        public void LogError(string message)
        {
            WriteToFile($"[ERROR] {message}");
        }

        private void WriteToFile(string message)
        {
            if (_disposed || _writer == null)
            {
                Debug.Log(message);
                return;
            }

            try
            {
                lock (_lockObject)
                {
                    if (_writer != null && !_disposed)
                    {
                        _writer.WriteLine($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lockObject)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.Flush();
                        _writer.Close();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error closing log file: {ex.Message}");
                    }
                    finally
                    {
                        _writer?.Dispose();
                        _writer = null;
                    }
                }

                _disposed = true;
            }
        }
    }
}

