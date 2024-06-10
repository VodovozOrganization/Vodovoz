using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Pacs.Core;
using Pacs.MangoCalls.Services;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Repositories;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.MangoCalls
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsMangoCallsServices(this IServiceCollection services)
		{
			services
				.AddSingleton<IPacsRepository, PacsRepository>()
				.AddScoped<ICallEventRegistrar, CallEventRegistrar>()
				.AddScoped<CallEventHandler>()
				.AddScoped<CallEventHandlerFactory>()

				.AddPacsMassTransit(
					(context, cfg) =>
					{
						cfg.AddCallsTopology(context);
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
