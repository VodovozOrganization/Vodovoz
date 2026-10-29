using Edo.DeviationMonitoring.Options;
using Edo.DeviationMonitoring.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.DeviationMonitoring
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавить сервис мониторинга отклонений документооборота ЭДО в коллекцию сервисов
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddEdoDeviationMonitoring(this IServiceCollection services)
		{
			services.AddCoreDataRepositories();
			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			services.ConfigureOptions<ConfigureEdoDeviationMonitoringOptions>();

			services
				.AddDeviationValidatorsFromAssembly<IEdoTaskDeviationValidator>()
				.AddDeviationValidatorsFromAssembly<IEdoRequestDeviationValidator>()
				.AddDeviationValidatorsFromAssembly<IEdoTransferDeviationValidator>();

			services.TryAddScoped<EdoDeviationValidatorsProvider>();
			services.TryAddScoped<IEdoDeviationRegistrationService, EdoDeviationRegistrationService>();
			services.TryAddScoped<IEdoDeviationResolvingService, EdoDeviationResolvingService>();

			return services;
		}

		/// <summary>
		/// Регистрирует все валидаторы отклонений указанного вида,
		/// объявленные в сборке библиотеки мониторинга
		/// </summary>
		/// <typeparam name="TValidator">Интерфейс валидатора отклонений</typeparam>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		private static IServiceCollection AddDeviationValidatorsFromAssembly<TValidator>(this IServiceCollection services)
		{
			var validatorDescriptors = Assembly.GetExecutingAssembly()
				.GetTypes()
				.Where(type => type.IsClass && !type.IsAbstract)
				.Where(type => typeof(TValidator).IsAssignableFrom(type))
				.Select(type => ServiceDescriptor.Scoped(typeof(TValidator), type))
				.ToArray();

			services.TryAddEnumerable(validatorDescriptors);

			return services;
		}
	}
}
