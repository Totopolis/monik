using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Monik.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(hostConfigurator =>
            {
                hostConfigurator.Service<NancyHostHolder>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(name => new NancyHostHolder());
                    serviceConfigurator.WhenStarted(hostHolder => hostHolder.Start());
                    serviceConfigurator.WhenStopped(hostHolder => hostHolder.Stop());
                });

                hostConfigurator.RunAsLocalSystem();
                //System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
                hostConfigurator.SetDescription("Monik service");
                hostConfigurator.SetDisplayName("Monik");
                hostConfigurator.SetServiceName("Monik");
            });
        }
    }
}
