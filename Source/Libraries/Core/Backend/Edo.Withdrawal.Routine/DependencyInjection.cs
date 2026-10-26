using System;
using MassTransit;
using Edo.Common;
using Edo.Transport;
using Edo.Withdrawal.Routine.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Edo.Withdrawal.Routine
{
	public static class DependencyInjection
	{
		/// <summary>
		/// Добавить сервисы для вывода кодов оборота с хост-сервисами
		/// </summary>
		/// <param name="services">Коллекция сервисов</param>
		/// <returns>Коллекция сервисов</returns>
		/// <param name="configureBus">Дополнительная настройка шины в приложении, например outbox</param>
		public static IServiceCollection AddEdoWithdrawalRoutine(
			this IServiceCollection services,
			Action<IBusRegistrationConfigurator> configureBus = null)
		{
			services
				.AddEdo()
				.AddScoped<TrueMarkTimedOutDocumentsWithdrawalService>()
				.AddScoped<ITrueMarkWithdrawalCancellationService, TrueMarkWithdrawalCancellationService>()
				.AddScoped<TrueMarkDocumentsStatusUpdateService>()
				.AddEdoMassTransit(configureBus: configureBus);

			return services;
		}
	}
}
