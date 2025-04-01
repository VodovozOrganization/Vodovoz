using Microsoft.Extensions.DependencyInjection;
using TaxcomEdoApi.Library.Converters.Format5_01;
using TaxcomEdoApi.Library.Converters.Format5_03;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Factories.Format5_01;
using TaxcomEdoApi.Library.Factories.Format5_03;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Library
{
	public static class TaxcomEdoApiLibraryExtensions
	{
		public static IServiceCollection AddTaxcomEdoApiLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoTaxcomDocumentsFactory5_01, EdoTaxcomDocumentsFactory5_01>()
				.AddScoped<IEdoTaxcomDocumentsFactory5_03, EdoTaxcomDocumentsFactory5_03>()
				.AddScoped<IParticipantDocFlowConverter5_01, ParticipantDocFlowConverter5_01>()
				.AddScoped<IParticipantDocFlowConverter5_03, ParticipantDocFlowConverter5_03>()
				.AddScoped<IUpdProductConverter5_01, UpdProductConverter5_01>()
				.AddScoped<IUpdProductConverter5_03, UpdProductConverter5_03>()
				.AddScoped<IErpDocumentInfoConverter5_01, ErpDocumentInfoConverter5_01>()
				.AddScoped<IErpDocumentInfoConverter5_03, ErpDocumentInfoConverter5_03>()
				.AddScoped<ITaxcomEdoService, TaxcomEdoService>()
				.AddScoped<IEdoBillFactory, EdoBillFactory>();
			
			return services;
		}
	}
}
