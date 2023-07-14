using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventViewModel : EntityTabViewModelBase<CarEvent>
	{
		private readonly IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private readonly IEntitySelectorFactory _employeeSelectorFactory;
		private readonly IEmployeeSettings _employeeSettings;
		private readonly ICarEventSettings _carEventSettings;
		public string CarEventTypeCompensation = "Компенсация от страховой, по суду";
		public decimal RepairCost
		{
			get => Math.Abs(Entity.RepairCost);
			set => SetRepairCost(value);
		}

		public CarEventType CarEventType { 
			get => Entity.CarEventType; 
			set => SetCarEventType(value); 
		}

		public bool DoNotShowInOperation
		{
			get => Entity.DoNotShowInOperation;
			set => Entity.DoNotShowInOperation = value;
		}

		public bool CompensationFromInsuranceByCourt
		{
			get => Entity.CompensationFromInsuranceByCourt;
			set => Entity.CompensationFromInsuranceByCourt = value;
		}

		public Car Car
		{
			get => Entity.Car;
			set => SetCar(value);
		}

		public bool CanEdit => PermissionResult.CanUpdate && CheckDatePeriod();
		public bool CanChangeWithClosedPeriod { get; }
		public bool CanAddFine => CanEdit;
		public bool CanAttachFine => CanEdit;
		public IEmployeeService EmployeeService { get; }
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IList<FineItem> FineItems { get; private set; }
		public bool ShowlabelOriginalCarEvent => UoW.IsNew || Entity.CompensationFromInsuranceByCourt;
		private int _startNewPeriodDay;

		public CarEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			ICarEventJournalFactory carEventSelectorFactory,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeSettings employeeSettings,
			ICarEventSettings carEventSettings
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			CanChangeWithClosedPeriod =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_edit_car_events_in_closed_period");
			_startNewPeriodDay = _carEventSettings.CarEventStartNewPeriodDay;
			UpdateFileItems();

			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;

			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			CreateCommands();

			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory();
			CarEventTypeSelectorFactory = carEventTypeJournalFactory.CreateCarEventTypeAutocompleteSelectorFactory();
			CarEventSelectorFactory = carEventSelectorFactory.CreateCarEventAutocompleteSelectorFactory();
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			TabName = "Событие ТС";

			if(Entity.Id == 0)
			{
				Entity.Author = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				Entity.CreateDate = DateTime.Now;
			}
			Entity.PropertyChanged += EntityPropertyChanged;
		}

		private void EntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.CarEventType):
					OnPropertyChanged(nameof(CarEventType));
					break;
				case nameof(Entity.RepairCost):
					OnPropertyChanged(nameof(RepairCost));
					break;
				case nameof(Entity.DoNotShowInOperation):
					OnPropertyChanged(nameof(DoNotShowInOperation));
					break;
				case nameof(Entity.CompensationFromInsuranceByCourt):
					OnPropertyChanged(nameof(CompensationFromInsuranceByCourt));
					break;
				case nameof(Entity.Car):
					OnPropertyChanged(nameof(Car));
					break;
				default:
					break;
			}
		}

		private bool CheckDatePeriod()
		{
			if(UoW.IsNew)
			{
				return true;
			}

			if(CanChangeWithClosedPeriod)
			{
				return true;
			}

			return InCorrectPeriod(Entity.EndDate);
		}

		public new void SaveAndClose()
		{
			if(Entity.StartDate == default)
			{
				ShowWarningMessage("Дата начала события должна быть указана.");
				return;
			}

			if(CanChangeWithClosedPeriod)
			{
				if(InCorrectPeriod(Entity.EndDate) || AskQuestion("Вы уверенны что хотите сохранить изменения в закрытом периоде?"))
				{
					base.SaveAndClose();
				}
				return;
			}

			var today = DateTime.Now;
			DateTime startCurrentMonth = new DateTime(today.Year, today.Month, 1);
			DateTime startPreviousMonth = startCurrentMonth.AddMonths(-1);
			if(today.Day <= _startNewPeriodDay && Entity.EndDate < startPreviousMonth)
			{
				ShowWarningMessage($"С 1 по {_startNewPeriodDay} текущего месяца можно создать/изменить событие ТС с датой завершения равной или более 1 числа прошлого месяца.");
				return;
			}

			if(today.Day > _startNewPeriodDay && Entity.EndDate < startCurrentMonth)
			{
				ShowWarningMessage($"С {_startNewPeriodDay + 1} числа текущего месяца можно создать/изменить событие ТС с датой завершения равной или более 1 числа текущего месяца");
				return;
			}

			base.SaveAndClose();
		}

		private bool InCorrectPeriod(DateTime endDate)
		{
			var today = DateTime.Now;
			DateTime startCurrentMonth = new DateTime(today.Year, today.Month, 1);
			DateTime startPreviousMonth = startCurrentMonth.AddMonths(-1);
			if(today.Day <= _startNewPeriodDay && endDate > startPreviousMonth)
			{
				return true;
			}

			if(today.Day > _startNewPeriodDay && endDate >= startCurrentMonth)
			{
				return true;
			}
			return false;
		}

		public override void Dispose()
		{
			Entity.ObservableFines.ListContentChanged -= ObservableFines_ListContentChanged;
			base.Dispose();
		}

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CarEventSelectorFactory { get; }

		private void SetCar(Car car)
		{
			Entity.Car = car;

			if(Car != null)
			{
				Entity.Driver = (Car.Driver != null && Car.Driver.Status != EmployeeStatus.IsFired)
					? Car.Driver
					: null;
			}
		}

		private void SetCarEventType(CarEventType carEventType)
		{
			Entity.CarEventType = carEventType;
			ChangeEventType();
		}

		public void ChangeEventType()
		{
			ChangeDoNotShowInOperation();
			SetCompensationFromInsuranceByCourt();
		}

		public void ChangeDoNotShowInOperation()
		{
			if(CarEventType?.IsDoNotShowInOperation == true)
			{
				DoNotShowInOperation = true;
			}
		}

		private void SetRepairCost(decimal value)
		{
			if(IsCompensationFromInsuranceByCourt())
			{
				Entity.RepairCost = -value;
			}
			else
			{
				Entity.RepairCost = value;
			}
		}

		private void SetCompensationFromInsuranceByCourt()
		{
			if(IsCompensationFromInsuranceByCourt())
			{
				CompensationFromInsuranceByCourt = true;
			}
			else
			{
				if(CompensationFromInsuranceByCourt)
				{
					CompensationFromInsuranceByCourt = false;
				}
			}
		}

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
		}

		private bool IsCompensationFromInsuranceByCourt()
		{
			return CarEventType?.Id == _carEventSettings.CompensationFromInsuranceByCourtId;
		}

		public DelegateCommand AddFineCommand { get; private set; }

		private void CreateAddFineCommand()
		{
			AddFineCommand = new DelegateCommand(
				() => CreateAddFine(),
				() => CanAddFine
			);
			AddFineCommand.CanExecuteChangedWith(this, x => CanAddFine);
		}

		public DelegateCommand AttachFineCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			AttachFineCommand = new DelegateCommand(
				() => CreateAttachFine(),
				() => CanAttachFine
			);
			AttachFineCommand.CanExecuteChangedWith(this, x => CanAttachFine);
		}

		private void CreateAttachFine()
		{
			var fineJournalViewModel = CreateFinesJournalViewModel();
			fineJournalViewModel.OnEntitySelectedResult += (sender, e) =>
			{
				var selectedNode = e.SelectedNodes.FirstOrDefault();
				if(selectedNode == null)
				{
					return;
				}
				Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
			};
			TabParent.AddSlaveTab(this, fineJournalViewModel);
		}

		private void CreateAddFine()
		{
			var fineViewModel = new FineViewModel(
						   EntityUoWBuilder.ForCreate(),
						   QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
						   _undeliveryViewOpener,
						   EmployeeService,
						   EmployeeJournalFactory,
						   _employeeSettings,
						   CommonServices
					   )
			{
				FineReasonString = Entity.GetFineReason()
			};
			fineViewModel.EntitySaved += (sender, e) =>
			{
				Entity.AddFine(e.Entity as Fine);
			};
			TabParent.AddSlaveTab(this, fineViewModel);
		}

		private FinesJournalViewModel CreateFinesJournalViewModel()
		{
			var fineFilter = new FineFilterViewModel(true)
			{
				ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray()
			};

			return new FinesJournalViewModel(
				fineFilter,
				_undeliveryViewOpener,
				EmployeeService,
				EmployeeJournalFactory,
				QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
				_employeeSettings,
				CommonServices
			)
			{
				SelectionMode = JournalSelectionMode.Single
			};
		}

		private void ObservableFines_ListContentChanged(object sender, EventArgs e)
		{
			UpdateFileItems();
			OnPropertyChanged(() => FineItems);
		}

		private void UpdateFileItems()
		{
			FineItems = Entity.Fines.SelectMany(x => x.Items).OrderByDescending(x => x.Id).ToList();
		}
	}
}
