// <copyright file="Program.cs" company="Patrick Kramer">
// Licensed under the terms of the MIT license. See the LICENSE file in the repository root for the full license text.
// </copyright>

namespace BulbapediaCrawler.Api
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Main class of this ASP.NET Core stand-alone web application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Runs this web application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Configures a <see cref="IWebHostBuilder"/> with the configuration needed by this web application.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The created web-host builder instance.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
