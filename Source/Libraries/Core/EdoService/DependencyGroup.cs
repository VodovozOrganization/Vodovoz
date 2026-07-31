using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception;
using Edo.Transport;
using EdoService.Library.Converters;
using EdoService.Library.Factories;
using EdoService.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vodovoz.Core.Domain.Controllers;

namespace EdoService.Library
{
	public static class DependencyGroup
	{
		public static IServiceCollection AddEdoServicesLibrary(this IServiceCollection services)
		{
			services
				.AddScoped<IEdoService, EdoService>()
				.AddScoped<IEdoLogger, EdoLogger>()
				.AddScoped<IContactListService, ContactListService>()
				.AddScoped<IAuthorizationService, TaxcomAuthorizationService>()
				.AddScoped<IContactStateConverter, ContactStateConverter>()
				.AddScoped<IInformalEdoRequestFactory, EquipmentTransferEdoRequestFactory>()
				.AddScoped<EdoTaskCustomSourcesPersister>()
				.AddScoped<EdoTaskExceptionSourcesPersister>()
				.AddScoped<EdoProblemRegistrar>()
				.AddScoped<MessageService>()
				.TryAddScoped<ICounterpartyEdoAccountEntityController, CounterpartyEdoAccountEntityController>()
				;

			return services;
		}
	}
}
