using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Employee;
using Vodovoz.Settings.Organizations;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListAnalysisViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly IEmployeeService _employeeService;
		private readonly IWageParameterService _wageParameterService;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ISubdivisionSettings _subdivisionSettings;
		private readonly ICommonServices _commonServices;

		#region Constructor

		public RouteListAnalysisViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			IOrderSelectorFactory orderSelectorFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IDeliveryShiftRepository deliveryShiftRepository,
			IEmployeeSettings employeeSettings,
			IEmployeeService employeeService,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			IRouteListItemRepository routeListItemRepository,
			IWageParameterService wageParameterService,
			ISubdivisionSettings subdivisionSettings,
			IFileDialogService fileDialogService,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base (uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = 
				deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_subdivisionSettings =
				subdivisionSettings ?? throw new ArgumentNullException(nameof(subdivisionSettings));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService)); _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_currentPermissionService = currentPermissionService;

			UndeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));

			if(deliveryShiftRepository == null)
			{
				throw new ArgumentNullException(nameof(deliveryShiftRepository));
			}

			if(routeListItemRepository == null)
			{
				throw new ArgumentNullException(nameof(routeListItemRepository));
			}

			DeliveryShifts = deliveryShiftRepository.ActiveShifts(UoW);
			Entity.ObservableAddresses.PropertyOfElementChanged += ObservableAddressesOnPropertyOfElementChanged;
			
			CurrentEmployee = _employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);
			CanCreateRouteListWithoutOrders = _currentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders);

			if(CurrentEmployee == null) {
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете открыть " +
				             "диалог разбора МЛ, так как некого указывать в качестве логиста.", "Невозможно открыть разбор МЛ");
			}
			
			LogisticanSelectorFactory = _employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = _employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			ForwarderSelectorFactory = _employeeJournalFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();
			CarEntryViewModel = BuildCarEntryViewModel();

			TabName = $"Диалог разбора {Entity.Title}";
			
			ValidationContext.Items.Add(nameof(IRouteListItemRepository), routeListItemRepository);
			ValidationContext.Items.Add(Core.Domain.Permissions.LogisticPermissions.RouteList.CanCreateRouteListWithoutOrders, CanCreateRouteListWithoutOrders);
			
		}
		
		#endregion
		
		#region Properties

		public IEntityAutocompleteSelectorFactory LogisticanSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }
		public IUndeliveredOrdersRepository UndeliveredOrdersRepository { get; }
		public IEntityEntryViewModel CarEntryViewModel { get; }

		public readonly IList<DeliveryShift> DeliveryShifts;
		
		public Employee CurrentEmployee { get; }

		public RouteListItem SelectedItem { get; set; }

		public bool CanEditRouteList => PermissionResult.CanUpdate;

		public bool CanCreateRouteListWithoutOrders { get; }
		
		#endregion

		public Action UpdateTreeAddresses;

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<RouteList>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			var viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private void ObservableAddressesOnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LateArrivalReason))
				SelectedItem.LateArrivalReasonAuthor = CurrentEmployee;
		}

		#region Commands

		private DelegateCommand openOrderCommand;
		public DelegateCommand OpenOrderCommand => openOrderCommand ?? (openOrderCommand = new DelegateCommand(
			() => {
				_gtkDialogsOpener.OpenOrderDlgAsSlave(this, SelectedItem.Order);
			}, () => SelectedItem != null
		));
		
		private DelegateCommand openUndeliveredOrderCommand;
		public DelegateCommand OpenUndeliveredOrderCommand => 
			openUndeliveredOrderCommand ?? (openUndeliveredOrderCommand = new DelegateCommand(
				() => {
					var page = NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(this, filter =>
					{
						filter.HidenByDefault = true;
						filter.RestrictOldOrder = SelectedItem.Order;
						filter.RestrictOldOrderStartDate = SelectedItem.Order.DeliveryDate;
						filter.RestrictOldOrderEndDate = SelectedItem.Order.DeliveryDate;
					});

					page.PageClosed += (s,e) => UpdateTreeAddresses?.Invoke();
				}, 
				() => SelectedItem != null
			)
		);
		
		private DelegateCommand createFineCommand;
		public DelegateCommand CreateFineCommand => createFineCommand ?? (createFineCommand = new DelegateCommand(
			() => {
				var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

				page.ViewModel.SetRouteListById(SelectedItem.RouteList.Id);

				var undeliveredOrder = GetUndeliveredOrder();

				if (undeliveredOrder != null)
				{
					page.ViewModel.UndeliveredOrder = undeliveredOrder;
				}

				if(SelectedItem.CalculateTimeLateArrival() != null)
					page.ViewModel.FineReasonString = $"Опоздание по заказу №{SelectedItem.Order.Id} от {SelectedItem.Order.DeliveryDate:d}";

				page.ViewModel.EntitySaved += (sender, args) =>
				{
					SelectedItem.AddFine(args.Entity as Fine);
					UpdateTreeAddresses?.Invoke();
				}; 
			}, () => SelectedItem != null
		));
		
		private DelegateCommand attachFineCommand;
		public DelegateCommand AttachFineCommand => attachFineCommand ?? (attachFineCommand = new DelegateCommand(
			() =>
			{
				var page = NavigationManager.OpenViewModel<FinesJournalViewModel, Action<FineFilterViewModel>>(
					this,
					filter =>
					{
						filter.CanEditFilter = false;
						filter.ExcludedIds = SelectedItem.Fines.Select(x => x.Id).ToArray();
					});

				page.ViewModel.SelectionMode = JournalSelectionMode.Single;
				page.ViewModel.OnSelectResult +=
					(sender, e) =>
					{
						var selectedObject = e.SelectedObjects.FirstOrDefault();
						
						if(!(selectedObject is FineJournalNode selectedNode))
						{
							return;
						}

						var fine = UoW.GetById<Fine>(selectedNode.Id);

						var undeliveredOrder = GetUndeliveredOrder();
						if(undeliveredOrder != null)
						{
							fine.UndeliveredOrder = undeliveredOrder;
							UoW.Save(fine);
						}

						SelectedItem.AddFine(fine);
						UpdateTreeAddresses?.Invoke();
					};
			}, () => SelectedItem != null
		));
		
		private DelegateCommand detachAllFinesCommand;
		public DelegateCommand DetachAllFinesCommand => 
			detachAllFinesCommand ?? (detachAllFinesCommand = new DelegateCommand(
			() =>
			{
				SelectedItem.RemoveAllFines();
				UpdateTreeAddresses?.Invoke();
			},
			() => SelectedItem != null
		));
		
		private DelegateCommand createDetachFineCommand;

		public DelegateCommand DetachFineCommand => 
			createDetachFineCommand ?? (createDetachFineCommand = new DelegateCommand(
			() =>
			{
				var page = NavigationManager.OpenViewModel<FinesJournalViewModel, Action<FineFilterViewModel>>(
					this,
					filter =>
					{
						filter.CanEditFilter = false;
						filter.ExcludedIds = SelectedItem.Fines.Select(x => x.Id).ToArray();
					});

				page.ViewModel.SelectionMode = JournalSelectionMode.Single;
				page.ViewModel.OnSelectResult +=
					(sender, e) =>
					{
						var selectedObject = e.SelectedObjects.FirstOrDefault();

						if(!(selectedObject is FineJournalNode selectedNode))
						{
							return;
						}

						var fine = UoW.GetById<Fine>(selectedNode.Id);

						SelectedItem.RemoveFine(fine);
						UpdateTreeAddresses?.Invoke();
					};
			}, () => SelectedItem != null
		));
		 
		#endregion

		public string UpdateBottlesSummaryInfo()
		{
			string bottles = null;
			int completedBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.Completed)
												   .Sum(x => x.Order.Total19LBottlesToDeliver);
			int canceledBottles = Entity.Addresses.Where(
				x => x != null && (x.Status == RouteListItemStatus.Canceled
				                   || x.Status == RouteListItemStatus.Overdue
				                   || x.Status == RouteListItemStatus.Transfered)
			).Sum(x => x.Order.Total19LBottlesToDeliver);
			int enrouteBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.EnRoute)
												 .Sum(x => x.Order.Total19LBottlesToDeliver);
			bottles = "<b>Всего 19л. бутылей в МЛ:</b>\n";
			bottles += $"Выполнено: <b>{completedBottles}</b>\n";
			bottles += $" Отменено: <b>{canceledBottles}</b>\n";
			bottles += $" Осталось: <b>{enrouteBottles}</b>\n";
			
			return bottles;
		}

		private UndeliveredOrder GetUndeliveredOrder() =>
			UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, SelectedItem.Order.Id).SingleOrDefault();

		public bool AskSaveOnClose => CanEditRouteList;

		public ILifetimeScope LifetimeScope => _lifetimeScope;

		protected override bool BeforeSave()
		{
			SetLogisticianCommentAuthor();
			Entity.CalculateWages(_wageParameterService);
			return true;
		}

		public void SaveWithClose()
		{
			Save();
			Close(false, CloseSource.Save);
		}

		protected override void AfterSave()
		{
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, Entity);
			UoW.Save(Entity.RouteListProfitability);
			UoW.Commit();
			base.AfterSave();
		}

		private void SetLogisticianCommentAuthor()
		{
			if(!string.IsNullOrEmpty(Entity.LogisticiansComment))
			{
				Entity.LogisticiansCommentAuthor = CurrentEmployee;
			}
		}
	}
}
