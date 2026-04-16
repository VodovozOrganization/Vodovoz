using Edo.Common;
using Edo.Problem.Routine.Options;
using Edo.Problem.Routine.Services;
using Edo.Transport;
using MessageTransport;
using Microsoft.Extensions.DependencyInjection;
using QS.Project.Core;
using Vodovoz.Core.Data.NHibernate;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Infrastructure.Persistance;

namespace Edo.Problem.Routine
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавить сервисы обработки проблем в ЭДО в коллекцию сервисов
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddEdoProblemRoutine(this IServiceCollection services)
		{
			services
				.AddCoreDataRepositories()
				.AddCore()
				.AddEdo()
				.AddMessageTransportSettings()
				.AddEdoMassTransit();

			services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

			services
				.AddOrderSelfDeliveryPaidProblem();

			return services;
		}

		private static IServiceCollection AddOrderSelfDeliveryPaidProblem(this IServiceCollection services)
		{
			services.ConfigureOptions<ConfigureOrderSelfDeliveryPaidProblemWorkerOptions>();
			services.AddScoped<OrderSelfDeliveryPaidProblemService>();

			return services;
		}
	}
}
