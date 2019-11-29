using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Monik.Common;
using Nancy.Owin;

namespace Monik.Service
{
    public class Startup
    {
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            IHostApplicationLifetime hostLifetime,
            IConfiguration config)
        {
            var settings = config.GetSection("Service").Get<MonikServiceSettings>();
            var bootstrapper = new Bootstrapper(settings);

            hostLifetime.ApplicationStarted.Register(() =>
            {
                bootstrapper.Resolve<IMonik>().ApplicationWarning("ApplicationStarted");
            });
            hostLifetime.ApplicationStopping.Register(() =>
            {
                bootstrapper.Resolve<IMonik>().ApplicationWarning("ApplicationStopping");
            });
            hostLifetime.ApplicationStopped.Register(() =>
            {
                bootstrapper.OnApplicationStop();
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOwin(x => x.UseNancy(opt =>
            {
                opt.Bootstrapper = bootstrapper;
            }));
        }
    }
}