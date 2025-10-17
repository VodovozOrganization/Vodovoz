using BitrixApi.Library.Services;
using Mailganer.Api.Client;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.UoW;
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
				.AddScoped<IEmailAttachmentsCreateService, EmailAttachmentsCreateService>()
				.AddMailganerApiClient()
				.AddScoped<EmailDirectSender>()
				.AddScoped<IUnitOfWork>(sp => sp.GetService<IUnitOfWorkFactory>().CreateWithoutRoot(nameof(BitrixApi)))
				.AddScoped<EmalSendService>();

			return services;
		}
	}
}
