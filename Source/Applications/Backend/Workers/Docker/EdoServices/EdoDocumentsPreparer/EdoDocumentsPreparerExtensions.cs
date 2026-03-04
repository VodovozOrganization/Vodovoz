using EdoDocumentsPreparer.Factories;
using EdoService.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using QS.Report;
using Vodovoz.Application.Clients;
using VodovozBusiness.Controllers;

namespace EdoDocumentsPreparer
{
	public static class EdoDocumentsPreparerExtensions
	{
		public static IServiceCollection AddPreparerDependencyGroup(this IServiceCollection services)
		{
			services
				.AddScoped<PrintableDocumentSaver>()
				.AddScoped<IFileDataFactory, FileDataFactory>()
				.AddScoped<IInfoForCreatingBillWithoutShipmentEdoFactory, InfoForCreatingBillWithoutShipmentEdoFactory>()
				.AddScoped<IInfoForCreatingEdoBillFactory, InfoForCreatingEdoBillFactory>()
				.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>()
				.AddScoped<ICounterpartyEdoAccountController, CounterpartyEdoAccountController>();
				
			return services;
		}
	}
}
