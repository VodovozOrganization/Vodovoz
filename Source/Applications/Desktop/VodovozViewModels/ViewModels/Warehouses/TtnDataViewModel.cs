using Autofac;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Employees;
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
		private readonly IInteractiveService _interactiveService;
		private readonly ILifetimeScope _scope;

		private readonly ViewModelEEVMBuilder<Organization> _organizationEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Car> _carEEVMBuilder;
		private readonly ViewModelEEVMBuilder<Employee> _employeeEEVMBuilder;

		private DelegateCommand _acceptCommand;
		private DelegateCommand _cancelCommand;

		public TtnDataViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			ViewModelEEVMBuilder<Organization> organizationEEVMBuilder,
			ViewModelEEVMBuilder<Car> carEEVMBuilder,
			ViewModelEEVMBuilder<Employee> employeeEEVMBuilder)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_organizationEEVMBuilder = organizationEEVMBuilder ?? throw new ArgumentNullException(nameof(organizationEEVMBuilder));
			_carEEVMBuilder = carEEVMBuilder ?? throw new ArgumentNullException(nameof(carEEVMBuilder));
			_employeeEEVMBuilder = employeeEEVMBuilder ?? throw new ArgumentNullException(nameof(employeeEEVMBuilder));

			Title = "Данные для печати ТТН";

			AcceptCommand = new DelegateCommand(Accept, () => CanAccept);
			CancelCommand = new DelegateCommand(Cancel);
		}

		public IEntityEntryViewModel CargoSenderViewModel { get; set; }
		public IEntityEntryViewModel CargoReceiverViewModel { get; set; }
		public IEntityEntryViewModel PayerViewModel { get; set; }
		public IEntityEntryViewModel CarViewModel { get; set; }
		public IEntityEntryViewModel TrailerViewModel { get; set; }
		public IEntityEntryViewModel DriverViewModel { get; set; }

		public bool CanAccept => true;
		public DelegateCommand AcceptCommand { get; }
		public DelegateCommand CancelCommand { get; }

		/// <summary>
		/// Строит все EntityEntryViewModel с владельцем <c>this</c> (TtnDataViewModel),
		/// чтобы вложенные журналы открывались от вкладки ТТН.
		/// </summary>
		public void Configure(MovementDocument entity)
		{
			if(entity is null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			CargoSenderViewModel = BuildOrganizationEntryViewModel(entity, e => e.TtnCargoSender);
			CargoReceiverViewModel = BuildOrganizationEntryViewModel(entity, e => e.TtnCargoReceiver);
			PayerViewModel = BuildOrganizationEntryViewModel(entity, e => e.TtnPayer);
			CarViewModel = BuildCarEntryViewModel(entity, e => e.TtnCar);
			TrailerViewModel = BuildTrailerEntryViewModel(entity, e => e.TtnSemitrailer);
			DriverViewModel = BuildDriverEntryViewModel(entity, e => e.TtnDriver);
		}

		private IEntityEntryViewModel BuildOrganizationEntryViewModel(
			MovementDocument entity,
			System.Linq.Expressions.Expression<Func<MovementDocument, Organization>> property)
		{
			return _organizationEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.UseViewModelDialog<OrganizationViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildCarEntryViewModel(
			MovementDocument entity,
			System.Linq.Expressions.Expression<Func<MovementDocument, Car>> property)
		{
			return _carEEVMBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(entity, property)
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(f => { })
				.UseViewModelDialog<CarViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildTrailerEntryViewModel(
			MovementDocument entity,
			System.Linq.Expressions.Expression<Func<MovementDocument, Car>> property)
		{
			return _carEEVMBuilder
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
			MovementDocument entity,
			System.Linq.Expressions.Expression<Func<MovementDocument, Employee>> property)
		{
			return _employeeEEVMBuilder
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
