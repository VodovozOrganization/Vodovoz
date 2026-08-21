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
		public UndeliveredOrderViewModel CreateUndeliveredOrderViewModel(
			UndeliveredOrder undeliveredOrder,
			ILifetimeScope scope,
			ITdiTab tab,
			IUnitOfWork uow)
		{
			return scope.Resolve<UndeliveredOrderViewModel>(
				new TypedParameter(typeof(UndeliveredOrder), undeliveredOrder),
				new TypedParameter(typeof(IUnitOfWork), uow),
				new TypedParameter(typeof(ITdiTab), tab));
		}
	}
}
