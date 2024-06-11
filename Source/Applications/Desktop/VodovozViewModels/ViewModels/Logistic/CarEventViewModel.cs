﻿using Autofac;
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
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Logistics;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using static Vodovoz.Permissions.Logistic;
using Car = Vodovoz.Domain.Logistic.Cars.Car;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class CarEventViewModel : EntityTabViewModelBase<CarEvent>
	{
		private readonly ICarEventSettings _carEventSettings;
		private readonly ICarEventRepository _carEventRepository;
		private readonly ILifetimeScope _lifetimeScope;
		public string CarEventTypeCompensation = "Компенсация от страховой, по суду";
		private EntityEntryViewModel<Car> _viewModel;

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
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			ICarEventSettings carEventSettings,
			INavigationManager navigationManager,
			ICarEventRepository carEventRepository,
			ILifetimeScope lifetimeScope)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigationManager)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
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

			CanChangeCarEventType = !(IsTechInspectCarEventType && Entity.Id > 0);
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
			_viewModel.ChangedByUser -= OnCarChangedByUser;
			_viewModel.Dispose();
			base.Dispose();
		}

		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }
		public IEntityEntryViewModel OriginalCarEventViewModel { get; private set; }
		public IEntityEntryViewModel CarEntryViewModel { get; }

		private IEntityEntryViewModel BuildCarEntryViewModel()
		{
			var carViewModelBuilder = new CommonEEVMBuilderFactory<CarEvent>(this, Entity, UoW, NavigationManager, _lifetimeScope);

			_viewModel = carViewModelBuilder
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(
					filter =>
					{
					})
				.Finish();

			_viewModel.CanViewEntity = CommonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanUpdate;

			_viewModel.ChangedByUser += OnCarChangedByUser;

			return _viewModel;
		}

		private void OnCarChangedByUser(object sender, EventArgs e)
		{
			if(Entity.Car != null)
			{
				Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired)
				? Entity.Car.Driver
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
			OnPropertyChanged(nameof(IsTechInspectCarEventType));
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
			var page = NavigationManager.OpenViewModel<FinesJournalViewModel, Action<FineFilterViewModel>>(this, filter =>
			{
				filter.ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray();
			});

			page.ViewModel.SelectionMode = JournalSelectionMode.Single;

			page.ViewModel.OnSelectResult += (sender, e) =>
			{
				var selectedObject = e.SelectedObjects.FirstOrDefault();

				if(!(selectedObject is FineJournalNode selectedNode))
				{
					return;
				}

				var carEvent = _carEventRepository.GetCarEventByFine(UoW, selectedNode.Id);

				if(carEvent != null)
				{
					CommonServices.InteractiveService.ShowMessage(
						ImportanceLevel.Warning,
						$"Невозможно прикрепить данный штраф, так как он уже закреплён за другим событием:\n{carEvent.Id} - {carEvent.CarEventType.ShortName}");

					return;
				}

				Entity.AddFine(UoW.GetById<Fine>(selectedNode.Id));
			};
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

		public bool IsTechInspectCarEventType => Entity.CarEventType?.Id == _carEventSettings.TechInspectCarEventTypeId;

		public bool CanChangeCarEventType { get;}
	}
}
