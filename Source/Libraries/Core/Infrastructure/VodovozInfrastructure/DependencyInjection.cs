using Microsoft.Extensions.DependencyInjection;
using QS.Report;
using VodovozInfrastructure.Report;

namespace VodovozInfrastructure
{
	public static class DependencyInjection {
		public static IServiceCollection AddSlaveDbPreferredReportsCore(this IServiceCollection services) {

			services.AddScoped<IReportInfoFactory, SlaveDbPreferredReportInfoFactory>();

			return services;
		}
	}
}
