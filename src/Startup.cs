using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using src.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using src.Areas.ActiviyProtocol.Policies;
using Microsoft.AspNetCore.Authorization;
using src.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace src
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => 
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))
                //.LogTo(msg => Console.WriteLine(msg))
                //.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
            );
            services.AddDatabaseDeveloperPageExceptionFilter();

            //use the "Identity" framework to do authentication
            //any authentication or authorization calls (enabled below in Configure()) will ask this service for auth!
            services.AddDefaultIdentity<ApplicationUser>(options => {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<SignInManager<ApplicationUser>, ApplicationSignInManager>();

            services.AddControllersWithViews();

            // add our policy that requires documents to be no older than 24h for editing
            services.AddAuthorization(options => {
                options.AddPolicy("OneDayEditPolicy", policy => 
                    policy.Requirements.Add(new OneDayEditRequirement()));
            });

            //add the handler for that policy
            services.AddSingleton<IAuthorizationHandler, OneDayEditPolicyHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            //order is important here! This is a pipeline!
            //1) check where we are going
            //2) check who we are
            //3) check if we are allowed to go there
            //used by the [Authorize] annotation for example

            //find out where we need to go
            //ex: site.com/Home/Privacy
            //calls HomeController.Privacy()
            //see Endpoints below for things we can route to
            app.UseRouting();

            //first authenticate (prove who you are)
            app.UseAuthentication();
            //then authorize (check if you're allowed to do something)
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapAreaControllerRoute(
                    name: "BirdVoiceArea",
                    areaName: "BirdVoice",
                    pattern: "BirdVoice/{action=Index}/{id?}",
                    defaults: new { controller = "BirdVoice" });

                endpoints.MapAreaControllerRoute(
                    name: "ActivityProtocolArea",
                    areaName: "ActivityProtocol",
                    pattern: "ActivityProtocol/{action=Index}/{id?}",
                    defaults: new { controller = "ActivityProtocol" });

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{action=Index}/{id?}",
                    defaults: new { controller = "Home" });

                endpoints.MapRazorPages();
            });
        }
    }
}
