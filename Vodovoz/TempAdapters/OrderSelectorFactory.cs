using QS.Dialog.GtkUI.FileDialog;
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
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private readonly OrderJournalFilterViewModel _orderJournalFilter;

		public OrderSelectorFactory(OrderJournalFilterViewModel orderFilter = null)
		{
			_orderJournalFilter = orderFilter;
		}

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
			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				CreateOrderJournalViewModel
			);
		}

		public IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector()
		{
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var counterpartyJournalFactory = new CounterpartyJournalFactory();
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictStatus = OrderStatus.WaitForPayment,
						x => x.AllowPaymentTypes = new[] { PaymentType.cash },
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictWithoutSelfDelivery = false,
						x => x.RestrictHideService = true,
						x => x.RestrictOnlyService = false);

					return new OrderJournalViewModel(
						filter,
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
						new NomenclatureJournalFactory(),
						new UndeliveredOrdersRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new FileDialogService());
				});
		}

		public IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector()
		{

			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var counterpartyJournalFactory = new CounterpartyJournalFactory();
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictStatus = OrderStatus.OnLoading
					);

					return new OrderJournalViewModel(
						filter,
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
						new NomenclatureJournalFactory(),
						new UndeliveredOrdersRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new FileDialogService());
				});
		}

		public OrderJournalViewModel CreateOrderJournalViewModel()
		{
			var subdivisionJournalFactory = new SubdivisionJournalFactory();
			var counterpartyJournalFactory = new CounterpartyJournalFactory();
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			return new OrderJournalViewModel(
				_orderJournalFilter ?? new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory),
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
				new NomenclatureJournalFactory(),
				new UndeliveredOrdersRepository(),
				new SubdivisionRepository(new ParametersProvider()),
				new FileDialogService()
			);
		}
	}
}
