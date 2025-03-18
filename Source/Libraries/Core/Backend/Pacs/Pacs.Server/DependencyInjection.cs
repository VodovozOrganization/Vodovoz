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
				.AddScoped<IOperatorPhoneService, OperatorPhoneService>()
				.AddScoped<IOperatorRepository, OperatorRepository>()
				.AddScoped<IOperatorStateService, OperatorStateService>()
				.AddScoped<IOperatorNotifier, OperatorNotifier>()
				.AddScoped<IBreakAvailabilityNotifier, BreakAvailabilityNotifier>()
				.AddScoped<IPacsRepository, PacsRepository>()
				.AddScoped<IGlobalBreakController, GlobalBreakController>()
				.AddScoped<IOperatorBreakAvailabilityService, OperatorBreakAvailabilityService>();

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
