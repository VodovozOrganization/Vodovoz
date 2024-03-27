using Autofac;
using QS.DomainModel.UoW;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.ViewModels.Factories
{
	public interface IUndeliveryDiscussionsViewModelFactory
	{
		UndeliveryDiscussionsViewModel CreateUndeliveryDiscussionsViewModel(UndeliveredOrder undeliveredOrder, ITdiTab parentTab, ILifetimeScope scope, IUnitOfWork uow);
	}
}
