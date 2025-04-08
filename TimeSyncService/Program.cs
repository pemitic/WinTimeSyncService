using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TimeSyncService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            // Debug code: allows running as console app
            var service = new TimeSyncService();
            service.OnStart(null);
            Console.WriteLine("Service running. Press any key to stop...");
            Console.ReadKey();
            service.OnStop();
#else
            // Release code: runs as Windows Service
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new TimeSyncService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
