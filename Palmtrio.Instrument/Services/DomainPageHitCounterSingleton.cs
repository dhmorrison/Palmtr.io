using Palmtrio.Domain.Interfaces;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading;

namespace Palmtrio.Instrument.Services
{
    public class DomainPageHitCounterSingleton : IPageHitCounter, IDisposable
    {
        private object _countLock = new object();
        public void Count(string pageHitInfo = "")
        {
            lock(_countLock)
            {
                _domainHits++;
                if (!String.IsNullOrEmpty(pageHitInfo))
                {
                    _domainHitInfoBuilder.Append(pageHitInfo);
                }
            }
        }

        private bool _isActive = false;
        private int _domainHits = 0;
        private StringBuilder _domainHitInfoBuilder = new StringBuilder();
        private Timer _flushTimer;
        private TimeSpan _flushTimerInterval = TimeSpan.FromHours(1);

        public string InjLogPath { get; set; }
        public DomainPageHitCounterSingleton()
        {
            _isActive = (ConfigurationManager.AppSettings["PageHitCounterEnabled"] ?? "false").ToLower() == "true";
            if(_isActive)
            {
                TimeSpan minutesToNextHour = TimeSpan.FromMinutes((60 - DateTime.Now.Minute));
                _flushTimer = new Timer(Flush, this, minutesToNextHour, _flushTimerInterval);
            }
        }

        private static void Flush(object data)
        {
            DomainPageHitCounterSingleton instance = data as DomainPageHitCounterSingleton;

            string logFileName = "clicks_" + DateTime.Now.ToString("ddd").ToLower() + ".txt";
            string logFilePath = Path.Combine(instance.InjLogPath, logFileName);

            using (StreamWriter sw = new StreamWriter(logFilePath, true))
            {
                string hitsCount = "[ " + instance._domainHits.ToString() + " ]domain hits " + DateTime.Now.ToString("HH:mm");
                string hitsInfo = instance._domainHitInfoBuilder.ToString();

                sw.WriteLine(hitsCount + " *" + hitsInfo + "*");
                sw.Flush();
            }

            lock (instance._countLock)
            {
                instance._domainHits = 0;
                instance._domainHitInfoBuilder.Clear();
            }
        }

        public void Dispose()
        {
            if (_isActive)
            {
                try
                {
                    DomainPageHitCounterSingleton.Flush(this);
                }
                catch { }
            }

            if (_flushTimer != null)
                _flushTimer.Dispose();
        }

    }
}
