using ExportTo1c.Library.Factories;
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
	}
}
