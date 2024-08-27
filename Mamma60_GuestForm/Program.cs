using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mamma60_GuestForm.Data;
using Microsoft.Extensions.DependencyInjection;
using Mamma60_GuestForm.Models;
using Mamma60_GuestForm.Custom;
using Microsoft.AspNetCore.Identity;

namespace Mamma60_GuestForm
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            CreateDbIfNotExists(host);

            host.Run();
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                System.IO.Directory.CreateDirectory("images");
                System.IO.Directory.CreateDirectory("images_nobg");
                System.IO.Directory.CreateDirectory("images_clipped");

                try
                {
                    var userProfileContext = services.GetRequiredService<UserProfileContext>();
                    //DbInitializer.Initialize(userProfileContext);
                    if (false)
                    {
                        ClipImages();
                    }
                    if(false)
                    {
                        ExportedMashUpToJSON(userProfileContext);
                    }
                    
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred creating the DB.");
                }
            }
        }

        public static void ClipImages()
        {
            // If we want to fit clustering within a specific shape, make the error function based on fitness within said space
            AutoCrop.GenerateShapes("images_nobg", "images_clipped");
        }

        public static void ExportedMashUpToJSON(UserProfileContext context)
        {
            // If we want to fit clustering within a specific shape, make the error function based on fitness within said space
            AutoCrop.GenerateExpandedImages("images_final", context);
        }
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
