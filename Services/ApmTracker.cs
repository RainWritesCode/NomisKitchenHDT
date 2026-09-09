using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace NomisKitchenHDT.Services
{
    public class ApmTracker
    {
        private const int WindowSeconds = 4;
        private const int SampleMs = 500;

        private readonly object _lock = new object();
        private readonly List<Sample> _samples = new List<Sample>();
        private Timer _timer;

        public int ActionsThisTurn { get; private set; }
        public double CurrentApm { get; private set; }
        public double PeakApm { get; private set; }
        public double AverageApm { get; private set; }

        private int _totalGameActions;
        private DateTime _gameStart = DateTime.MinValue;

        public event Action OnStatsUpdated;

        public void Start()
        {
            _timer = new Timer(SampleMs) { AutoReset = true };
            _timer.Elapsed += (_, _1) => Tick();
            _timer.Start();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        public void OnGameStart()
        {
            lock (_lock)
            {
                _samples.Clear();
                ActionsThisTurn = 0;
                CurrentApm = 0;
                PeakApm = 0;
                AverageApm = 0;
                _totalGameActions = 0;
                _gameStart = DateTime.UtcNow;
            }
            OnStatsUpdated?.Invoke();
        }

        public void OnGameEnd() { }

        public void OnTurnStart()
        {
            lock (_lock)
            {
                ActionsThisTurn = 0;
            }
        }

        public void OnPlayerAction(string kind)
        {
            lock (_lock)
            {
                ActionsThisTurn++;
                _totalGameActions++;
            }
        }

        private void Tick()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-WindowSeconds);
                _samples.RemoveAll(s => s.Timestamp < windowStart);
                _samples.Add(new Sample { Timestamp = now, Actions = ActionsThisTurn });

                if (_samples.Count >= 2)
                {
                    var oldest = _samples[0];
                    var newest = _samples[_samples.Count - 1];
                    var diff = newest.Actions - oldest.Actions;
                    var seconds = (newest.Timestamp - oldest.Timestamp).TotalSeconds;
                    CurrentApm = seconds > 0 ? Math.Max(0, diff / seconds * 60) : 0;
                    if (CurrentApm > PeakApm) PeakApm = CurrentApm;
                }

                if (_gameStart != DateTime.MinValue)
                {
                    var minutes = (now - _gameStart).TotalMinutes;
                    AverageApm = minutes > 0 ? _totalGameActions / minutes : 0;
                }
            }
            OnStatsUpdated?.Invoke();
        }

        private struct Sample
        {
            public DateTime Timestamp;
            public int Actions;
        }
    }
}
