﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using QS.Commands;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Extension;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.JournalViewers;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.Services;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListAnalysisViewModel : EntityTabViewModelBase<RouteList>, IAskSaveOnCloseViewModel
	{
		private readonly IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private readonly IEmployeeService _employeeService;
		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(new ParametersProvider()));
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IGtkTabsOpener _gtkDialogsOpener;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
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
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IGtkTabsOpener gtkDialogsOpener,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			IDeliveryShiftRepository deliveryShiftRepository,
			IEmployeeSettings employeeSettings,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IRouteListProfitabilityController routeListProfitabilityController,
			IRouteListItemRepository routeListItemRepository,
			ISubdivisionParametersProvider subdivisionParametersProvider) : base (uowBuilder, unitOfWorkFactory, commonServices)
		{
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = 
				deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_gtkDialogsOpener = gtkDialogsOpener ?? throw new ArgumentNullException(nameof(gtkDialogsOpener));
			_undeliveredOrdersJournalOpener =
				undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_routeListProfitabilityController =
				routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_subdivisionParametersProvider =
				subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			UndeliveredOrdersRepository =
				undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

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
			
			_undeliveryViewOpener = new UndeliveredOrdersJournalOpener();
			_employeeService = VodovozGtkServicesConfig.EmployeeService;
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
				var dlg = new OrderDlg(SelectedItem.Order) {
					HasChanges = false
				};

				dlg.SetDlgToReadOnly();
				TabParent.AddSlaveTab(this, dlg);

			}, () => SelectedItem != null
		));
		
		private DelegateCommand openUndeliveredOrderCommand;
		public DelegateCommand OpenUndeliveredOrderCommand => 
			openUndeliveredOrderCommand ?? (openUndeliveredOrderCommand = new DelegateCommand(
				() => {

					var undeliveredOrdersFilter = new UndeliveredOrdersFilterViewModel(
						_commonServices,
						_orderSelectorFactory,
						_employeeJournalFactory,
						_counterpartyJournalFactory,
						_deliveryPointJournalFactory,
						_subdivisionJournalFactory)
					{
						HidenByDefault = true,
						RestrictOldOrder = SelectedItem.Order,
						RestrictOldOrderStartDate = SelectedItem.Order.DeliveryDate,
						RestrictOldOrderEndDate = SelectedItem.Order.DeliveryDate
					};

					var dlg = new UndeliveredOrdersJournalViewModel(
						undeliveredOrdersFilter,
						UnitOfWorkFactory,
						_commonServices,
						_gtkDialogsOpener,
						_employeeJournalFactory,
						_employeeService,
						_undeliveredOrdersJournalOpener,
						_orderSelectorFactory,
						UndeliveredOrdersRepository,
						_employeeSettings,
						_subdivisionParametersProvider
					);

					dlg.TabClosed += (s,e) => UpdateTreeAddresses?.Invoke();
					
					TabParent.AddTab(dlg, this);
				}, 
				() => SelectedItem != null
			)
		);
		
		private DelegateCommand createFineCommand;
		public DelegateCommand CreateFineCommand => createFineCommand ?? (createFineCommand = new DelegateCommand(
			() => {
				
				var fineViewModel = new FineViewModel(
					EntityUoWBuilder.ForCreate(),
					QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
					_undeliveryViewOpener,
					_employeeService,
					_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
					_employeeSettings,
					CommonServices
				);

				fineViewModel.RouteList = SelectedItem.RouteList;

				var undeliveredOrder = GetUndeliveredOrder();

				if (undeliveredOrder != null)
				{
					fineViewModel.UndeliveredOrder = undeliveredOrder;
				}

				if(SelectedItem.CalculateTimeLateArrival() != null)
					fineViewModel.FineReasonString = $"Опоздание по заказу №{SelectedItem.Order.Id} от {SelectedItem.Order.DeliveryDate:d}";

				fineViewModel.EntitySaved += (sender, args) =>
				{
					SelectedItem.AddFine(args.Entity as Fine);
					UpdateTreeAddresses?.Invoke();
				}; 
				
				TabParent.AddSlaveTab(this, fineViewModel);
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
					_undeliveryViewOpener,
					_employeeService,
					_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
					UnitOfWorkFactory, 
					_employeeSettings,
					CommonServices
				);
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
					_undeliveryViewOpener,
					_employeeService,
					_employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory(),
					UnitOfWorkFactory, 
					_employeeSettings,
					CommonServices
				);
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
