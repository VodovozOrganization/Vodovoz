using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System.Collections.Generic;
using QS.ViewModels;
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

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private readonly INomenclatureSelectorFactory _nomenclatureSelectorFactory = new NomenclatureSelectorFactory();
		
		public IEntitySelector CreateOrderSelectorForDocument(bool isOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses)
		{
			OrderForMovDocJournalFilterViewModel orderFilterVM = new OrderForMovDocJournalFilterViewModel();
			orderFilterVM.IsOnlineStoreOrders = isOnlineStoreOrders;
			orderFilterVM.OrderStatuses = orderStatuses;
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			OrderForMovDocJournalViewModel vm = new OrderForMovDocJournalViewModel(
				journalActions,
				orderFilterVM,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			) 
			{
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}

		public IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory()
		{
			ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			IEntityAutocompleteSelectorFactory counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
					CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);

			OrderJournalFilterViewModel orderJournalFilterViewModel = new OrderJournalFilterViewModel();
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() => new OrderJournalViewModel(
					journalActions,
					orderJournalFilterViewModel,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices,
					VodovozGtkServicesConfig.EmployeeService,
					_nomenclatureSelectorFactory,
					nomenclatureRepository,
					new OrderSelectorFactory(),
					new EmployeeJournalFactory(),
					new CounterpartyJournalFactory(),
					new DeliveryPointJournalFactory(),
					subdivisionJournalFactory,
					new GtkTabsOpener(),
					new UndeliveredOrdersJournalOpener(),
					new UndeliveredOrdersRepository()
				)
			);
		}
	}
}
