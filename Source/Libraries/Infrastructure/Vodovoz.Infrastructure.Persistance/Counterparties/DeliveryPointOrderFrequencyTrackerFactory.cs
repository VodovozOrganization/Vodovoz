using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Tracking;
using System;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	/// <summary>
	/// Создаёт независимый трекер частоты заказов для каждой единицы работы.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyTrackerFactory : ISingleUowEventsListnerFactory
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public DeliveryPointOrderFrequencyTrackerFactory(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
			=> new DeliveryPointOrderFrequencyTracker(uow, _scopeFactory);
	}
}
