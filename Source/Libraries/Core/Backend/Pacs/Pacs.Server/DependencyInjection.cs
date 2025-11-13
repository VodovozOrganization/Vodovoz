using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.Core;
using Pacs.Server.Breaks;
using Pacs.Server.Consumers.Definitions;
using Pacs.Server.Operators;
using Pacs.Server.Phones;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Repositories;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IOperatorPhoneService, OperatorPhoneService>()
				.AddSingleton<IOperatorRepository, OperatorRepository>()
				.AddSingleton<IOperatorServerStateMachineFactory, OperatorServerStateMachineFactory>()
				.AddSingleton<IOperatorStateService, OperatorStateService>()
				.AddSingleton<IOperatorNotifier, OperatorNotifier>()
				.AddSingleton<IPhoneRepository, PhoneRepository>()
				.AddSingleton<IBreakAvailabilityNotifier, BreakAvailabilityNotifier>()
				.AddSingleton<IPacsRepository, PacsRepository>()
				.AddSingleton<IGlobalBreakController, GlobalBreakController>()
				.AddSingleton<IOperatorBreakAvailabilityService, OperatorBreakAvailabilityService>();
				;

			services.AddPacsMassTransit(
				(context, rabbitCfg) =>
				{
					rabbitCfg.AddOperatorProducerTopology(context);
				},
				(busCfg) =>
				{
					busCfg.AddConsumers(Assembly.GetAssembly(typeof(PacsServerCallEventConsumerDefinition)));
				}
			);

			return services;
		}
	}
}
