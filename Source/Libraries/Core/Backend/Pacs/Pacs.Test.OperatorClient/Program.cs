using Autofac.Core;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pacs.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Pacs.Core.Messages.Commands;

namespace Pacs.Test.OperatorClient
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			await CreateHostBuilder(args).Build().RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddMassTransit(x =>
					{
						x.AddRequestClient<Connect>();

						x.UsingRabbitMq((context, cfg) =>
						{
							cfg.Host("localhost", 5672, "/", h =>
							{
								//h.UseCluster(cluster =>
								//{
								//    cluster.Node("localhost:5673");
								//    cluster.Node("localhost:5672");
								//});
							});

							cfg.ConfigureCoreMessageTopology(context);

							cfg.ConfigureEndpoints(context);
						});
					});

					//services.AddMassTransitHostedService(true);
					//services.AddScoped(typeof(PublishUserHeaderFilter<>), typeof(PublishUserHeaderFilter<>));
					//services.RegisterGeneric(typeof(SendUserHeaderFilter<>)).InstancePerLifetimeScope();

					services.AddHostedService<StartupService>();
				});
		}

		//public static void Main(string[] args)
		//{
		//	//setup our DI
		//	var serviceProvider = new ServiceCollection()
		//		.AddMassTransit(x =>
		//		{
		//			x.SetKebabCaseEndpointNameFormatter();

		//			x.AddRequestClient<OperatorConnectCommand>();

		//			x.UsingRabbitMq((context, cfg) =>
		//			{
		//				cfg.Host("localhost", 5672, "/", x => { });

		//				cfg.ConfigureMessageTopology(context);

		//				cfg.ConfigureEndpoints(context);
		//			});
		//		})
		//		.BuildServiceProvider();

		//	//var bus = serviceProvider.GetService<IBus>();
		//	var client = serviceProvider.GetService<IRequestClient<OperatorConnectCommand>>();

		//	var message = new OperatorConnectCommand { OperatorId = 5 };

		//	//bus.Send(message);

		//	var response = client.GetResponse<OperatorResult>(message).Result;
		//	var result = response.Message;

		//	if(result.Result == Result.Success)
		//	{
		//		var state = result.Operator;
		//		Console.WriteLine($"Оператор Id: {state.Id}");
		//		Console.WriteLine($"Состояние: {state.State}");
		//		Console.WriteLine($"Начало: {state.Started}");
		//	}
		//}
	}
}
