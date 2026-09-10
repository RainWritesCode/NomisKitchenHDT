using System;
using System.IO;

namespace NomisKitchenHDT.Utils
{
    internal static class Log
    {
        static readonly object _lock = new object();
        const long MaxBytes = 1024 * 1024;

        internal static string FilePath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HearthstoneDeckTracker",
                "NomisKitchenHDT.log");

        internal static void Info(string msg) => Write("INFO ", msg);
        internal static void Warn(string msg) => Write("WARN ", msg);
        internal static void Error(string msg, Exception ex = null) =>
            Write("ERROR", ex == null ? msg : msg + " :: " + ex.GetType().Name + ": " + ex.Message);

        static void Write(string level, string msg)
        {
            try
            {
                lock (_lock)
                {
                    var p = FilePath;
                    Directory.CreateDirectory(Path.GetDirectoryName(p));
                    var fi = new FileInfo(p);
                    if (fi.Exists && fi.Length > MaxBytes)
                    {
                        var old = p + ".old";
                        if (File.Exists(old)) File.Delete(old);
                        File.Move(p, old);
                    }
                    File.AppendAllText(p, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + level + " " + msg + Environment.NewLine);
                }
            }
            catch { }
        }
    }
}