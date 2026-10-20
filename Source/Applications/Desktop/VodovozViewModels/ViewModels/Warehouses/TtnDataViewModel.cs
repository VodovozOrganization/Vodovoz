using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Warehouses
{
	public class TtnDataViewModel : DialogTabViewModelBase
	{
		private readonly ViewModelEEVMBuilder<Organization> _cargoSenderEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Organization> _cargoReceiverEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Organization> _payerEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Car> _carEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Car> _semitrailerEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _employeeEEVMBuilder;

		public TtnDataViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ViewModelEEVMBuilder<Organization> cargoSenderEEVMBuilder,
			ViewModelEEVMBuilder<Organization> cargoReceiverEEVMBuilder,
			ViewModelEEVMBuilder<Organization> payerEEVMBuilder,
			ViewModelEEVMBuilder<Car> carEEVMBuilder,
			ViewModelEEVMBuilder<Car> semitrailerEEVMBuilder,
			ViewModelEEVMBuilder<Employee> employeeEEVMBuilder)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_cargoSenderEEVMBuilder = cargoSenderEEVMBuilder
				?? throw new ArgumentNullException(nameof(cargoSenderEEVMBuilder));
			_cargoReceiverEEVMBuilder = cargoReceiverEEVMBuilder
				?? throw new ArgumentNullException(nameof(cargoReceiverEEVMBuilder));
			_payerEEVMBuilder = payerEEVMBuilder
				?? throw new ArgumentNullException(nameof(payerEEVMBuilder));
			_carEEVMBuilder = carEEVMBuilder
				?? throw new ArgumentNullException(nameof(carEEVMBuilder));
			_semitrailerEEVMBuilder = semitrailerEEVMBuilder
				?? throw new ArgumentNullException(nameof(semitrailerEEVMBuilder));
			_employeeEEVMBuilder = employeeEEVMBuilder
				?? throw new ArgumentNullException(nameof(employeeEEVMBuilder));

			Title = "Данные для печати ТТН";

			AcceptCommand = new DelegateCommand(Accept, () => CanAccept);
			CancelCommand = new DelegateCommand(Cancel);
		}

		public IEntityEntryViewModel CargoSenderViewModel { get; private set; }
		public IEntityEntryViewModel CargoReceiverViewModel { get; private set; }
		public IEntityEntryViewModel PayerViewModel { get; private set; }
		public IEntityEntryViewModel CarViewModel { get; private set; }
		public IEntityEntryViewModel TrailerViewModel { get; private set; }
		public IEntityEntryViewModel DriverViewModel { get; private set; }

		public bool CanAccept => true;
		public DelegateCommand AcceptCommand { get; }
		public DelegateCommand CancelCommand { get; }

		public void Configure(MovementDocument entity)
		{
			if(entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			CargoSenderViewModel = BuildOrganizationEntryViewModel(
				_cargoSenderEEVMBuilder, entity, e => e.CargoSender);

			CargoReceiverViewModel = BuildOrganizationEntryViewModel(
				_cargoReceiverEEVMBuilder, entity, e => e.CargoReceiver);

			PayerViewModel = BuildOrganizationEntryViewModel(
				_payerEEVMBuilder, entity, e => e.Payer);

			CarViewModel = BuildCarEntryViewModel(
				_carEEVMBuilder, entity, e => e.Car);

			TrailerViewModel = BuildTrailerEntryViewModel(
				_semitrailerEEVMBuilder, entity, e => e.Semitrailer);

			DriverViewModel = BuildDriverEntryViewModel(
				_employeeEEVMBuilder, entity, e => e.Driver);
		}

		private IEntityEntryViewModel BuildOrganizationEntryViewModel(
			ViewModelEEVMBuilder<Organization> builder,
			MovementDocument entity,
			Expression<Func<MovementDocument, Organization>> property)
		{
			return builder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildCarEntryViewModel(
			ViewModelEEVMBuilder<Car> builder,
			MovementDocument entity,
			Expression<Func<MovementDocument, Car>> property)
		{
			return builder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(f => { })
				.UseViewModelDialog<CarViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildTrailerEntryViewModel(
			ViewModelEEVMBuilder<Car> builder,
			MovementDocument entity,
			Expression<Func<MovementDocument, Car>> property)
		{
			return builder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(f =>
				{
					f.RestrictedCarTypesOfUse = new[] { CarTypeOfUse.Semitrailer };
					f.Archive = false;
				})
				.UseViewModelDialog<SemitrailerViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildDriverEntryViewModel(
			ViewModelEEVMBuilder<Employee> builder,
			MovementDocument entity,
			Expression<Func<MovementDocument, Employee>> property)
		{
			return builder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(f =>
				{
					f.Status = EmployeeStatus.IsWorking;
					f.RestrictCategory = EmployeeCategory.driver;
				})
				.UseViewModelDialog<EmployeeViewModel>()
				.Finish();
		}

		private void Accept() => Close(false, CloseSource.Save);
		private void Cancel() => Close(true, CloseSource.Cancel);
	}
}
