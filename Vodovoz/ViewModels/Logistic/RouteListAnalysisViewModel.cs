using System;
using System.ComponentModel;
using System.Linq;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Domain.Orders;
using QS.Commands;
using QS.Project.Journal;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.JournalViewers;
using Vodovoz.Repositories;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListAnalysisViewModel : EntityTabViewModelBase<RouteList>
	{
		private readonly IUndeliveriesViewOpener undeliveryViewOpener;
		private readonly IEntitySelectorFactory employeeSelectorFactory;
		private readonly IEmployeeService employeeService;

		#region Properties

		public IEntityAutocompleteSelectorFactory LogisticanSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory DriverSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory ForwarderSelectorFactory { get; }
		
		public Employee CurrentEmployee { get; }

		public RouteListItem SelectedItem { get; set; }

		#endregion

		public Action UpdateTreeAddresses;

		#region Constructor

		public RouteListAnalysisViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices) : base (uowBuilder, unitOfWorkFactory, commonServices)
		{
			Entity.ObservableAddresses.PropertyOfElementChanged += ObservableAddressesOnPropertyOfElementChanged;
			
			undeliveryViewOpener = new UndeliveriesViewOpener();
			employeeService = VodovozGtkServicesConfig.EmployeeService;
			CurrentEmployee = employeeService.GetEmployeeForUser(UoW, CurrentUser.Id);
			employeeSelectorFactory = new DefaultEntityAutocompleteSelectorFactory<Employee, EmployeesJournalViewModel, EmployeeFilterViewModel>(commonServices);
			
			LogisticanSelectorFactory =
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() => {
						var filter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking, RestrictCategory = EmployeeCategory.office };
						return new EmployeesJournalViewModel(filter, UnitOfWorkFactory, CommonServices);
					});

			DriverSelectorFactory =
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() => {
						var filter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking, RestrictCategory = EmployeeCategory.driver };
						return new EmployeesJournalViewModel(filter, UnitOfWorkFactory, CommonServices);
					});

			ForwarderSelectorFactory =
				new EntityAutocompleteSelectorFactory<EmployeesJournalViewModel>(typeof(Employee),
					() => {
						var filter = new EmployeeFilterViewModel { Status = EmployeeStatus.IsWorking, RestrictCategory = EmployeeCategory.forwarder };
						return new EmployeesJournalViewModel(filter, UnitOfWorkFactory, CommonServices);
					});

			TabName = $"Диалог разбора {Entity.Title}";
		}

		private void ObservableAddressesOnPropertyOfElementChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(LateArrivalReason))
				SelectedItem.LateArrivalReasonAuthor = CurrentEmployee;
		}

		#endregion

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
					
					var dlg = new UndeliveriesView();
					dlg.HideFilterAndControls();
					dlg.UndeliveredOrdersFilter.SetAndRefilterAtOnce(
						x => x.ResetFilter(),
						x => x.RestrictOldOrder = SelectedItem.Order,
						x => x.RestrictOldOrderStartDate = SelectedItem.Order.DeliveryDate,
						x => x.RestrictOldOrderEndDate = SelectedItem.Order.DeliveryDate
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
					undeliveryViewOpener,
					employeeService,
					employeeSelectorFactory,
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
					undeliveryViewOpener,
					employeeService,
					employeeSelectorFactory,
					UnitOfWorkFactory,
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
					undeliveryViewOpener,
					employeeService,
					employeeSelectorFactory,
					UnitOfWorkFactory,
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
												   .Sum(x => x.Order.TotalWaterBottles);
			int canceledBottles = Entity.Addresses.Where(
				x => x != null && (x.Status == RouteListItemStatus.Canceled
				                   || x.Status == RouteListItemStatus.Overdue
				                   || x.Status == RouteListItemStatus.Transfered)
			).Sum(x => x.Order.TotalWaterBottles);
			int enrouteBottles = Entity.Addresses.Where(x => x != null && x.Status == RouteListItemStatus.EnRoute)
												 .Sum(x => x.Order.TotalWaterBottles);
			bottles = "<b>Всего 19л. бутылей в МЛ:</b>\n";
			bottles += $"Выполнено: <b>{completedBottles}</b>\n";
			bottles += $" Отменено: <b>{canceledBottles}</b>\n";
			bottles += $" Осталось: <b>{enrouteBottles}</b>\n";
			
			return bottles;
		}

		private UndeliveredOrder GetUndeliveredOrder() => 
			UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, SelectedItem.Order.Id).SingleOrDefault();
	}
}
