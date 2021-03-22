using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using urlShortner.Server.Models;
using urlShortner.Server.Services;

namespace urlShortner.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // requires using Microsoft.Extensions.Options
            services.Configure<UrlMapDatabaseSettings>(
                Configuration.GetSection(nameof(UrlMapDatabaseSettings)));

            // The IUrlMapDatabaseSettings interface is registered in DI with a singleton service lifetime. When injected, the interface instance resolves to a UrlMapDatabaseSettings object.
            services.AddSingleton<IUrlMapDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<UrlMapDatabaseSettings>>().Value);

            // The UrlService class is registered with DI to support constructor injection in consuming classes.
            // The singleton service lifetime is most appropriate because UrlService takes a direct dependency on MongoClient. 
            // Per the official Mongo Client reuse guidelines, MongoClient should be registered in DI with a singleton service lifetime.
            // https://mongodb.github.io/mongo-csharp-driver/2.8/reference/driver/connecting/#re-use
            services.AddSingleton<IUrlService, UrlService>();

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
