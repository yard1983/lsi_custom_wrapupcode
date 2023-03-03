using System.ServiceProcess;

namespace Genesys.PS.SelfHost
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[]
            {
                new WindowsService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
