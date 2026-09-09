using System;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace NomisKitchenHDT.Services
{
    public class ApmTracker
    {
        const string MmfName = "NomisKitchenApm";
        const int MmfSize = 512;

        MemoryMappedFile _mmf;
        MemoryMappedViewAccessor _accessor;
        Timer _timer;
        readonly byte[] _buffer = new byte[MmfSize];

        public bool InGame { get; private set; }
        public int ActionsThisTurn { get; private set; }
        public double CurrentApm { get; private set; }
        public double PeakApm { get; private set; }
        public double AverageApm { get; private set; }

        public event Action OnStatsUpdated;

        public void Start()
        {
            _timer = new Timer(500) { AutoReset = true };
            _timer.Elapsed += (_, _1) => Tick();
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            try { _accessor?.Dispose(); _accessor = null; } catch { }
            try { _mmf?.Dispose(); _mmf = null; } catch { }
        }

        void Tick()
        {
            if (_accessor == null && !TryOpen()) return;
            try
            {
                _accessor.ReadArray(0, _buffer, 0, MmfSize);
                int len = 0;
                while (len < MmfSize && _buffer[len] != 0) len++;
                if (len == 0) { InGame = false; return; }

                var json = Encoding.UTF8.GetString(_buffer, 0, len);
                InGame = ParseBool(json, "inGame");
                ActionsThisTurn = ParseInt(json, "actionsThisTurn");
                CurrentApm = ParseDouble(json, "currentApm");
                PeakApm = ParseDouble(json, "peakApm");
                AverageApm = ParseDouble(json, "averageApm");

                OnStatsUpdated?.Invoke();
            }
            catch
            {
                try { _accessor?.Dispose(); _accessor = null; _mmf?.Dispose(); _mmf = null; } catch { }
            }
        }

        bool TryOpen()
        {
            try
            {
                _mmf = MemoryMappedFile.OpenExisting(MmfName);
                _accessor = _mmf.CreateViewAccessor(0, MmfSize, MemoryMappedFileAccess.Read);
                return true;
            }
            catch { return false; }
        }

        static int ParseInt(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+)");
            return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        static double ParseDouble(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)");
            return m.Success ? double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        static bool ParseBool(string json, string key)
        {
            var m = Regex.Match(json, "\"" + Regex.Escape(key) + "\"\\s*:\\s*(true|false)");
            return m.Success && m.Groups[1].Value == "true";
        }
    }
}
