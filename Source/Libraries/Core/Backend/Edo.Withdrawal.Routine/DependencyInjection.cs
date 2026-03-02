using Edo.Transport;
using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Edo.Withdrawal.Routine
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавить сервисы для вывода кодов оборота с хост-сервисами
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		public static IServiceCollection AddEdoWithdrawalRoutine(this IServiceCollection services)
		{
			services
				.AddEdoWithdrawalRoutineServices()
				.AddHostedService<TrueMarkTimedOutDocumentsWithdrawalWorker>();

			return services;
		}

		private static IServiceCollection AddEdoWithdrawalRoutineServices(this IServiceCollection services)
		{
			services.TryAddScoped<DocflowTimeoutCheckService>();
			services.AddEdoMassTransit();

			return services;
		}
	}
}
