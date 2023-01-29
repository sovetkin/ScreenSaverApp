using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ScreenSaverServer.BussinessLogic;
using ScreenSaverServer.BussinessLogic.Interfaces;
using ScreenSaverServer.BussinessLogic.Models;

namespace ScreenSaver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("Properties\\launchSettings.json");
            AppConfiguration = builder.Build();
        }

        public IConfiguration AppConfiguration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.Configure<Config>(AppConfiguration.GetSection("Config"));
            services.AddSingleton<IRectangleRepository, RectangleRepository>()
                    .AddSingleton<IPathGenerator, RectangleUpdater>()
                    .AddScoped<IRectangleHandler, RectangleHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ScreenSaverService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
