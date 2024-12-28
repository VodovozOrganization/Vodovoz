using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Edo.TaskValidation
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoTaskValidation(this IServiceCollection services)
		{
			services.AddValidatorsFromAssembly();
			services.AddSingleton<EdoTaskValidatorsPersister>();

			services.AddScoped<EdoTaskValidatorsProvider>();
			services.AddScoped<EdoTaskMainValidator>();

			return services;
		}

		private static IServiceCollection AddValidatorsFromAssembly(this IServiceCollection services)
		{
			var validatorTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract)
				.Where(type => typeof(IEdoTaskValidator).IsAssignableFrom(type));

			foreach(var validatorType in validatorTypes)
			{
				services.AddSingleton(typeof(IEdoTaskValidator), validatorType);
			}

			return services;
		}
	}
}
