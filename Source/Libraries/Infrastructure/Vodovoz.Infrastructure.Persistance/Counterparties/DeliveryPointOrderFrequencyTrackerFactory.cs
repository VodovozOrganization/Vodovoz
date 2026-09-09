using Autofac;
using Microsoft.Extensions.DependencyInjection;
using QS.DomainModel.Tracking;
using System;

namespace Vodovoz.Infrastructure.Persistance.Counterparties
{
	/// <summary>
	/// Подключает фабрику трекеров частоты заказов на время жизни контейнера Autofac.
	/// </summary>
	internal sealed class DeliveryPointOrderFrequencyTrackerFactory : ISingleUowEventsListnerFactory, IStartable, IDisposable
	{
		private readonly IServiceScopeFactory _scopeFactory;

		public DeliveryPointOrderFrequencyTrackerFactory(IServiceScopeFactory scopeFactory)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
		}

		public void Start() => SingleUowEventsTracker.RegisterSingleUowListnerFactory(this);

		public void Dispose() => SingleUowEventsTracker.UnregisterSingleUowListnerFactory(this);

		public ISingleUowEventListener CreateListnerForNewUow(IUnitOfWorkTracked uow)
			=> new DeliveryPointOrderFrequencyTracker(uow, _scopeFactory);
	}
}
