using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.Core;
using Pacs.MangoCalls.Services;
using System.Reflection;
using Vodovoz.Settings.Database.Pacs;
using Vodovoz.Settings.Pacs;

namespace Pacs.MangoCalls
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsMangoCallsServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IPacsSettings, PacsSettings>()
				.AddSingleton<ICallEventSequenceValidator, CallEventSequenceValidator>()
				.AddScoped<ICallEventRegistrar, CallEventRegistrar>()

				.AddPacsMassTransit(
					(context, cfg) =>
					{
						cfg.AddCallsProducerTopology(context);
					},
					(busCfg) =>
					{
						busCfg.AddConsumers(Assembly.GetExecutingAssembly());
					}
				)
			;

			return services;
		}
	}
}
