using System;
using System.Reflection;
using Controlzmo.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Controlzmo
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class ComponentAttribute : Attribute { }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR();
            services.AddLogging();
            foreach (var candidate in Assembly.GetEntryAssembly()!.DefinedTypes)
            {
                if (candidate.GetCustomAttribute<ComponentAttribute>() == null) continue;
                services.AddSingleton(candidate, candidate);
                foreach (var also in candidate.GetInterfaces())
                    services.AddSingleton(also, x => x.GetRequiredService(candidate));
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<ControlzmoHub>("/hub/connectzmo");
            });

            app.ApplicationServices.GetServices<CreateOnStartup>();
        }
    }

    public interface CreateOnStartup { }
}
