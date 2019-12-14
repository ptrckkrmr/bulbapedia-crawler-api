// <copyright file="Startup.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Api
{
    using System;
    using AngleSharp;
    using BulbapediaCrawler.Api.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

    /// <summary>
    /// Startup class of this ASP.NET Core web application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the application configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the services for this application container.
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton(
                typeof(BulbapediaService),
                provider => new BulbapediaService(
                    AngleSharp.Configuration.Default.WithDefaultLoader(),
                    new Uri(this.Configuration.GetSection("BulbapediaRootUrl").Get<string>())));

            services.AddSingleton(typeof(PokemonService));
        }

        /// <summary>
        /// Configures the request pipeline for this application container.
        /// </summary>
        /// <remarks>
        /// This method gets called by the runtime.
        /// </remarks>
        /// <param name="app">The application builder instance.</param>
        /// <param name="env">The hosting environment.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
