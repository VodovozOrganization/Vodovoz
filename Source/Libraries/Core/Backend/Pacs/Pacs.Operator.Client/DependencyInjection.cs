using Microsoft.Extensions.DependencyInjection;
using Pacs.Admin.Client;
using Pacs.Calls;
using Pacs.Core.Messages.Events;
using System;

namespace Pacs.Operators.Client
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorClient(this IServiceCollection services)
		{
			services
				.AddSingleton<IOperatorClient, OperatorClient>()
				.AddSingleton<OperatorKeepAliveController>()
				.AddScoped<IOperatorClientFactory, OperatorClientFactory>()
				.AddSingleton<OperatorStateConsumer>()
				.AddSingleton<IObservable<OperatorStateEvent>>(ctx => ctx.GetService<OperatorStateConsumer>())

				.AddSingleton<PacsCallEventConsumer>()
				.AddSingleton<IObservable<Vodovoz.Core.Domain.Pacs.CallEvent>>(ctx => ctx.GetService<PacsCallEventConsumer>())

				.AddSingleton<GlobalBreakAvailabilityConsumer>()
				.AddSingleton<IObservable<GlobalBreakAvailability>>(ctx => ctx.GetService<GlobalBreakAvailabilityConsumer>())

				.AddSingleton<OperatorsOnBreakConsumer>()
				.AddSingleton<IObservable<OperatorsOnBreakEvent>>(ctx => ctx.GetService<OperatorsOnBreakConsumer>())

				.AddSingleton<OperatorSettingsConsumer>()

				//.AddSingleton<OperatorStateConsumer>()
				//.AddSingleton<IConsumer<OperatorState>, OperatorStateConsumer>()
				//.AddSingleton<IObservable<OperatorState>, OperatorStateConsumer>()
				;

			


			//services.AddMassTransitServices(x =>
			//{
			//	x.AddConsumers(Assembly.GetAssembly(typeof(OperatorStateConsumerDefinition)));
			//	x.AddConsumers(Assembly.GetAssembly(typeof(PacsCallEventConsumerDefinition)));
			//	additionalRegistrations.Invoke(x);

			//	x.UsingRabbitMq((context, cfg) =>
			//	{
			//		var transportSettings = context.GetService<IMessageTransportSettings>();
			//		cfg.Host(
			//			transportSettings.Host,
			//			(ushort)transportSettings.Port,
			//			transportSettings.VirtualHost,
			//			hostCfg =>
			//			{
			//				hostCfg.Username(transportSettings.Username);
			//				hostCfg.Password(transportSettings.Password);
			//				if(transportSettings.UseSSL)
			//				{
			//					hostCfg.UseSsl(ssl =>
			//					{
			//						ssl.Protocol = SslProtocols.Tls12;
			//					});
			//				}
			//			}
			//		);

			//		cfg.ConfigureOperatorPublishTopology(context);

			//		cfg.ConfigureEndpoints(context);
			//	});
			//});

			return services;
		}

		
	}
}
