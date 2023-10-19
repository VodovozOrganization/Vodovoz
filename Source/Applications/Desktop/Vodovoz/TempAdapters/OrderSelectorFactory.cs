using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System.Collections.Generic;
using Vodovoz.Core;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private OrderJournalFilterViewModel _orderJournalFilter;

		public OrderSelectorFactory(OrderJournalFilterViewModel orderFilter = null)
		{
			_orderJournalFilter = orderFilter;
		}

		public IEntitySelector CreateOrderSelectorForDocument(bool IsOnlineStoreOrders, IEnumerable<OrderStatus> orderStatuses)
		{
			var orderFilter = new OrderForMovDocJournalFilterViewModel();
			orderFilter.IsOnlineStoreOrders = IsOnlineStoreOrders;
			orderFilter.OrderStatuses = orderStatuses;

			var vm = new OrderForMovDocJournalViewModel(
				orderFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			) {
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}

		public IEntityAutocompleteSelectorFactory CreateOrderAutocompleteSelectorFactory(OrderJournalFilterViewModel filterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() => CreateOrderJournalViewModel(filterViewModel)
			);
		}

		public IEntityAutocompleteSelectorFactory CreateCashSelfDeliveryOrderAutocompleteSelector()
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();
			var counterpartyJournalFactory = new CounterpartyJournalFactory(scope);
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();
			var employeeJournalFactory = new EmployeeJournalFactory();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictStatus = OrderStatus.WaitForPayment,
						x => x.AllowPaymentTypes = new[] { PaymentType.Cash },
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictWithoutSelfDelivery = false,
						x => x.RestrictHideService = true,
						x => x.RestrictOnlyService = false);

					return new OrderJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						scope,
						VodovozGtkServicesConfig.EmployeeService,
						nomenclatureRepository,
						userRepository,
						new OrderSelectorFactory(),
						new EmployeeJournalFactory(),
						counterpartyJournalFactory,
						new DeliveryPointJournalFactory(),
						new GtkTabsOpener(),
						new NomenclatureJournalFactory(),
						new UndeliveredOrdersRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new FileDialogService(),
						new SubdivisionParametersProvider(new ParametersProvider()),
						new DeliveryScheduleParametersProvider(new ParametersProvider()),
						new RdlPreviewOpener(),
						new RouteListItemRepository());
				});
		}

		public IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector()
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var counterpartyJournalFactory = new CounterpartyJournalFactory(scope);
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();
			var employeeJournalFactory = new EmployeeJournalFactory();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictStatus = OrderStatus.OnLoading
					);

					return new OrderJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						scope,
						VodovozGtkServicesConfig.EmployeeService,
						nomenclatureRepository,
						userRepository,
						new OrderSelectorFactory(),
						new EmployeeJournalFactory(),
						counterpartyJournalFactory,
						new DeliveryPointJournalFactory(),
						new GtkTabsOpener(),
						new NomenclatureJournalFactory(),
						new UndeliveredOrdersRepository(),
						new SubdivisionRepository(new ParametersProvider()),
						new FileDialogService(),
						new SubdivisionParametersProvider(new ParametersProvider()),
						new DeliveryScheduleParametersProvider(new ParametersProvider()),
						new RdlPreviewOpener(),
						new RouteListItemRepository());
				});
		}

		public OrderJournalViewModel CreateOrderJournalViewModel(OrderJournalFilterViewModel filterViewModel = null)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var counterpartyJournalFactory = new CounterpartyJournalFactory(scope);
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();
			var employeeJournalFactory = new EmployeeJournalFactory();

			if(filterViewModel != null)
			{
				_orderJournalFilter = filterViewModel;
			}

			return new OrderJournalViewModel(
				_orderJournalFilter
					?? new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, employeeJournalFactory),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				scope,
				VodovozGtkServicesConfig.EmployeeService,
				nomenclatureRepository,
				userRepository,
				new OrderSelectorFactory(),
				new EmployeeJournalFactory(),
				counterpartyJournalFactory,
				new DeliveryPointJournalFactory(),
				new GtkTabsOpener(),
				new NomenclatureJournalFactory(),
				new UndeliveredOrdersRepository(),
				new SubdivisionRepository(new ParametersProvider()),
				new FileDialogService(),
				new SubdivisionParametersProvider(new ParametersProvider()),
				new DeliveryScheduleParametersProvider(new ParametersProvider()),
				new RdlPreviewOpener(),
				new RouteListItemRepository());
		}
	}
}
