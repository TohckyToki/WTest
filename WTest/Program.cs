using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Text;
using WTest.Components;
using WTest.Middlewares;
using WTest.Services;

namespace WTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add logger of Serilog.
            var logger = new LoggerConfiguration()
                .WriteTo.File(builder.Configuration["LogPath"]!, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, retainedFileCountLimit: 90, encoding: Encoding.UTF8)
                .CreateLogger();
            builder.Logging.AddSerilog(logger);

            // Add services to the container.
            builder.Services.AddRazorComponents();

            builder.Services.AddControllers();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            builder.Services.AddSingleton<WsManager>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>();

            app.MapControllers();
            app.UseWebSockets();

            app.UseSession();

            app.UseMiddleware<WsDispatcher>();

            app.Run();
        }
    }
}
