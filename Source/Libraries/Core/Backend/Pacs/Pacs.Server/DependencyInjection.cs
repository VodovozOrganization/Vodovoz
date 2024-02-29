﻿using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.Core;
using Pacs.Server.Breaks;
using Pacs.Server.Consumers.Definitions;
using Pacs.Server.Operators;
using Pacs.Server.Phones;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Repositories;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Settings.Database;
using Vodovoz.Settings.Pacs;

namespace Pacs.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IPhoneController, PhoneController>()
				.AddSingleton<IOperatorRepository, OperatorRepository>()
				.AddSingleton<IOperatorAgentFactory, OperatorAgentFactory>()
				.AddSingleton<IOperatorControllerFactory, OperatorControllerFactory>()
				.AddSingleton<IOperatorControllerProvider, OperatorControllerProvider>()
				.AddSingleton<IOperatorNotifier, OperatorNotifier>()
				.AddSingleton<IPhoneRepository, PhoneRepository>()
				.AddSingleton<IBreakAvailabilityNotifier, BreakAvailabilityNotifier>()
				.AddSingleton<IPacsRepository, PacsRepository>()
				.AddSingleton<GlobalBreakController>()
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
