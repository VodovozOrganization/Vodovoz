using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System.Collections.Generic;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		public IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses)
		{
			OrderForMovDocJournalFilterViewModel orderFilterVM = new OrderForMovDocJournalFilterViewModel();
			orderFilterVM.IsOnlineStoreOrders = IsOnlineStoreOrders;
			orderFilterVM.OrderStatuses = orderStatuses;

			OrderForMovDocJournalViewModel vm = new OrderForMovDocJournalViewModel(
				orderFilterVM,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			) {
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}

		public IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory()
		{
			ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

			var counterpartyJournalFactory = new CounterpartyJournalFactory();
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			var orderJournalFilterViewModel = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory);

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(typeof(Order),
				() => new OrderJournalViewModel(
					orderJournalFilterViewModel,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					VodovozGtkServicesConfig.EmployeeService,
					nomenclatureRepository,
					userRepository,
					new OrderSelectorFactory(),
					new EmployeeJournalFactory(),
					counterpartyJournalFactory,
					new DeliveryPointJournalFactory(),
					subdivisionJournalFactory,
					new GtkTabsOpener(),
					new UndeliveredOrdersJournalOpener(),
					new NomenclatureSelectorFactory(),
					new UndeliveredOrdersRepository()
				)
			);
		}
	}
}
