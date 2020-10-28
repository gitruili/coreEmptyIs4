using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace coreEmptyIs4
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddControllersWithViews();

            var builder = services.AddIdentityServer(options =>
                            {
                                options.Events.RaiseErrorEvents = true;
                                options.Events.RaiseInformationEvents = true;
                                options.Events.RaiseFailureEvents = true;
                                options.Events.RaiseSuccessEvents = true;
                                //options.UserInteraction.LoginUrl = "/Account/Login";
                                //options.UserInteraction.LogoutUrl = "/Account/Logout";
                                options.Authentication = new AuthenticationOptions()
                                {
                                    CookieLifetime = TimeSpan.FromHours(10), // ID server cookie timeout set to 10 hours
                                    CookieSlidingExpiration = true
                                };
                            })
                            .AddConfigurationStore(options =>
                            {
                                options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                            })
                            .AddOperationalStore(options =>
                            {
                                options.ConfigureDbContext = b => b.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                                options.EnableTokenCleanup = true;
                            })
                            //.AddInMemoryIdentityResources(Config.Ids)
                            //.AddInMemoryApiResources(Config.Apis)
                            //.AddInMemoryClients(Config.Clients)
                            .AddTestUsers(Config.Users);

            builder.AddDeveloperSigningCredential();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});
            });
        }
    }
}
