using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WeddingPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WeddingPlanner
{
    public class Startup
    {
        // Creates a new Configuration object with a getter only
        public IConfiguration Configuration {get;}

        // Injects default IConfiguration service
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Adds DbContext
            services.AddDbContext<HomeContext>(options => options.UseMySql(Configuration["DBInfo:ConnectionString"]));
            // Adds Session
            services.AddSession();
            // Adds MVC
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // Uses Session
            app.UseSession();
            // Uses Mvc
            app.UseMvc();
        }
    }
}
