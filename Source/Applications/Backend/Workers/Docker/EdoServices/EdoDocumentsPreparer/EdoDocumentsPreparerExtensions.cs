using EdoService.Library.Services;
using EdoService.Library.Factories;
using Microsoft.Extensions.DependencyInjection;
using QS.Report;
using Vodovoz.Core.Application.Clients;
using VodovozBusiness.Controllers;

namespace EdoDocumentsPreparer
{
	public static class EdoDocumentsPreparerExtensions
	{
		public static IServiceCollection AddPreparerDependencyGroup(this IServiceCollection services)
		{
			services
				.AddScoped<PrintableDocumentSaver>()
				.AddScoped<IBillEdoRequestFactory, BillEdoRequestFactory>()
				.AddScoped<IBillEdoRequestSender, BillEdoRequestSender>()
				.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>()
				.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>();
				
			return services;
		}
	}
}
