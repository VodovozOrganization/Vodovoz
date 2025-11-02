using Autofac;
using QS.DomainModel.UoW;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Widgets;

namespace Vodovoz.ViewModels.Factories
{
	public interface IUndeliveredOrderViewModelFactory
	{
		UndeliveredOrderViewModel CreateUndeliveredOrderViewModel(UndeliveredOrder undeliveredOrder, ILifetimeScope scope, ITdiTab tab, IUnitOfWork uow);
	}
}
