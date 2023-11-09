using Autofac;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
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
using Vodovoz.Infrastructure.Print;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.TempAdapters
{
	public class OrderSelectorFactory : IOrderSelectorFactory
	{
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private OrderJournalFilterViewModel _orderJournalFilter;

		public OrderSelectorFactory(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			OrderJournalFilterViewModel orderFilter = null)
		{
			_navigationManager = navigationManager ?? throw new System.ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));
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
			var unitOfWorkFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			var commonServices = _lifetimeScope.Resolve<ICommonServices>();
			var employeeService = _lifetimeScope.Resolve<IEmployeeService>();
			var nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
			var userRepository = _lifetimeScope.Resolve<IUserRepository>();
			var orderSelectorFactory = _lifetimeScope.Resolve<IOrderSelectorFactory>();
			var employeeJournalFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			var counterpartyJournalFactory = _lifetimeScope.Resolve<ICounterpartyJournalFactory>();
			var deliveryPointJournalFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			var gtkTabsOpener = _lifetimeScope.Resolve<IGtkTabsOpener>();
			var nomenclatureJournalFactory = _lifetimeScope.Resolve<INomenclatureJournalFactory>();
			var undeliveredOrdersRepository = _lifetimeScope.Resolve<IUndeliveredOrdersRepository>();
			var subdivisionRepository = _lifetimeScope.Resolve<ISubdivisionRepository>();
			var fileDialogService = _lifetimeScope.Resolve<IFileDialogService>();
			var subdivisionParametersProvider = _lifetimeScope.Resolve<ISubdivisionParametersProvider>();
			var deliveryScheduleParametersProvider = _lifetimeScope.Resolve<IDeliveryScheduleParametersProvider>();
			var rdlPreviewOpener = _lifetimeScope.Resolve<IRDLPreviewOpener>();
			var routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, _lifetimeScope);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictStatus = OrderStatus.WaitForPayment,
						x => x.AllowPaymentTypes = new[] { PaymentType.Cash },
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictWithoutSelfDelivery = false,
						x => x.RestrictHideService = true,
						x => x.RestrictOnlyService = false);

					return new OrderJournalViewModel(
						filter,
						unitOfWorkFactory,
						commonServices,
						_navigationManager,
						_lifetimeScope,
						employeeService,
						nomenclatureRepository,
						userRepository,
						orderSelectorFactory,
						employeeJournalFactory,
						counterpartyJournalFactory,
						deliveryPointJournalFactory,
						gtkTabsOpener,
						nomenclatureJournalFactory,
						undeliveredOrdersRepository,
						subdivisionRepository,
						fileDialogService,
						subdivisionParametersProvider,
						deliveryScheduleParametersProvider,
						rdlPreviewOpener,
						routeListItemRepository);
				});
		}

		public IEntityAutocompleteSelectorFactory CreateSelfDeliveryDocumentOrderAutocompleteSelector()
		{
			var unitOfWorkFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			var commonServices = _lifetimeScope.Resolve<ICommonServices>();
			var employeeService = _lifetimeScope.Resolve<IEmployeeService>();
			var nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
			var userRepository = _lifetimeScope.Resolve<IUserRepository>();
			var orderSelectorFactory = _lifetimeScope.Resolve<IOrderSelectorFactory>();
			var employeeJournalFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			var counterpartyJournalFactory = _lifetimeScope.Resolve<ICounterpartyJournalFactory>();
			var deliveryPointJournalFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			var gtkTabsOpener = _lifetimeScope.Resolve<IGtkTabsOpener>();
			var nomenclatureJournalFactory = _lifetimeScope.Resolve<INomenclatureJournalFactory>();
			var undeliveredOrdersRepository = _lifetimeScope.Resolve<IUndeliveredOrdersRepository>();
			var subdivisionRepository = _lifetimeScope.Resolve<ISubdivisionRepository>();
			var fileDialogService = _lifetimeScope.Resolve<IFileDialogService>();
			var subdivisionParametersProvider = _lifetimeScope.Resolve<ISubdivisionParametersProvider>();
			var deliveryScheduleParametersProvider = _lifetimeScope.Resolve<IDeliveryScheduleParametersProvider>();
			var rdlPreviewOpener = _lifetimeScope.Resolve<IRDLPreviewOpener>();
			var routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();

			return new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(
				typeof(Order),
				() =>
				{
					var filter = new OrderJournalFilterViewModel(counterpartyJournalFactory, deliveryPointJournalFactory, _lifetimeScope);
					filter.SetAndRefilterAtOnce(
						x => x.RestrictOnlySelfDelivery = true,
						x => x.RestrictStatus = OrderStatus.OnLoading
					);

					return new OrderJournalViewModel(
						filter,
						unitOfWorkFactory,
						commonServices,
						_navigationManager,
						_lifetimeScope,
						employeeService,
						nomenclatureRepository,
						userRepository,
						orderSelectorFactory,
						employeeJournalFactory,
						counterpartyJournalFactory,
						deliveryPointJournalFactory,
						gtkTabsOpener,
						nomenclatureJournalFactory,
						undeliveredOrdersRepository,
						subdivisionRepository,
						fileDialogService,
						subdivisionParametersProvider,
						deliveryScheduleParametersProvider,
						rdlPreviewOpener,
						routeListItemRepository);
				});
		}

		public OrderJournalViewModel CreateOrderJournalViewModel(OrderJournalFilterViewModel filterViewModel = null)
		{
			var unitOfWorkFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			var commonServices = _lifetimeScope.Resolve<ICommonServices>();
			var employeeService = _lifetimeScope.Resolve<IEmployeeService>();
			var nomenclatureRepository = _lifetimeScope.Resolve<INomenclatureRepository>();
			var userRepository = _lifetimeScope.Resolve<IUserRepository>();
			var orderSelectorFactory = _lifetimeScope.Resolve<IOrderSelectorFactory>();
			var employeeJournalFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
			var counterpartyJournalFactory = _lifetimeScope.Resolve<ICounterpartyJournalFactory>();
			var deliveryPointJournalFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			var gtkTabsOpener = _lifetimeScope.Resolve<IGtkTabsOpener>();
			var nomenclatureJournalFactory = _lifetimeScope.Resolve<INomenclatureJournalFactory>();
			var undeliveredOrdersRepository = _lifetimeScope.Resolve<IUndeliveredOrdersRepository>();
			var subdivisionRepository = _lifetimeScope.Resolve<ISubdivisionRepository>();
			var fileDialogService = _lifetimeScope.Resolve<IFileDialogService>();
			var subdivisionParametersProvider = _lifetimeScope.Resolve<ISubdivisionParametersProvider>();
			var deliveryScheduleParametersProvider = _lifetimeScope.Resolve<IDeliveryScheduleParametersProvider>();
			var rdlPreviewOpener = _lifetimeScope.Resolve<IRDLPreviewOpener>();
			var routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();

			if(filterViewModel != null)
			{
				_orderJournalFilter = filterViewModel;
			}

			return new OrderJournalViewModel(
				filterViewModel,
				unitOfWorkFactory,
				commonServices,
				_navigationManager,
				_lifetimeScope,
				employeeService,
				nomenclatureRepository,
				userRepository,
				orderSelectorFactory,
				employeeJournalFactory,
				counterpartyJournalFactory,
				deliveryPointJournalFactory,
				gtkTabsOpener,
				nomenclatureJournalFactory,
				undeliveredOrdersRepository,
				subdivisionRepository,
				fileDialogService,
				subdivisionParametersProvider,
				deliveryScheduleParametersProvider,
				rdlPreviewOpener,
				routeListItemRepository);
		}
	}
}
