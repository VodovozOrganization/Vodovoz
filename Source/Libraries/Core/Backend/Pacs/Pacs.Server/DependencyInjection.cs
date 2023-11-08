using Microsoft.Extensions.DependencyInjection;

namespace Pacs.Server
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddPacsOperatorServices(this IServiceCollection services)
		{
			services
				.AddScoped<IPacsSettings, PacsStaticSettings>()
				.AddScoped<IPhoneController, PhoneController>()
				.AddScoped<IOperatorAgentFactory, OperatorAgentFactory>()
				.AddScoped<IOperatorControllerFactory, OperatorControllerFactory>()
				.AddScoped<IOperatorControllerProvider, OperatorControllerProvider>()
				.AddScoped<IOperatorNotifier, OperatorNotifier>()
				.AddScoped<IOperatorRepository, OperatorRepository>()
				.AddScoped<IPhoneRepository, PhoneRepository>()
				;

			return services;
		}
	}
}
