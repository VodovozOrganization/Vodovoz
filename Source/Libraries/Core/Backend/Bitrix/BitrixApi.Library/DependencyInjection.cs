using BitrixApi.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using QS.Report;
using Vodovoz.Infrastructure.Persistance;

namespace BitrixApi.Library
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddBitrixApiServices(this IServiceCollection services)
		{
			services
				.AddInfrastructure()
				.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>()
				.AddScoped<IEmailAttachmentsCreateService, EmailAttachmentsCreateService>();

			return services;
		}
	}
}
