using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTestApp5536.Data;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;

namespace MyTestApp5536 {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddControllersWithViews();

            if (Config.DBConnectionString == "") {
                string ConnectionString = Configuration.GetConnectionString("MyDbConnection");
                // "Server=mytestpostgredb5536.postgres.database.azure.com;Database=mypgsqldb;Port=5432;User Id=SWedig@mytestpostgredb5536;Password=TcvDzE8cBjVbLmy;Ssl Mode=Require;";

                var client = new SecretClient(new Uri("https://mytestkeyvault5536.vault.azure.net/"), new DefaultAzureCredential());
                var secret = client.GetSecret("DBPW");
                if (ConnectionString == null)
                    ConnectionString = "Server=mytestpostgredb5536.postgres.database.azure.com;Database=mypgsqldb;Port=5432;User Id=SWedig@mytestpostgredb5536;Password=TcvDzE8cBjVbLmy;Ssl Mode=Require;";
                if (ConnectionString != null)
                    ConnectionString = ConnectionString.Replace("{PW}", secret.Value.Value);
                Config.DBConnectionString = ConnectionString;
            }
            services.AddDbContext<MyTestApp5536Context>(options =>
                    options.UseNpgsql(Config.DBConnectionString));
            if (Config.StorageACConStr == "") {
                Config.StorageACConStr = Configuration.GetConnectionString("StorageAccount");
                if (Config.StorageACConStr == null)
                    Config.StorageACConStr = "DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=teststorageac5536;AccountKey=+i0qNITgie6J2jtKaPt8i7cYo/MPLVL9Qk5T0KoDUvSSkQfdauRpXidTC/vtz+m+4XV+NqdsLBdatl7cspyWxg==";
            }
            BlobContainerClient container = new BlobContainerClient(Config.StorageACConStr, "filecontainer");
            container.CreateIfNotExists();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            } else {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
