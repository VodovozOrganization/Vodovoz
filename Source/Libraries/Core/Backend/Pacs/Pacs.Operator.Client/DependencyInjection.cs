using Microsoft.Extensions.DependencyInjection;
using Pacs.Calls.Consumers;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client.Consumers;
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
				.AddSingleton<IObservable<PacsCallEvent>>(ctx => ctx.GetService<PacsCallEventConsumer>())

				.AddSingleton<GlobalBreakAvailabilityConsumer>()
				.AddSingleton<IObservable<GlobalBreakAvailabilityEvent>>(ctx => ctx.GetService<GlobalBreakAvailabilityConsumer>())

				.AddSingleton<OperatorsOnBreakConsumer>()
				.AddSingleton<IObservable<OperatorsOnBreakEvent>>(ctx => ctx.GetService<OperatorsOnBreakConsumer>())

				.AddSingleton<OperatorSettingsConsumer>()
				;

			return services;
		}
	}
}
