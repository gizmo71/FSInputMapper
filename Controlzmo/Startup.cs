using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var component in Assembly.GetEntryAssembly()!.DefinedTypes.Where(IsConcreteComponent))
            {
                //Console.WriteLine($"{component}");
                services.AddSingleton(component, component);
                foreach (var also in GetInterfacesAndParentComponents(component))
                    services.AddSingleton(also, x => x.GetRequiredService(component));
            }
        }

        private static IEnumerable<TypeInfo> GetInterfacesAndParentComponents(TypeInfo type)
        {
            foreach (var also in type.GetInterfaces())
            {
                Console.WriteLine($"\t{also} interface for {type}");
                yield return also.GetTypeInfo();
            }
            for (TypeInfo? @base = type; (@base = @base!.BaseType?.GetTypeInfo()) != null;)
            {
                if (IsComponent(@base)) {
                    Console.WriteLine($"\t{@base} base for {type}");
                    yield return @base;
                }
            }
        }

        private static bool IsConcreteComponent(TypeInfo type) => !type.IsAbstract && IsComponent(type);
        private static bool IsComponent(TypeInfo type) => type.GetCustomAttribute<ComponentAttribute>() != null;

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
