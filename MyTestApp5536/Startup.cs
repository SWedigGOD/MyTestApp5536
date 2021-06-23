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
using System.Configuration;

namespace MyTestApp5536
{
    public class Startup
    {
        private static string Host = "mytestpostgredb5536.postgres.database.azure.com";
        private static string User = "SWedig@mytestpostgredb5536";
        private static string DBname = "mypgsqldb";
        private static string Password = "TcvDzE8cBjVbLmy";
        private static string Port = "5432";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            string conStr = "Server=mytestpostgredb5536.postgres.database.azure.com;Database=mypgsqldb;Port=5432;User Id=SWedig@mytestpostgredb5536;Password=TcvDzE8cBjVbLmy;Ssl Mode=Require;";/*"Database=mypgsqldb; Data Source=mytestpostgredb5536.postgres.database.azure.com; User Id=SWedig@mytestpostgredb5536; Password=TcvDzE8cBjVbLmy";/*String.Format(Configuration.GetConnectionString("MyTestApp5536Context"),
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);*/
            //Configuration["My:Hierarchical:Config:Data"];
            services.AddDbContext<MyTestApp5536Context>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("POSTGRESQLCONNSTR_MyDbConnection")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
