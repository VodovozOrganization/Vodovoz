using Autofac;
using Core.Infrastructure;
using Gamma.Utilities;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using Vodovoz.ViewModels.Logistic;
using static Vodovoz.ViewModels.Journals.JournalViewModels.Employees.FineTemplateJournalViewModel;

namespace Vodovoz.ViewModels.Employees
{
	public class FineViewModel : EntityTabViewModelBase<Fine>, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeService _employeeService;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IEntitySelectorFactory _employeeSelectorFactory;
		private readonly IWagesMovementRepository _wagesMovementRepository;

		private Employee _currentEmployee;
		private List<FineCategory> _fineCategories;

		public FineViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			IEmployeeService employeeService,
			IInteractiveService interactiveService,
			IEmployeeJournalFactory employeeJournalFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ICarEventRepository carEventRepository,
			IWagesMovementRepository wagesMovementRepository
		) : base(uowBuilder, uowFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_employeeSelectorFactory = _employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_wagesMovementRepository = wagesMovementRepository ?? throw new ArgumentNullException(nameof(wagesMovementRepository));
			CreateCommands();
			ConfigureEntityPropertyChanges();

			RouteListViewModel = BuildRouteListEntityViewModel();

			ConfigureCarEvent(carEventRepository);
		}

		private void ConfigureCarEvent(ICarEventRepository carEventRepository)
		{
			var carEvents = carEventRepository.GetCarEventsByFine(UoW, Entity.Id);
			
			if(carEvents.Any())
			{
				CarEvent = $"Взыскано по событию ТС: {string.Join(", ", carEvents.Select(ce => $"{ce.Id} - {ce.CarEventType.ShortName}"))}";
			}
		}

		public List<FineCategory> FineCategories
		{
			get
			{
				if(_fineCategories == null)
				{
					_fineCategories = UoW.GetAll<FineCategory>()
										.Where(x => !x.IsArchive)
										.ToList();
				}
				return _fineCategories;
			}
		}

		public Employee CurrentEmployee
		{
			get
			{
				if(_currentEmployee == null)
				{
					_currentEmployee = _employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				}

				return _currentEmployee;
			}
		}

		public FineTypes FineType
		{
			get => Entity.FineType;
			set
			{
				if(Entity.FineType != value)
				{
					Entity.FineType = value;
					UpdateEmployeeList();
				}
			}
		}

		public string FineReasonString
		{
			get => Entity.FineReasonString;
			set => Entity.FineReasonString = value;
		}

		public virtual RouteList RouteList
		{
			get => Entity.RouteList;
			set
			{
				if(Entity.RouteList != value)
				{
					Entity.RouteList = value;
					UpdateEmployeeList();
					CalculateMoneyFromLiters();
				}
			}
		}

		public virtual UndeliveredOrder UndeliveredOrder
		{
			get => Entity.UndeliveredOrder;
			set => Entity.UndeliveredOrder = value;
		}

		public bool CanEdit => PermissionResult.CanUpdate || (PermissionResult.CanCreate && Entity.Id == 0);

		public bool DateEditable => UoW.IsNew && Entity.RouteList == null;

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => CanEdit && HasChanges;

		#endregion

		[PropertyChangedAlso(nameof(CanShowRequestRouteListMessage))]
		public bool IsFuelOverspendingFine => Entity.FineType == FineTypes.FuelOverspending;

		public bool IsStandartFine => Entity.FineType == FineTypes.Standart;

		public bool CanShowRequestRouteListMessage => IsFuelOverspendingFine && Entity.RouteList == null;

		public bool CanEditFineType => CanEdit;

        public bool IsNew => Entity.Id == 0;

        public decimal Liters
		{
			get => Entity.LitersOverspending;

			set
			{
				Entity.LitersOverspending = value;
				CalculateMoneyFromLiters();
			}
		}

		public string CarEvent { get; private set; }

		public IEntityEntryViewModel RouteListViewModel { get; }

		public void SetRouteListById(int routeListId)
		{
			RouteList = UoW.GetById<RouteList>(routeListId);
		}

		private void ConfigureEntityPropertyChanges()
		{
			SetPropertyChangeRelation(e => e.FineType,
				() => IsFuelOverspendingFine,
				() => IsStandartFine);

			SetPropertyChangeRelation(e => e.FineReasonString,
				() => FineReasonString);

			SetPropertyChangeRelation(e => e.RouteList,
				() => CanShowRequestRouteListMessage,
				() => DateEditable,
				() => IsStandartFine);

			OnEntityPropertyChanged(SetDefaultReason,
				x => x.FineType);

			OnEntityPropertyChanged(Entity.UpdateItems,
				x => x.FineType);

			Entity.ObservableItems.ListChanged += aList => OnPropertyChanged(() => CanEditFineType);
		}

		private void CalculateMoneyFromLiters()
		{
			if(Entity.ObservableItems.Count() > 1 && IsFuelOverspendingFine)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					"При типе штрафа \"Перерасход топлива\" недопустимо наличие более одного сотрудника в списке.");
				return;
			}

			if(RouteList != null)
			{
				decimal fuelCost = RouteList.Car.FuelType.Cost;
				Entity.TotalMoney = Math.Round(Entity.LitersOverspending * fuelCost, 0, MidpointRounding.ToEven);
				var item = Entity.ObservableItems.FirstOrDefault();

				if(item != null)
				{
					item.Money = Entity.TotalMoney;
					item.LitersOverspending = Entity.LitersOverspending;
				}
			}
		}

		private IEntityEntryViewModel BuildRouteListEntityViewModel()
		{
			var routeListEntryViewModelBuilder = new CommonEEVMBuilderFactory<FineViewModel>(
				this, this, UoW, NavigationManager, _lifetimeScope);

			var viewModel = routeListEntryViewModelBuilder
				.ForProperty(x => x.RouteList)
				.UseViewModelJournalAndAutocompleter<RouteListJournalViewModel, RouteListJournalFilterViewModel>(filter =>
				{
					filter.StartDate = DateTime.Today.AddDays(-7);
					filter.EndDate = DateTime.Today.AddDays(1);
				})
				.Finish();

			viewModel.IsEditable = CanEdit;

			return viewModel;
		}

		private void SetDefaultReason()
		{
			Entity.FineReasonString = Entity.FineType.GetEnumTitle();
		}

		private void UpdateEmployeeList()
		{
			if(Entity.RouteList != null)
			{
				ClearItems(Entity.RouteList.Driver);
			}
			else
			{
				ClearItems();
			}
		}

		private void ClearItems(Employee driver = null)
		{
			if(driver != null)
			{
				FineItem item = null;
				item = Entity.ObservableItems.Where(x => x.Employee == driver).FirstOrDefault();

				if(item != null)
				{
					Entity.ObservableItems.Add(item);
				}
				else
				{
					Entity.AddItem(driver);
				}
			}
		}

		protected override bool BeforeSave()
		{
			var allowedCategories = new[] 
			{
				EmployeeCategory.driver,
				EmployeeCategory.forwarder
			};

			var allowedStatuses = new[] 
			{
				EmployeeStatus.IsFired,
				EmployeeStatus.OnCalculation 
			};

			foreach(var item in Entity.ObservableItems)
			{
				var employee = item.Employee;
				if(employee == null)
				{
					continue;
				}

				if(employee.Category.IsIn(allowedCategories) 
					&& employee.Status.IsIn(allowedStatuses))
				{
					decimal employeeBalance = _wagesMovementRepository.GetCurrentEmployeeWageBalance(UoW, employee.Id);

					if(item.Money > employeeBalance)
					{
						CommonServices.InteractiveService.ShowMessage(
							ImportanceLevel.Warning,
							$"Баланс сотрудника {employee.GetPersonNameWithInitials()} меньше, чем сумма штрафа.",
							"Невозможно выставить штраф"
						);
						return false;
					}
				}
			}
			Entity.UpdateWageOperations(UoW);
			Entity.UpdateFuelOperations(UoW);

			return base.BeforeSave();
		}

		public override bool Save(bool close)
		{
			if(Entity.Author == null)
			{
				Entity.Author = CurrentEmployee;
			}

			return base.Save(close);
		}

		#region Commands

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineItemCommand();
			CreateDivideByAllCommand();
			CreateSelectReasonTemplateCommand();
			CreateDeleteFineItemCommand();
		}

		#region AttachFineCommand

		public DelegateCommand OpenUndeliveryCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			OpenUndeliveryCommand = new DelegateCommand(
				() =>
				{
					NavigationManager.OpenViewModel<UndeliveredOrdersJournalViewModel, Action<UndeliveredOrdersFilterViewModel>>(this, filter =>
						{
							filter.RestrictOldOrder = Entity.UndeliveredOrder.OldOrder;
							filter.RestrictOldOrderStartDate = Entity.UndeliveredOrder.OldOrder.DeliveryDate;
							filter.RestrictOldOrderEndDate = Entity.UndeliveredOrder.OldOrder.DeliveryDate;
							filter.RestrictUndeliveryStatus = Entity.UndeliveredOrder.UndeliveryStatus;
						});
				},
				() => true
			);
		}

		#endregion AttachFineCommand

		#region DivideByAllCommand

		public DelegateCommand DivideByAllCommand { get; private set; }

		private void CreateDivideByAllCommand()
		{
			DivideByAllCommand = new DelegateCommand(
				Entity.DivideAtAll,
				() => true
			);
		}

		#endregion DivideByAllCommand

		#region SelectReasonTemplateCommand

		public DelegateCommand SelectReasonTemplateCommand { get; private set; }

		private void CreateSelectReasonTemplateCommand()
		{
			SelectReasonTemplateCommand = new DelegateCommand(
				() =>
				{
					var page = NavigationManager.OpenViewModel<FineTemplateJournalViewModel>(this);

					page.ViewModel.SelectionMode = JournalSelectionMode.Single;
					page.ViewModel.OnSelectResult += (sender, args) =>
					{
						var selectedNode = args.SelectedObjects.FirstOrDefault();
						if(selectedNode == null || Entity.FineType == FineTypes.FuelOverspending)
						{
							return;
						}
						var selectedFineTemplate = UoW.GetById<FineTemplate>((selectedNode as FineTemplateJournalNode).Id);
						Entity.FineReasonString = selectedFineTemplate.Reason;
						Entity.TotalMoney = selectedFineTemplate.FineMoney;
					};
				},
				() => true
			);
		}

		#endregion SelectReasonTemplateCommand

		#region AddFineItemCommand

		public DelegateCommand AddFineItemCommand { get; private set; }

		private void CreateAddFineItemCommand()
		{
			AddFineItemCommand = new DelegateCommand(
				() =>
				{
					var employeeSelector = _employeeSelectorFactory.CreateSelector();
					employeeSelector.OnEntitySelectedResult += (sender, e) =>
					{
						var node = e.SelectedNodes.FirstOrDefault();
						if(node == null)
						{
							return;
						}

						Entity.AddItem(UoW.GetById<Employee>(node.Id));
					};
					TabParent.AddSlaveTab(this, employeeSelector);
				},
				() => IsStandartFine && IsNew
            );
			AddFineItemCommand.CanExecuteChangedWith(Entity, x => x.RouteList);
			AddFineItemCommand.CanExecuteChangedWith(this, x => x.IsStandartFine, x => x.CanEdit, x => x.IsNew);
		}

		#endregion AddFineItemCommand

		#region DeleteFineItemCommand

		public DelegateCommand<FineItem> DeleteFineItemCommand { get; private set; }		

		private void CreateDeleteFineItemCommand()
		{
			DeleteFineItemCommand = new DelegateCommand<FineItem>(
				Entity.RemoveItem,
				(item) => item != null && CanEdit && IsStandartFine && IsNew
            );
			DeleteFineItemCommand.CanExecuteChangedWith(Entity, x => x.RouteList);
			DeleteFineItemCommand.CanExecuteChangedWith(this, x => x.IsStandartFine, x => x.CanEdit, x => x.IsNew);

        }

        #endregion DeleteFineItemCommand

        #endregion Commands
    }
}
