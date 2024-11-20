using EdoDocumentsPreparer.Factories;
using Microsoft.Extensions.DependencyInjection;
using QS.Report;

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
				.AddScoped<IReportInfoFactory, DefaultReportInfoFactory>();
				
			return services;
		}
	}
}
