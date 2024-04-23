using Autofac;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
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
		private readonly ILifetimeScope _lifetimeScope;

		public MileageWriteOffViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation,
			ILifetimeScope lifetimeScope,
			IEmployeeRepository employeeRepository)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			if(employeeRepository is null)
			{
				throw new ArgumentNullException(nameof(employeeRepository));
			}

			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			if(!CanRead)
			{
				AbortOpening("У вас недостаточно прав для просмотра");
			}

			TabName =
				UoWGeneric.IsNew
				? $"Диалог создания {Entity.GetType().GetClassUserFriendlyName().Genitive}"
				: $"{Entity.GetType().GetClassUserFriendlyName().Nominative.CapitalizeSentence()} №{Entity.Title}";

			SaveCommand = new DelegateCommand(() => Save(true), () => CanCreateOrUpdate);
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			CarChangedByUserCommand = new DelegateCommand(OnCarChangedByUser);

			if(UoW.IsNew)
			{
				Entity.CreationDate = DateTime.Today;
				Entity.Author = employeeRepository.GetEmployeeForCurrentUser(UoW);
			}

			CarEntryViewModel = CreateCarEntryViewModel();
			DriverEntryViewModel = CreateDriverEntryViewModel();
			AuthorEntryViewModel = CreateAuthorEntryViewModell();
			WriteOffReasonEntryViewModel = CreateWriteOffReasonEntryViewModell();
		}

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand CarChangedByUserCommand { get; }

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
			var viewModel =
				new CommonEEVMBuilderFactory<MileageWriteOff>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Car)
				.UseViewModelDialog<CarViewModel>()
				.UseViewModelJournalAndAutocompleter<CarJournalViewModel, CarJournalFilterViewModel>(filter =>
				{
					filter.Archive = false;
					filter.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company };
				})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateDriverEntryViewModel()
		{
			var viewModel =
				new CommonEEVMBuilderFactory<MileageWriteOff>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Driver)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.driver;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateAuthorEntryViewModell()
		{
			var viewModel =
				new CommonEEVMBuilderFactory<MileageWriteOff>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Author)
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel, EmployeeFilterViewModel>(filter =>
				{
					filter.Category = EmployeeCategory.office;
					filter.Status = EmployeeStatus.IsWorking;
				})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private IEntityEntryViewModel CreateWriteOffReasonEntryViewModell()
		{
			var viewModel =
				new CommonEEVMBuilderFactory<MileageWriteOff>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Reason)
				.UseViewModelJournalAndAutocompleter<MileageWriteOffReasonJournalViewModel, MileageWriteOffReasonJournalFilterViewModel>(filter =>
				{
					filter.IsShowArchived = false;
				})
				.Finish();

			viewModel.CanViewEntity = false;

			return viewModel;
		}

		private void OnCarChangedByUser()
		{
			Entity.Driver = Entity.Car?.Driver;
		}
	}
}
