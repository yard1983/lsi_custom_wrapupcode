using Newtonsoft.Json;
using NLog;
using NLog.Targets.Wrappers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Genesys.PS.Test
{
    class Program
    {
        static void Main(string[] args)
        {            
            Genesys.PS.SelfHost.WindowsService service = new SelfHost.WindowsService();
            service.onStart();

            string command = Console.ReadLine();
            while (command.ToLower() != "exit")
            {
                command = Console.ReadLine();
            }


        }
       
    }


}
