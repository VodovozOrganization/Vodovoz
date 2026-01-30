using ExportTo1c.Library.Factories;
using ExportTo1c.Library.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExportTo1c.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddExportTo1c(this IServiceCollection services)
		{
			services.TryAddScoped<IDataExporterFor1cFactory, DataExporterFor1cFactory>();

			return services;
		}

		public static IServiceCollection AddExportTo1cApi(this IServiceCollection services)
		{
			services.TryAddScoped<IApi1cChangesExporterFactory, Api1cChangesExporterFactory>();
			services.TryAddScoped<IOrderTo1cExportRepository, OrderTo1cExportRepository>();
			services.AddHttpClient();

			return services;
		}
	}
}
