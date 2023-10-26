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
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListAnalysisViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEmployeeService _employeeService;
		private readonly IWageParameterService _wageParameterService;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
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
			ISubdivisionParametersProvider subdivisionParametersProvider,
			IFileDialogService fileDialogService,
			ILifetimeScope lifetimeScope,
			INavigationManager navigationManager)
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
			_subdivisionParametersProvider =
				subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService)); _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			;
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
			
			if(CurrentEmployee == null) {
				AbortOpening("Ваш пользователь не привязан к действующему сотруднику, вы не можете открыть " +
				             "диалог разбора МЛ, так как некого указывать в качестве логиста.", "Невозможно открыть разбор МЛ");
			}
			
			LogisticanSelectorFactory = _employeeJournalFactory.CreateWorkingOfficeEmployeeAutocompleteSelectorFactory();
			DriverSelectorFactory = _employeeJournalFactory.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();
			ForwarderSelectorFactory = _employeeJournalFactory.CreateWorkingForwarderEmployeeAutocompleteSelectorFactory();

			TabName = $"Диалог разбора {Entity.Title}";
			
			ValidationContext.Items.Add(nameof(IRouteListItemRepository), routeListItemRepository);
		}
		
		#endregion
		
		#region Properties

		public IEntityAutocompleteSelectorFactory LogisticanSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }
		public IUndeliveredOrdersRepository UndeliveredOrdersRepository { get; }

		public readonly IList<DeliveryShift> DeliveryShifts;
		
		public Employee CurrentEmployee { get; }

		public RouteListItem SelectedItem { get; set; }

		public bool CanEditRouteList => PermissionResult.CanUpdate;

		#endregion

		public Action UpdateTreeAddresses;
		
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
				var fineFilter = new FineFilterViewModel();
				fineFilter.ExcludedIds = SelectedItem.Fines.Select(x => x.Id).ToArray();
				var fineJournalViewModel = new FinesJournalViewModel(
					fineFilter,
					_employeeService,
					_employeeJournalFactory,
					UnitOfWorkFactory, 
					CommonServices,
					_lifetimeScope,
					NavigationManager);

				fineJournalViewModel.SelectionMode = JournalSelectionMode.Single;
				fineJournalViewModel.OnEntitySelectedResult +=
					(sender, e) =>
					{
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						
						if (selectedNode == null)
							return;
						
						var fine = UoW.GetById<Fine>(selectedNode.Id);

						var undeliveredOrder = GetUndeliveredOrder();
						if (undeliveredOrder != null)
						{
							fine.UndeliveredOrder = undeliveredOrder;
							UoW.Save(fine);
						}

						SelectedItem.AddFine(fine);
						UpdateTreeAddresses?.Invoke();
					};
				TabParent.AddSlaveTab(this, fineJournalViewModel);
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
				var fineFilter = new FineFilterViewModel();
				fineFilter.FindFinesWithIds = SelectedItem.Fines.Select(x => x.Id).ToArray();
				var fineJournalViewModel = new FinesJournalViewModel(
					fineFilter,
					_employeeService,
					_employeeJournalFactory,
					UnitOfWorkFactory, 
					CommonServices,
					_lifetimeScope,
					NavigationManager);

				fineJournalViewModel.SelectionMode = JournalSelectionMode.Single;
				fineJournalViewModel.OnEntitySelectedResult +=
					(sender, e) =>
					{
						var selectedNode = e.SelectedNodes.FirstOrDefault();
						
						if (selectedNode == null)
							return;
						
						var fine = UoW.GetById<Fine>(selectedNode.Id);

						SelectedItem.RemoveFine(fine);
						UpdateTreeAddresses?.Invoke();
					};
				TabParent.AddSlaveTab(this, fineJournalViewModel);
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
