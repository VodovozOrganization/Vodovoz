using Edo.Problems.Custom;
using Edo.Problems.Exception;
using Edo.Problems.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QS.DomainModel.UoW;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Controllers;

namespace Edo.Problems
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddEdoProblemRegistration(this IServiceCollection services)
		{
			services.TryAddScoped<IUnitOfWork>(x => x.GetService<IUnitOfWorkFactory>().CreateWithoutRoot());

			services
				.AddCustomProblemSourcesFromAssembly()
				.AddExceptionProblemSourcesFromAssembly()
				.AddEdoTaskValidatorsFromAssembly()
				;

			services.TryAddSingleton<EdoTaskCustomSourcesPersister>();
			services.TryAddSingleton<EdoTaskExceptionSourcesPersister>();
			services.TryAddSingleton<EdoTaskValidatorsPersister>();

			services.TryAddScoped<EdoTaskValidatorsProvider>();
			services.TryAddScoped<EdoTaskValidator>();
			services.TryAddScoped<EdoProblemRegistrar>();
			services.TryAddScoped<ICounterpartyEdoAccountEntityController, CounterpartyEdoAccountEntityController>();

			return services;
		}

		private static IServiceCollection AddCustomProblemSourcesFromAssembly(this IServiceCollection services)
		{
			var validatorTypes = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract)
				.Where(type => typeof(EdoTaskProblemCustomSource).IsAssignableFrom(type));

			services.RemoveAll(typeof(EdoTaskProblemCustomSource));
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

			services.RemoveAll(typeof(EdoTaskProblemExceptionSource));
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

			services.RemoveAll(typeof(IEdoTaskValidator));
			foreach(var validatorType in validatorTypes)
			{
				services.AddSingleton(typeof(IEdoTaskValidator), validatorType);
			}

			return services;
		}
	}
}
