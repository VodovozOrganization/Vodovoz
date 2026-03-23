using CustomerNotifications.Consumer.Configuration;
using Microsoft.Extensions.Hosting;

namespace PushNotifications.Service
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddCustomerNotificationsConsumer(hostContext);
				});
	}
}
