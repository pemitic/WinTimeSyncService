using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
//using System.Threading.Tasks;
using System.Threading;

namespace TimeSyncService
{
    public partial class TimeSyncService : ServiceBase
    {
        private Timer _syncTimer;
        private string[] _timeServers;
        private readonly string _iniFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

        public TimeSyncService()
        {
            InitializeComponent();
            ServiceName = "InternetTimeSyncService";
            CanStop = true;
            CanPauseAndContinue = true;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            LoadConfiguration();

            // First sync immediately on start
            SyncTimeWithInternet();

            // Then set up periodic sync every 15 minutes
            _syncTimer = new Timer(SyncTimerCallback, null, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(15));

            EventLog.WriteEntry("Time synchronization service started", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            _syncTimer?.Dispose();
            EventLog.WriteEntry("Time synchronization service stopped", EventLogEntryType.Information);
        }

        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(_iniFilePath))
                {
                    // Create default config if doesn't exist
                    CreateDefaultConfig();
                }

                _timeServers = IniFileReader.ReadAllValues("TimeServers", _iniFilePath)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                if (_timeServers.Length == 0)
                {
                    _timeServers = new[] { "time.windows.com", "time.nist.gov", "pool.ntp.org" };
                    EventLog.WriteEntry("Using default time servers as none were configured", EventLogEntryType.Warning);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Error loading configuration: {ex.Message}", EventLogEntryType.Error);
                _timeServers = new[] { "time.windows.com", "time.nist.gov", "pool.ntp.org" };
            }
        }

        private void CreateDefaultConfig()
        {
            try
            {
                File.WriteAllText(_iniFilePath,
                    "[TimeServers]\n" +
                    "server1=time.windows.com\n" +
                    "server2=time.nist.gov\n" +
                    "server3=pool.ntp.org\n");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Error creating default config: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private void SyncTimerCallback(object state)
        {
            SyncTimeWithInternet();
        }

        private void SyncTimeWithInternet()
        {
            try
            {
                EventLog.WriteEntry("Starting time synchronization", EventLogEntryType.Information);

                foreach (var server in _timeServers)
                {
                    if (TrySyncWithServer(server))
                    {
                        EventLog.WriteEntry($"Successfully synchronized time with {server}", EventLogEntryType.Information);
                        return;
                    }
                }

                EventLog.WriteEntry("Failed to synchronize with all time servers", EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Error during time synchronization: {ex.Message}", EventLogEntryType.Error);
            }
        }

        private bool TrySyncWithServer(string server)
        {
            try
            {
                var configProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "w32tm",
                        Arguments = $"/config /syncfromflags:manual /manualpeerlist:\"{server}\"",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    }
                };
                configProcess.Start();
                configProcess.WaitForExit(5000);

                var resyncProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "w32tm",
                        Arguments = "/resync",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = false
                    }
                };
                resyncProcess.Start();
                resyncProcess.WaitForExit(5000);

                return resyncProcess.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
