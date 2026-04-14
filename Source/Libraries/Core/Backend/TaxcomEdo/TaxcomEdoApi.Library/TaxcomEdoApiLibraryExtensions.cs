using Microsoft.Extensions.DependencyInjection;
using TaxcomEdoApi.Library.Converters.Format5_03;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Factories.Format5_03;
using TaxcomEdoApi.Library.Parsers;
using TaxcomEdoApi.Library.Providers;
using TaxcomEdoApi.Library.Services;
using TaxcomEdoApi.Library.Services.ContainerDocumentsServices;
using TaxcomEdoApi.Library.Services.Interfaces;

namespace TaxcomEdoApi.Library
{
	public static class TaxcomEdoApiLibraryExtensions
	{
		public static IServiceCollection AddTaxcomEdoApiLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoTaxcomDocumentsFactory5_03, EdoTaxcomDocumentsFactory5_03>()
				.AddScoped<IUpdProductConverter5_03, UpdProductConverter5_03>()
				.AddScoped<IErpDocumentInfoConverter5_03, ErpDocumentInfoConverter5_03>()
				.AddScoped<ITaxcomEdoService, TaxcomEdoService>()
				.AddScoped<IEdoBillFactory, EdoBillFactory>()
				.AddScoped<ContainerService>()
				.AddScoped<ISignProcessorFactory, SignProcessorFactory>()
				.AddScoped<ISignFilenameProvider, DefaultSignFilenameProvider>()
				.AddScoped<ICertificateSearcher, CertificateSearcher>()
				.AddScoped<ICertificateParser, CertificateParser>()
				.AddScoped<DocumentService>()
				.AddScoped<UniversalInvoiceContainerDocumentService>()
				.AddScoped<CustomerUniversalInvoiceContainerDocumentService>()
				.AddScoped<CancellationOfferContainerDocumentService>()
				.AddScoped<CancellationOfferResignContainerDocumentService>()
				.AddScoped<NonformalizedContainerDocumentService>()
				;
			
			return services;
		}
	}
}
