using Autofac;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.Widgets;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.ViewModels.Factories
{
	public class UndeliveredOrderViewModelFactory : IUndeliveredOrderViewModelFactory
	{
		public UndeliveredOrderViewModel CreateUndeliveredOrderViewModel(UndeliveredOrder undeliveredOrder, ILifetimeScope scope, ITdiTab tab, IUnitOfWork uow)
		{
			return new UndeliveredOrderViewModel(
				undeliveredOrder,
				scope.Resolve<ICommonServices>(),
				scope.Resolve<IUnitOfWorkFactory>(),
				scope.Resolve<IUndeliveryDetalizationJournalFactory>(),
				uow,
				scope.Resolve<INavigationManager>(),
				scope,
				tab,
				scope.Resolve<IOrderRepository>(),
				scope.Resolve<IOrderSelectorFactory>(),
				scope.Resolve<IDeliveryScheduleJournalFactory>(),
				scope.Resolve<ISubdivisionRepository>(),
				scope.Resolve<IEmployeeJournalFactory>(),
				scope.Resolve<IEmployeeRepository>(),
				scope.Resolve<IGtkTabsOpener>(),
				scope.Resolve<IRouteListItemRepository>(),
				scope.Resolve<IOrderContractUpdater>()
				);
		}
	}
}
