using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Warehouses.Documents;
using Vodovoz.Domain.Documents.WriteOffDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;
using Vodovoz.ViewModels.Warehouses;
using Car = Vodovoz.Domain.Logistic.Cars.Car;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventViewModel : EntityTabViewModelBase<CarEvent>
	{
		private readonly ICarEventSettings _carEventSettings;
		private readonly ICarEventRepository _carEventRepository;
		private readonly ViewModelEEVMBuilder<WriteOffDocument> _writeOffDocumentViewModelEEVMBuilder;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IFuelRepository _fuelRepository;
		private readonly ViewModelEEVMBuilder<CarEventType> _carEventTypeViewModelEEVMBuilder;
		public string CarEventTypeCompensation = "Компенсация от страховой, по суду";
		private EntityEntryViewModel<Car> _carEntryViewModel;
		private readonly int _startNewPeriodDay;
		private bool _canCreateFuelBalanceCalibrationCarEvent;
		private IInteractiveService _interactiveService;

		public CarEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			ICarEventSettings carEventSettings,
			INavigationManager navigationManager,
			ICarEventRepository carEventRepository,
			ViewModelEEVMBuilder<WriteOffDocument> writeOffDocumentViewModelEEVMBuilder,
			ILifetimeScope lifetimeScope,
			IFuelRepository fuelRepository,
			ViewModelEEVMBuilder<CarEventType> carEventViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			if(commonServices is null)
			{
				throw new ArgumentNullException(nameof(commonServices));
			}

			_interactiveService = commonServices.InteractiveService;

			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));
			_writeOffDocumentViewModelEEVMBuilder = writeOffDocumentViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(writeOffDocumentViewModelEEVMBuilder));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_carEventTypeViewModelEEVMBuilder = carEventViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carEventViewModelEEVMBuilder));
			CanChangeWithClosedPeriod =
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_edit_car_events_in_closed_period");
			_canCreateFuelBalanceCalibrationCarEvent = commonServices.CurrentPermissionService.ValidatePresetPermission(
				Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanCreateFuelBalanceCalibrationCarEvent);
			_startNewPeriodDay = _carEventSettings.CarEventStartNewPeriodDay;
			UpdateFileItems();

			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;

			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			CreateCommands();

			CarEventTypeSelectorFactory = carEventTypeJournalFactory.CreateCarEventTypeAutocompleteSelectorFactory();

			OriginalCarEventViewModel = new CommonEEVMBuilderFactory<CarEvent>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.OriginalCarEvent)
				.UseViewModelDialog<CarEventViewModel>()
				.UseViewModelJournalAndAutocompleter<CarEventJournalViewModel, CarEventFilterViewModel>(filter =>
				{
					filter.ExcludeEventIds.Add(Entity.Id);
				})
				.Finish();

			OriginalCarEventViewModel.IsEditable = CanEdit;

			CarEntryViewModel = BuildCarEntryViewModel();
			CarEventTypeEntryViewModel = BuildCarEventTypeEntryViewModel();
			WriteOffDocumentEntryViewModel = BuildWriteOffDocumentEntryViewModel();
			WriteOffDocumentEntryViewModel.ChangedByUser += OnWriteOffDocumentChangedByUser;

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

			WriteOffDocumentNotRequiredChangedCommand = new DelegateCommand(WriteOffDocumentNotRequiredChanged);

			CanChangeCarEventType = !(IsTechInspectCarEventType && Entity.Id > 0) && CanEditFuelBalanceCalibration;
		}

		public DelegateCommand WriteOffDocumentNotRequiredChangedCommand { get; }

		public decimal RepairCost
		{
			get => Math.Abs(Entity.RepairCost);
			set => SetRepairCost(value);
		}

		public CarEventType CarEventType
		{
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

		public bool CanEdit => PermissionResult.CanUpdate && CheckDatePeriod();
		public bool CanChangeWithClosedPeriod { get; }
		public bool CanChangeCarEventType { get; }
		public bool CanAddFine => CanEdit;
		public bool CanAttachFine => CanEdit;
		public bool CanChangeCarTechnicalCheckupEndDate => CanEdit && IsCarTechnicalCheckupEventType;
		public bool CanAttachWriteOffDocument =>
			CanEdit
			&& Entity.CarEventType?.IsAttachWriteOffDocument == true
			&& !Entity.IsWriteOffDocumentNotRequired;

		public bool CanChangeWriteOffDocumentNotRequired =>
			CanEdit
			&& Entity.CarEventType?.IsAttachWriteOffDocument == true;

		public bool IsTechInspectCarEventType =>
			Entity.CarEventType?.Id == _carEventSettings.TechInspectCarEventTypeId;

		public bool IsCarTechnicalCheckupEventType =>
			Entity.CarEventType?.Id == _carEventSettings.CarTechnicalCheckupEventTypeId;

		public bool CanEditFuelBalanceCalibration =>
			(IsFuelBalanceCalibration && Entity.Id == 0)
			|| !IsFuelBalanceCalibration;

		public bool IsFuelBalanceCalibration =>
			Entity.CarEventType?.Id == _carEventSettings.FuelBalanceCalibrationCarEventTypeId;

		public IEmployeeService EmployeeService { get; }
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IList<FineItem> FineItems { get; private set; }
		public bool ShowlabelOriginalCarEvent => UoW.IsNew || Entity.CompensationFromInsuranceByCourt;

		private void EntityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(Entity.CarEventType):
					OnPropertyChanged(nameof(CarEventType));
					SetFuelBalanceCorrectionIfNeeded();
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
					UpdateCurrentFuelBalance();
					UpdateSubstractionFuelBalance();
					UpdateFuelCost();
					break;
				case nameof(Entity.ActualFuelBalance):
					UpdateSubstractionFuelBalance();
					UpdateFuelCost();
					break;
				default:
					break;
			}
		}

		private void SetFuelBalanceCorrectionIfNeeded()
		{
			if(Entity.CarEventType?.Id == _carEventSettings.FuelBalanceCalibrationCarEventTypeId)
			{
				UpdateCurrentFuelBalance();
				UpdateSubstractionFuelBalance();
				UpdateFuelCost();
			}
			else
			{
				Entity.ActualFuelBalance = null;
				Entity.CurrentFuelBalance = null;
				Entity.SubstractionFuelBalance = null;
				Entity.FuelCost = null;
			}
		}

		private void UpdateCurrentFuelBalance()
		{
			if(Entity.CarEventType?.Id != _carEventSettings.FuelBalanceCalibrationCarEventTypeId)
			{
				return;
			}

			Entity.CurrentFuelBalance = _fuelRepository.GetFuelBalance(UoW, null, Entity.Car, _carEventSettings.FuelBalanceCalibrationCarEventTypeId, Entity.CreateDate);
		}

		private void UpdateSubstractionFuelBalance()
		{
			if(Entity.CarEventType?.Id != _carEventSettings.FuelBalanceCalibrationCarEventTypeId)
			{
				return;
			}

			Entity.SubstractionFuelBalance = (Entity.ActualFuelBalance ?? 0) - (Entity.CurrentFuelBalance ?? 0);
		}

		private void UpdateFuelCost()
		{
			if(Entity.CarEventType?.Id != _carEventSettings.FuelBalanceCalibrationCarEventTypeId)
			{
				return;
			}

			Entity.FuelCost = Entity.SubstractionFuelBalance * Entity.Car?.FuelType?.Cost;
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

			if(UoW.IsNew && Entity.CarEventType?.Id == _carEventSettings.TechInspectCarEventTypeId)
			{
				Entity.Car.TechInspectForKm = null;
				UoW.Save(Entity.Car);
			}

			if(CanChangeWithClosedPeriod)
			{
				if(InCorrectPeriod(Entity.EndDate) || AskQuestion("Вы уверенны что хотите сохранить изменения в закрытом периоде?"))
				{
					if(Entity.CarEventType?.Id == _carEventSettings?.FuelBalanceCalibrationCarEventTypeId)
					{
						Entity.UpdateCalibrationFuelOperation();
					}

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

			if(Entity.CarEventType?.Id == _carEventSettings?.FuelBalanceCalibrationCarEventTypeId)
			{
				Entity.UpdateCalibrationFuelOperation();
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
			_carEntryViewModel.ChangedByUser -= OnCarChangedByUser;
			_carEntryViewModel.Dispose();
			base.Dispose();
		}

		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityEntryViewModel OriginalCarEventViewModel { get; private set; }
		public IEntityEntryViewModel CarEntryViewModel { get; }
		public IEntityEntryViewModel WriteOffDocumentEntryViewModel { get; }
		public IEntityEntryViewModel CarEventTypeEntryViewModel { get; }

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<CarEvent>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			_carEntryViewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			_carEntryViewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			_carEntryViewModel.ChangedByUser += OnCarChangedByUser;

			return _carEntryViewModel;
		}

		private IEntityEntryViewModel BuildWriteOffDocumentEntryViewModel()
		{
			var viewModel = _writeOffDocumentViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.WriteOffDocument)
				.UseViewModelJournalAndAutocompleter<WarehouseDocumentsJournalViewModel, WarehouseDocumentsJournalFilterViewModel>(
				filter =>
				{
					filter.DocumentType = DocumentType.WriteoffDocument;
					filter.CanChangeRestrictedDocumentType = false;
				})
				.UseViewModelDialog<WriteOffDocumentViewModel>()
				.Finish();

			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(CarEvent)).CanUpdate;

			return viewModel;
		}

		private IEntityEntryViewModel BuildCarEventTypeEntryViewModel()
		{
			var viewModel = _carEventTypeViewModelEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(this, x => x.CarEventType)
				.UseViewModelJournalAndAutocompleter<CarEventTypeJournalViewModel, CarEventTypeFilterViewModel>(
				filter =>
				{
					if(!_canCreateFuelBalanceCalibrationCarEvent)
					{
						filter.ExcludeCarEventTypeIds.Add(_carEventSettings.FuelBalanceCalibrationCarEventTypeId);
					}
				})
				.UseViewModelDialog<WriteOffDocumentViewModel>()
				.Finish();

			viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(CarEvent)).CanUpdate;

			return viewModel;
		}


		private void OnWriteOffDocumentChangedByUser(object sender, EventArgs e)
		{
			RemoveWriteOffDocumentIfCarsNotEqual();
			RemoveWriteOffDocumentIfItAlreadyAttachedToOtherCarEvents();

			OnPropertyChanged(nameof(Entity.RepairPartsCost));
			OnPropertyChanged(nameof(Entity.RepairAndPartsSummaryCost));
		}

		private void OnCarChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Car != null)
			{
				Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired)
				? Entity.Car.Driver
				: null;
			}

			RemoveWriteOffDocumentIfCarsNotEqual();
		}

		private void RemoveWriteOffDocumentIfCarsNotEqual()
		{
			if(IsWriteOffDocumentCanBeAttachedToSelectedCar)
			{
				return;
			}

			Entity.WriteOffDocument = null;
			ShowWarningMessage("Выбранный акт списания ТМЦ не может быть прикреплен, т.к. авто не совпадают!");
		}

		private void RemoveWriteOffDocumentIfItAlreadyAttachedToOtherCarEvents()
		{
			if(Entity.WriteOffDocument is null)
			{
				return;
			}

			var eventsIdsHavingAttachedWriteOffDocument =
				_carEventRepository.GetCarEventIdsByWriteOffDocument(UoW, Entity.WriteOffDocument.Id)
				.Where(x => x != Entity.Id)
				.ToList()
				.Distinct();

			if(!eventsIdsHavingAttachedWriteOffDocument.Any())
			{
				return;
			}

			Entity.WriteOffDocument = null;
			ShowWarningMessage(
				$"Выбранный акт списания ТМЦ уже прикреплен к событиям {string.Join(", ", eventsIdsHavingAttachedWriteOffDocument)}");
		}

		private bool IsWriteOffDocumentCanBeAttachedToSelectedCar =>
			Entity.Car is null
			|| Entity.WriteOffDocument is null
			|| Entity.Car?.Id == Entity.WriteOffDocument?.WriteOffFromCar?.Id;

		private void SetCarEventType(CarEventType carEventType)
		{
			Entity.CarEventType = carEventType;
			ChangeEventType();
		}

		public void ChangeEventType()
		{
			ChangeDoNotShowInOperation();
			SetCompensationFromInsuranceByCourt();
			ResetCarTechnicalCheckupEndDateIfNeed();
			OnPropertyChanged(nameof(IsTechInspectCarEventType));
			OnPropertyChanged(nameof(IsCarTechnicalCheckupEventType));
			OnPropertyChanged(nameof(CanChangeCarTechnicalCheckupEndDate));
			OnPropertyChanged(nameof(CanAttachWriteOffDocument));
			OnPropertyChanged(nameof(CanChangeWriteOffDocumentNotRequired));
			OnPropertyChanged(nameof(CanEditFuelBalanceCalibration));
			OnPropertyChanged(nameof(IsFuelBalanceCalibration));
			OnPropertyChanged(nameof(CanChangeCarEventType));

			if(CarEventType?.IsAttachWriteOffDocument == false)
			{
				Entity.WriteOffDocument = null;
				Entity.IsWriteOffDocumentNotRequired = false;
			}
		}

		public void ChangeDoNotShowInOperation()
		{
			if(CarEventType?.IsDoNotShowInOperation == true)
			{
				DoNotShowInOperation = true;
				return;
			}
			DoNotShowInOperation = false;
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

		private void ResetCarTechnicalCheckupEndDateIfNeed()
		{
			if(IsCarTechnicalCheckupEventType)
			{
				return;
			}

			Entity.CarTechnicalCheckupEndingDate = null;
		}

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
			InfoCommand = new DelegateCommand(ShowHelpInfo);
		}

		private void ShowHelpInfo()
		{
			_interactiveService.ShowMessage(
				ImportanceLevel.Info,
				"Ограничения при проведении калибровки:\n" +
				"1. Калибровку можно делать только утром до первого рейса на дату\n" +
				"2. На автомобиль не должно быть выдано топливо на текущую дату\n" +
				"3. У автомобиля не должно быть завершенных заказов по МЛ на текущую дату"
			);
		}

		private bool IsCompensationFromInsuranceByCourt()
		{
			return CarEventType?.Id == _carEventSettings.CompensationFromInsuranceByCourtId;
		}

		public DelegateCommand AddFineCommand { get; private set; }

		private void CreateAddFineCommand()
		{
			AddFineCommand = new DelegateCommand(
				CreateAddFine,
				() => CanAddFine
			);
			AddFineCommand.CanExecuteChangedWith(this, x => CanAddFine);
		}

		public DelegateCommand AttachFineCommand { get; private set; }
		public DelegateCommand InfoCommand { get; private set; }

		private void CreateAttachFineCommand()
		{
			AttachFineCommand = new DelegateCommand(
				CreateAttachFine,
				() => CanAttachFine
			);
			AttachFineCommand.CanExecuteChangedWith(this, x => CanAttachFine);
		}

		private void CreateAttachFine()
		{
			NavigationManager.OpenViewModel<FinesJournalViewModel, Action<FineFilterViewModel>>(
				this,
				filter =>
				{
					filter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
				},
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.SelectionMode = JournalSelectionMode.Single;
					vm.OnSelectResult += AttachFine;
				});
		}

		private void AttachFine(object sender, JournalSelectedEventArgs e)
		{
			if(sender is JournalViewModelBase journal)
			{
				journal.OnSelectResult -= AttachFine;
			}
			
			var selectedObject = e.SelectedObjects.FirstOrDefault();

			if(!(selectedObject is FineJournalNode selectedNode))
			{
				return;
			}

			var carEvents = _carEventRepository.GetCarEventsByFine(UoW, selectedNode.Id);

			if(carEvents.Any())
			{
				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					$"Невозможно прикрепить данный штраф, так как он уже закреплён за другим событием:\n" +
					$"{string.Join(", ", carEvents.Select(ce => $"{ce.Id} - {ce.CarEventType.ShortName}"))}");

				return;
			}

			Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
		}

		private void CreateAddFine()
		{
			var page = NavigationManager.OpenViewModel<FineViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate(), OpenPageOptions.AsSlave);

			page.ViewModel.FineReasonString = Entity.GetFineReason();

			page.ViewModel.EntitySaved += (sender, e) =>
			{
				Entity.AddFine(e.Entity as Fine);
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

		private void WriteOffDocumentNotRequiredChanged()
		{
			if(Entity.IsWriteOffDocumentNotRequired)
			{
				Entity.WriteOffDocument = null;
			}

			OnPropertyChanged(nameof(CanAttachWriteOffDocument));
		}
	}
}
