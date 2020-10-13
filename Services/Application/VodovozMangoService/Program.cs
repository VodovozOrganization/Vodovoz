using System;
using System.Net;
using Grpc.Core;
using MangoService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Nini.Config;
using NLog.Web;
using VodovozMangoService.HostedServices;

namespace VodovozMangoService
{
	public class Program
    {
	    private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
	    public static void Main(string[] args)
        {
	       var configuration = new VodovozMangoConfiguration();
	       CreateHostBuilder(args, configuration).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args, VodovozMangoConfiguration configuration) =>
            Host.CreateDefaultBuilder(args)
	            .ConfigureLogging(logging =>
	            {
		            logging.ClearProviders();
		            logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
	            })
	            .ConfigureServices(servises => servises.AddSingleton(configuration))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel(k =>
                        {
                            var appServices = k.ApplicationServices;
                            k.Listen(
                                IPAddress.Any, configuration.MangoServiceHttpsPort,
                                o => o.UseHttps(h =>
                                {
                                    h.UseLettuceEncrypt(appServices);
                                }));
                            k.Listen(IPAddress.Any, configuration.MangoServiceHttpPort);
                        })
                        .UseStartup<Startup>();
                })
	            .UseNLog();
    }
}