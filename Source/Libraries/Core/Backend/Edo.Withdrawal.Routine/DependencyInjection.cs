using Edo.Common;
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
				.AddEdo()
				.AddScoped<TrueMarkTimedOutDocumentsWithdrawalService>()
				.AddEdoMassTransit()
				.AddHostedService<TrueMarkTimedOutDocumentsWithdrawalWorker>();

			return services;
		}
	}
}
