using Monik.Service;
using Topshelf;

namespace MonicWinService
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
                });

                x.RunAsLocalSystem();
                x.SetDescription("Monik WinService");
                x.SetDisplayName("Monik WinService");
                x.SetServiceName("Monik");
            });
        }
    }
}