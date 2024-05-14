using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffViewModel : EntityTabViewModelBase<MileageWriteOff>
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ViewModelEEVMBuilder<Car> _carViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _authorViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<MileageWriteOffReason> _writeOffReasonViewModelEEVMBuilder;

		public MileageWriteOffViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			IEmployeeRepository employeeRepository,
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> authorViewModelEEVMBuilder,
			ViewModelEEVMBuilder<MileageWriteOffReason> writeOffReasonViewModelEEVMBuilder)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_carViewModelEEVMBuilder = carViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			_driverViewModelEEVMBuilder = driverViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(driverViewModelEEVMBuilder));
			_authorViewModelEEVMBuilder = authorViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(authorViewModelEEVMBuilder));
			_writeOffReasonViewModelEEVMBuilder = writeOffReasonViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(writeOffReasonViewModelEEVMBuilder));
			
			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true), () => CanCreateOrUpdate);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			CarChangedByUserCommand = new DelegateCommand(OnCarChangedByUser);
			WriteOffDateChangedCommand = new DelegateCommand(OnWriteOffDateChanged);
			DistanceChangedCommand = new DelegateCommand(OnDistanceChanged);

			UpdateCreationDateAndAuthorIfNeed();

			CarEntryViewModel = CreateCarEntryViewModel();
			DriverEntryViewModel = CreateDriverEntryViewModel();
			AuthorEntryViewModel = CreateAuthorEntryViewModell();
			WriteOffReasonEntryViewModel = CreateWriteOffReasonEntryViewModell();

			Entity.PropertyChanged += OnEntityPropertyChanged;
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand CarChangedByUserCommand { get; }
		public DelegateCommand WriteOffDateChangedCommand { get; }
		public DelegateCommand DistanceChangedCommand { get; }

		#region Permissions

		public bool CanCreate => PermissionResult.CanCreate;
		public bool CanRead => PermissionResult.CanRead;
		public bool CanUpdate => PermissionResult.CanUpdate;
		public bool CanDelete => PermissionResult.CanDelete;

		public bool CanCreateOrUpdate => Entity.Id == 0 ? CanCreate : CanUpdate;

		#endregion

		public IEntityEntryViewModel CarEntryViewModel { get; }
		public IEntityEntryViewModel DriverEntryViewModel { get; }
		public IEntityEntryViewModel AuthorEntryViewModel { get; }
		public IEntityEntryViewModel WriteOffReasonEntryViewModel { get; }

		private IEntityEntryViewModel CreateCarEntryViewModel()
		{
			var viewModel = _carViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
				{
					filter.Archive = false;
					filter.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company };
				})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateDriverEntryViewModel()
		{
			var viewModel = _driverViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateAuthorEntryViewModell()
		{
			var viewModel = _authorViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Author)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.office;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateWriteOffReasonEntryViewModell()
		{
			var viewModel = _writeOffReasonViewModelEEVMBuilder
				.SetViewModel(this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, x => x.Reason)
				.UseViewModelJournalAndAutocompleter<MileageWriteOffReasonJournalViewModel, MileageWriteOffReasonJournalFilterViewModel>(filter =>
				{
					filter.IsShowArchived = false;
				})
				.UseViewModelDialog<MileageWriteOffReasonViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private void CalculateLitersOutlayed()
		{
			if(Entity.Car == null || Entity.WriteOffDate == null)
			{
				Entity.LitersOutlayed = 0;
				return;
			}

			var activeFuelVersion = Entity.Car?.CarModel.GetCarFuelVersionOnDate(Entity.WriteOffDate.Value);

			if(activeFuelVersion is null)
			{
				Entity.LitersOutlayed = 0;

				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Расчет потраченного топлива выполнить не удалось.\n" +
					$"На указанную дату списания {Entity.WriteOffDate.Value} отсутствует версия топлива для модели авто {Entity.Car?.CarModel.Name}");

				return;
			}

			var fuelConsumption = (decimal)activeFuelVersion.FuelConsumption;

			if(fuelConsumption == 0)
			{
				Entity.LitersOutlayed = 0;

				CommonServices.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					$"Расчет потраченного топлива выполнить не удалось.\n" +
					$"На указанную дату списания {Entity.WriteOffDate.Value} в версии топлива для модели авто {Entity.Car.CarModel.Name} расход топлива установлен 0");

				return;
			}

			Entity.LitersOutlayed = fuelConsumption * Entity.DistanceKm / 100;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.WriteOffDate))
			{
				WriteOffDateChangedCommand.Execute();
			}
			if(e.PropertyName == nameof(Entity.DistanceKm))
			{
				DistanceChangedCommand.Execute();
			}
		}

		private void OnCarChangedByUser()
		{
			Entity.Driver = Entity.Car?.Driver;
			CalculateLitersOutlayed();
		}
		private void OnWriteOffDateChanged()
		{
			CalculateLitersOutlayed();
		}

		private void OnDistanceChanged()
		{
			CalculateLitersOutlayed();
		}

		private void UpdateCreationDateAndAuthorIfNeed()
		{
			if(!UoW.IsNew)
			{
				return;
			}

			Entity.CreationDate = DateTime.Now;
			Entity.Author = _employeeRepository.GetEmployeeForCurrentUser(UoW);
		}

		protected override bool BeforeSave()
		{
			UpdateCreationDateAndAuthorIfNeed();

			return base.BeforeSave();
		}
	}
}
