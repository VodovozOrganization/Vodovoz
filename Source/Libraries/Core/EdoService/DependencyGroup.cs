using Edo.Problems;
using Edo.Problems.Custom;
using Edo.Problems.Custom.Sources;
using Edo.Problems.Exception;
using EdoService.Library.Converters;
using EdoService.Library.Factories;
using EdoService.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using QS.Services;

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
				.AddScoped<IUserService, UserService>()
				.AddScoped<EdoTaskCustomSourcesPersister>()
				.AddScoped<EdoTaskExceptionSourcesPersister>()
				.AddScoped<TaskHasBeenCancelledWithReason>()
				.AddScoped<EdoProblemRegistrar>()
				;

			return services;
		}
	}
}
