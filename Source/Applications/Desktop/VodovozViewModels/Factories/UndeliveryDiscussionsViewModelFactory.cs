using Autofac;
using DocumentFormat.OpenXml.Office.CustomUI;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.ViewModels.Factories
{
	public class UndeliveryDiscussionsViewModelFactory : IUndeliveryDiscussionsViewModelFactory
	{
		public UndeliveryDiscussionsViewModel CreateUndeliveryDiscussionsViewModel(UndeliveredOrder undeliveredOrder, ITdiTab parentTab, ILifetimeScope scope, IUnitOfWork uow)
		{
			return new UndeliveryDiscussionsViewModel(
				undeliveredOrder,
				uow,
				parentTab,
				scope.Resolve<IEmployeeService>(),
				scope.Resolve<ICommonServices>(),
				scope.Resolve<INavigationManager>()
			);
		}
	}
}
