using Microsoft.Extensions.DependencyInjection;
using TaxcomEdoApi.Library.Converters;
using TaxcomEdoApi.Library.Factories;
using TaxcomEdoApi.Library.Services;

namespace TaxcomEdoApi.Library
{
	public static class TaxcomEdoApiLibraryExtensions
	{
		public static IServiceCollection AddTaxcomEdoApiLibrary(this IServiceCollection services)
		{
			services	
				.AddScoped<IEdoTaxcomDocumentsFactory, EdoTaxcomDocumentsFactory>()
				.AddScoped<IEdoBillFactory, EdoBillFactory>()
				.AddScoped<IParticipantDocFlowConverter, ParticipantDocFlowConverter>()
				.AddScoped<IUpdProductConverter, UpdProductConverter>()
				.AddScoped<IErpDocumentInfoConverter, ErpDocumentInfoConverter>()
				.AddScoped<ITaxcomEdoService, TaxcomEdoService>();
			
			return services;
		}
	}
}
