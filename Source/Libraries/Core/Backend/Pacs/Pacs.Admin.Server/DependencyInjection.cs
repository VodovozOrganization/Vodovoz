using Microsoft.Extensions.DependencyInjection;

namespace Pacs.Admin.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsAdminServices(this IServiceCollection services)
		{
			services.AddControllers();

			services
				.AddScoped<ISettingsNotifier, SettingsNotifier>()
				;

			return services;
		}
	}
}
