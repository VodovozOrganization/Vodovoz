using QS.Project.Filter;
using QS.Project.Journal;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffJournalFilterViewModel : FilterViewModelBase<MileageWriteOffJournalFilterViewModel>
	{
		private DateTime? _writeOffDateFrom;
		private DateTime? _writeOffDateTo;
		private Car _car;
		private Employee _driver;
		private Employee _author;
		private JournalViewModelBase _journal;
		private readonly ViewModelEEVMBuilder<Car> _carViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _driverViewModelEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _authorViewModelEEVMBuilder;

		public MileageWriteOffJournalFilterViewModel(
			ViewModelEEVMBuilder<Car> carViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> driverViewModelEEVMBuilder,
			ViewModelEEVMBuilder<Employee> authorViewModelEEVMBuilder)
		{
			_carViewModelEEVMBuilder = carViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(carViewModelEEVMBuilder));
			_driverViewModelEEVMBuilder = driverViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(driverViewModelEEVMBuilder));
			_authorViewModelEEVMBuilder = authorViewModelEEVMBuilder ?? throw new ArgumentNullException(nameof(authorViewModelEEVMBuilder));
		}

		public DateTime? WriteOffDateFrom
		{
			get => _writeOffDateFrom;
			set => UpdateFilterField(ref _writeOffDateFrom, value);
		}

		public DateTime? WriteOffDateTo
		{
			get => _writeOffDateTo;
			set => UpdateFilterField(ref _writeOffDateTo, value);
		}

		public Car Car
		{
			get => _car;
			set => UpdateFilterField(ref _car, value);
		}

		public Employee Driver
		{
			get => _driver;
			set => UpdateFilterField(ref _driver, value);
		}

		public Employee Author
		{
			get => _author;
			set => UpdateFilterField(ref _author, value);
		}

		public IEntityEntryViewModel CarEntryViewModel { get; private set; }
		public IEntityEntryViewModel DriverEntryViewModel { get; private set; }
		public IEntityEntryViewModel AuthorEntryViewModel { get; private set; }

		public JournalViewModelBase Journal
		{
			get => _journal;
			set
			{
				if(SetField(ref _journal, value) && value != null)
				{
					SetCarViewModel();
					SetDriverViewModel();
					SetAuthorViewModell();
				}
			}
		}

		private void SetCarViewModel()
		{
			if(CarEntryViewModel != null)
			{
				return;
			}

			var viewModel = _carViewModelEEVMBuilder
				.SetViewModel(_journal)
				.SetUnitOfWork(_journal.UoW)
				.ForProperty(this, x => x.Car)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
				{
					filter.Archive = false;
					filter.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company };
				})
				.UseViewModelDialog<CarViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			CarEntryViewModel = viewModel;
		}

		private void SetDriverViewModel()
		{
			if(DriverEntryViewModel != null)
			{
				return;
			}

			var viewModel = _driverViewModelEEVMBuilder
				.SetViewModel(_journal)
				.SetUnitOfWork(_journal.UoW)
				.ForProperty(this, x => x.Driver)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			DriverEntryViewModel = viewModel;
		}

		private void SetAuthorViewModell()
		{
			if(AuthorEntryViewModel != null)
			{
				return;
			}

			var viewModel = _authorViewModelEEVMBuilder
				.SetViewModel(_journal)
				.SetUnitOfWork(_journal.UoW)
				.ForProperty(this, x => x.Author)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.office;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();

			viewModel.CanViewEntity = false;

			AuthorEntryViewModel = viewModel;
		}
	}
}
