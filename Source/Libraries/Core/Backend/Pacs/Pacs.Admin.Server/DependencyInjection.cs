using Microsoft.Extensions.DependencyInjection;
using Pacs.Core;

namespace Pacs.Admin.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsAdminServices(this IServiceCollection services)
		{
			services.AddControllers();

			services.AddScoped<ISettingsNotifier, SettingsNotifier>();

			services.AddPacsMassTransit((context, cfg) =>
			{
				cfg.AddAdminProducerTopology(context);
			});

			return services;
		}
	}
}
