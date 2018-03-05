using MonicService.Workers;
using Monik.Service;
using Topshelf;
using Topshelf.Nancy;

namespace MonicService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<IObject>(s =>
                {
                    s.ConstructUsing(settings => new ServiceWorker());
                    s.WhenStarted(tc => tc.OnStart());
                    s.WhenStopped(tc => tc.OnStop());
                    s.WithNancyEndpoint(x, c => c.CreateUrlReservationsOnInstall());
                });

                x.RunAsLocalSystem();
                x.SetDescription("Monik WinService");
                x.SetDisplayName("Monik WinService");
                x.SetServiceName("Monik");
            });
        }
    }
}