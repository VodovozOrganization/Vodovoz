using Autofac;
using QS.Project.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Dialogs.Counterparties;

namespace Vodovoz.Factories
{
	public class DeliveryPointViewModelFactory : IDeliveryPointViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public DeliveryPointViewModelFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));
		}

		public DeliveryPointViewModel GetForOpenDeliveryPointViewModel(int id)
		{
			return _lifetimeScope.Resolve<DeliveryPointViewModel>(
				new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(id)));
		}

		public DeliveryPointViewModel GetForCreationDeliveryPointViewModel(Counterparty client)
		{
			return _lifetimeScope.Resolve<DeliveryPointViewModel>(
				new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate()),
				new TypedParameter(typeof(Counterparty), client));
		}
	}
}
