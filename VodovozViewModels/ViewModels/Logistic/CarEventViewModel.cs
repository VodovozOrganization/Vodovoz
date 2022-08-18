using QS.Commands;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.FilterViewModels.Employees;
using Vodovoz.Infrastructure.Services;
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
		private readonly ICarEventSettings _carEventSettingsSettings = new CarEventSettings(new ParametersProvider());
		private DelegateCommand _changeDriverCommand;
		private DelegateCommand _changeEventTypeCommand;
		private IUndeliveredOrdersJournalOpener _undeliveryViewOpener;
		private IEntitySelectorFactory _employeeSelectorFactory;
		private IEmployeeSettings _employeeSettings;

		public bool CanEdit => PermissionResult.CanUpdate;
		public bool CanCreateOrEditWithClosedPeriod { get; }
		public bool CanAddFine => CanEdit;
		public bool CanAttachFine => CanEdit;
		public IEmployeeService EmployeeService { get; }
		public IEmployeeJournalFactory EmployeeJournalFactory { get; }
		public IList<FineItem> FineItems { get; private set; }

		public CarEventViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ICarJournalFactory carJournalFactory,
			ICarEventTypeJournalFactory carEventTypeJournalFactory,
			IEmployeeService employeeService,
			IEmployeeJournalFactory employeeJournalFactory,
			IUndeliveredOrdersJournalOpener undeliveryViewOpener,
			IEmployeeSettings employeeSettings
			)
			: base(uowBuilder, unitOfWorkFactory, commonServices)
		{
			_undeliveryViewOpener = undeliveryViewOpener ?? throw new ArgumentNullException(nameof(undeliveryViewOpener));
			EmployeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			EmployeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeSelectorFactory = EmployeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
			_employeeSettings = employeeSettings ?? throw new ArgumentNullException(nameof(employeeSettings));
			CanCreateOrEditWithClosedPeriod = 
				commonServices.CurrentPermissionService.ValidatePresetPermission("can_create_edit_car_events_in_closed_period");

			UpdateFileItems();

			Entity.ObservableFines.ListContentChanged += ObservableFines_ListContentChanged;

			if(employeeService == null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			CreateCommands();

			CarSelectorFactory = carJournalFactory.CreateCarAutocompleteSelectorFactory();
			CarEventTypeSelectorFactory = carEventTypeJournalFactory.CreateCarEventTypeAutocompleteSelectorFactory();
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingDriverEmployeeAutocompleteSelectorFactory();

			TabName = "Событие ТС";

			if(Entity.Id == 0)
			{
				Entity.Author = employeeService.GetEmployeeForUser(UoW, UserService.CurrentUserId);
				Entity.CreateDate = DateTime.Now;
			}
		}

		public bool CheckDatePeriod()
		{
			if(UoW.IsNew)
			{
				return true;
			}

			if(CanCreateOrEditWithClosedPeriod)
			{
				return true;
			}

			var today = DateTime.Now;
			DateTime startCurrentMonth = new DateTime(today.Year, today.Month, 1);
			DateTime startPreviousMonth = new DateTime(today.Year, today.Month - 1, 1);
			if(today.Day <= 10 && Entity.EndDate > startPreviousMonth)
			{
				return true;
			}

			if(today.Day > 10 && Entity.EndDate > startCurrentMonth)
			{
				return true;
			}

			return false;
		}

		public new void SaveAndClose()
		{
			if(Entity.StartDate == default)
			{
				ShowWarningMessage("Дата начала события должна быть указана.");
				return;
			}

			if(CanCreateOrEditWithClosedPeriod)
			{
				base.SaveAndClose();
				return;
			}

			var today = DateTime.Now;
			DateTime startCurrentMonth = new DateTime(today.Year, today.Month, 1);
			DateTime startPreviousMonth = new DateTime(today.Year, today.Month - 1, 1);
			if(today.Day <= 10 && Entity.EndDate < startPreviousMonth)
			{
				ShowWarningMessage($"С 1 по {10} текущего месяца можно создать/изменить событие ТС с датой завершения равной или более 1 числа прошлого месяца.");
				return;
			}

			if(today.Day > 10 && Entity.EndDate < startCurrentMonth)
			{
				ShowWarningMessage($"С {10 + 1} числа текущего месяца можно создать/изменить событие ТС с датой завершения равной или более 1 числа текущего месяца");
				return;
			}

			base.SaveAndClose();
		}

		public override void Dispose()
		{
			Entity.ObservableFines.ListContentChanged -= ObservableFines_ListContentChanged;
			base.Dispose();
		}

		public IEntityAutocompleteSelectorFactory CarSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory CarEventTypeSelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		public DelegateCommand ChangeDriverCommand => _changeDriverCommand ?? (_changeDriverCommand =
			new DelegateCommand(() =>
				{
					if(Entity.Car != null)
					{
						Entity.Driver = (Entity.Car.Driver != null && Entity.Car.Driver.Status != EmployeeStatus.IsFired)
							? Entity.Car.Driver
							: null;
					}
				},
				() => true
			));

		public DelegateCommand ChangeEventTypeCommand => _changeEventTypeCommand ?? (_changeEventTypeCommand =
			new DelegateCommand(() =>
				{
					if(Entity.CarEventType?.Id == _carEventSettingsSettings.DontShowCarEventByReportId)
					{
						Entity.DoNotShowInOperation = true;
					}
				},
				() => true
			));

		private void CreateCommands()
		{
			CreateAttachFineCommand();
			CreateAddFineCommand();
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
						   _employeeSelectorFactory,
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
			var fineFilter = new FineFilterViewModel()
			{
				CanEditFineDate = true,
				CanEditRouteListDate = true,
				CanEditSubdivision = true,
				ExcludedIds = Entity.Fines.Select(x => x.Id).ToArray()
			};

			return new FinesJournalViewModel(
				fineFilter,
				_undeliveryViewOpener,
				EmployeeService,
				_employeeSelectorFactory,
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
