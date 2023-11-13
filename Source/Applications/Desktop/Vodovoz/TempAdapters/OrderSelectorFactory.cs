using Autofac;
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
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private readonly INavigationManager _navigationManager;
		private OrderJournalFilterViewModel _orderJournalFilter;

		public OrderSelectorFactory(INavigationManager navigationManager, OrderJournalFilterViewModel orderFilter = null)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
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

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, scope);
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
						_navigationManager,
						scope,
						VodovozGtkServicesConfig.EmployeeService,
						nomenclatureRepository,
						userRepository,
						new OrderSelectorFactory(_navigationManager),
						new EmployeeJournalFactory(_navigationManager),
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
						new RouteListItemRepository(),
						Startup.AppDIContainer.BeginLifetimeScope().Resolve<INomenclatureSettings>());
				});
		}

		public IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector()
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var counterpartyJournalFactory = new CounterpartyJournalFactory(scope);
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, scope);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictStatus = OrderStatus.OnLoading
					);

					return new OrderJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						_navigationManager,
						scope,
						VodovozGtkServicesConfig.EmployeeService,
						nomenclatureRepository,
						userRepository,
						new OrderSelectorFactory(_navigationManager),
						new EmployeeJournalFactory(_navigationManager),
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
						new RouteListItemRepository(),
						Startup.AppDIContainer.BeginLifetimeScope().Resolve<INomenclatureSettings>());
				});
		}

		public OrderJournalViewModel CreateOrderJournalViewModel(OrderJournalFilterViewModel filterViewModel = null)
		{
			var scope = Startup.AppDIContainer.BeginLifetimeScope();

			var counterpartyJournalFactory = new CounterpartyJournalFactory(scope);
			var deliveryPointJournalFactory = new DeliveryPointJournalFactory();
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			if(filterViewModel != null)
			{
				_orderJournalFilter = filterViewModel;
			}

			return new OrderJournalViewModel(
				_orderJournalFilter
					?? new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, scope),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_navigationManager,
				scope,
				VodovozGtkServicesConfig.EmployeeService,
				nomenclatureRepository,
				userRepository,
				new OrderSelectorFactory(_navigationManager),
				new EmployeeJournalFactory(_navigationManager),
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
				new RouteListItemRepository(),
				Startup.AppDIContainer.BeginLifetimeScope().Resolve<INomenclatureSettings>());
		}
	}
}
