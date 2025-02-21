using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Linq;
using System.Reflection;

namespace Edo.Problems
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoProblemRegistation(this IServiceCollection services)
		{
			services
				.AddCustomProblemSourcesFromAssembly()
				.AddExceptionProblemSourcesFromAssembly()
				.AddEdoTaskValidatorsFromAssembly()

				.AddSingleton<EdoTaskCustomSourcesPersister>()
				.AddSingleton<EdoTaskExceptionSourcesPersister>()
				.AddSingleton<EdoTaskValidatorsPersister>()

				.AddScoped<EdoTaskValidatorsProvider>()
				.AddScoped<EdoTaskValidator>()
				.AddScoped<EdoProblemRegistrar>()

				.TryAddScoped<IUnitOfWork>(x => x.GetService<IUnitOfWorkFactory>().CreateWithoutRoot())
			;

			return services;
		}

		private static IServiceCollection AddCustomProblemSourcesFromAssembly(this IServiceCollection services)
		{
			var validatorTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract)
				.Where(type => typeof(EdoTaskProblemCustomSource).IsAssignableFrom(type));

			foreach(var validatorType in validatorTypes)
			{
				services.AddSingleton(typeof(EdoTaskProblemCustomSource), validatorType);
			}

			return services;
		}

		private static IServiceCollection AddExceptionProblemSourcesFromAssembly(this IServiceCollection services)
		{
			var validatorTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract)
				.Where(type => typeof(EdoTaskProblemExceptionSource).IsAssignableFrom(type));

			foreach(var validatorType in validatorTypes)
			{
				services.AddSingleton(typeof(EdoTaskProblemExceptionSource), validatorType);
			}

			return services;
		}

		private static IServiceCollection AddEdoTaskValidatorsFromAssembly(this IServiceCollection services)
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
