﻿using Autofac;
using QS.Commands;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Employees;
using Vodovoz.ViewModels.QualityControl.Reports;
using Vodovoz.ViewModels.ViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Employees;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.FilterViewModels
{
	public partial class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
	{
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;
		private bool _isForSalesDepartment;
		private GuiltyItemViewModel _guiltyItemVM;
		private ComplaintKind _complaintKind;
		private IList<Subdivision> _allDepartments;
		private DateFilterType _filterDateType = DateFilterType.CreationDate;
		private ComplaintType? _complaintType = Domain.Complaints.ComplaintType.Client;
		private ComplaintStatuses? _complaintStatus;
		private ComplaintDiscussionStatuses? _complaintDiscussionStatus;
		private Subdivision _currentUserSubdivision;
		private Employee _employee;
		private Counterparty _counterparty;
		private Subdivision _subdivision;
		private DateTime? _startDate;
		private DateTime _endDate = DateTime.Now;
		private bool? _isForRetail;
		private IList<ComplaintKind> _complaintKindSource;
		private ComplaintDetalization _complainDetalization;
		private DialogViewModelBase _journalViewModel;
		private ComplaintKindJournalFilterViewModel _complaintKindJournalFilter;

		public ComplaintFilterViewModel(
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeJournalFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			Action<ComplaintFilterViewModel> filterParams = null)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();
			EmployeeSelectorFactory =
				(employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();
			InitializeComplaintKindAutocompleteSelectorFactory();
			GuiltyItemVM = new GuiltyItemViewModel(
				new ComplaintGuiltyItem(),
				commonServices,
				subdivisionRepository,
				employeeJournalFactory,
				_subdivisionParametersProvider,
				UoW,
				true);

			AllDepartments = subdivisionRepository.GetAllDepartmentsOrderedByName(UoW);
			CanChangeSubdivision = commonServices.CurrentPermissionService.ValidatePresetPermission("can_change_subdivision_on_complaint");

			GuiltyItemVM.Entity.OnGuiltyTypeChange = () =>
			{
				if(GuiltyItemVM.Entity.Responsible == null || !GuiltyItemVM.Entity.Responsible.IsEmployeeResponsible)
				{
					GuiltyItemVM.Entity.Employee = null;
				}
				if(GuiltyItemVM.Entity.Responsible == null || !GuiltyItemVM.Entity.Responsible.IsSubdivisionResponsible)
				{
					GuiltyItemVM.Entity.Subdivision = null;
				}
			};

			GuiltyItemVM.OnGuiltyItemReady += (sender, e) => Update();

			_complaintKinds = _complaintKindSource = UoW.GetAll<ComplaintKind>().ToList();

			if(filterParams != null)
			{
				SetAndRefilterAtOnce(filterParams);
			}

			UpdateWith(
				x => x.ComplaintType,
				x => x.ComplaintStatus,
				x => x.Counterparty,
				x => x.Employee,
				x => x.StartDate,
				x => x.EndDate,
				x => x.Subdivision,
				x => x.FilterDateType,
				x => x.ComplaintKind,
				x => x.ComplainDetalization,
				x => x.ComplaintDiscussionStatus,
				x => x.ComplaintObject,
				x => x.CurrentUserSubdivision);

			OpenNumberOfComplaintsAgainstDriversReportTabCommand = new DelegateCommand(OpenNumberOfComplaintsAgainstDriversReportTab);
		}

		private IEntityEntryViewModel BuildAuthorViewModel(DialogViewModelBase journal)
		{
			return new CommonEEVMBuilderFactory<ComplaintFilterViewModel>(journal, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Employee)
				.UseViewModelDialog<EmployeeViewModel>()
				.UseViewModelJournalAndAutocompleter<EmployeesJournalViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildAtWorkInSubdivisionViewModel(DialogViewModelBase journal)
		{
			return new CommonEEVMBuilderFactory<ComplaintFilterViewModel>(journal, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Subdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildCurrentSubdivisionViewModel(DialogViewModelBase journal)
		{
			var currentSubdivisionViewModel = new CommonEEVMBuilderFactory<ComplaintFilterViewModel>(journal, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.CurrentUserSubdivision)
				.UseViewModelDialog<SubdivisionViewModel>()
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.Finish();

			currentSubdivisionViewModel.IsEditable = CanChangeSubdivision;

			return currentSubdivisionViewModel;
		}

		public IEmployeeService EmployeeService { get; set; }

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		public IEntityEntryViewModel CurrentSubdivisionViewModel { get; private set; }

		public IEntityEntryViewModel AtWorkInSubdivisionViewModel { get; private set; }

		public IEntityAutocompleteSelectorFactory ComplaintKindSelectorFactory { get; private set; }

		public IEntityEntryViewModel ComplaintDetalizationEntiryEntryViewModel { get; private set; }

		public IEntityEntryViewModel AuthorEntiryEntryViewModel { get; private set; }

		public IEntityEntryViewModel CounterpartyEntiryEntryViewModel { get; private set; }

		#region Commands

		public DelegateCommand OpenNumberOfComplaintsAgainstDriversReportTabCommand { get; }

		#endregion Commands

		public virtual bool CanChangeSubdivision { get; }

		public bool CanReadDetalization => _commonServices.CurrentPermissionService
					.ValidateEntityPermission(typeof(ComplaintDetalization)).CanRead;

		public virtual GuiltyItemViewModel GuiltyItemVM
		{
			get => _guiltyItemVM;
			set => SetField(ref _guiltyItemVM, value);
		}

		public virtual ComplaintKind ComplaintKind
		{
			get => _complaintKind;
			set => SetField(ref _complaintKind, value);
		}

		public ComplaintDetalization ComplainDetalization
		{
			get => _complainDetalization;
			set => SetField(ref _complainDetalization, value);
		}

		public IList<Subdivision> AllDepartments
		{
			get => _allDepartments;
			private set => SetField(ref _allDepartments, value);
		}

		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(SetField(ref _complaintObject, value))
				{
					ComplaintKindSource = value == null ? _complaintKinds : _complaintKinds.Where(x => x.ComplaintObject == value).ToList();
					_complaintKindJournalFilter.ComplaintObject = _complaintObject;
				}
			}
		}

		public virtual DateFilterType FilterDateType
		{
			get => _filterDateType;
			set => SetField(ref _filterDateType, value);
		}

		public virtual ComplaintType? ComplaintType
		{
			get => _complaintType;
			set => SetField(ref _complaintType, value);
		}

		public virtual ComplaintStatuses? ComplaintStatus
		{
			get => _complaintStatus;
			set => SetField(ref _complaintStatus, value);
		}

		public virtual ComplaintDiscussionStatuses? ComplaintDiscussionStatus
		{
			get => _complaintDiscussionStatus;
			set => SetField(ref _complaintDiscussionStatus, value);
		}

		public virtual Subdivision CurrentUserSubdivision
		{
			get => _currentUserSubdivision;
			set => SetField(ref _currentUserSubdivision, value);
		}

		public virtual Employee Employee
		{
			get => _employee;
			set => SetField(ref _employee, value);
		}

		public virtual Counterparty Counterparty
		{
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => SetField(ref _subdivision, value);
		}

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public virtual DateTime EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public bool? IsForRetail
		{
			get => _isForRetail;
			set => SetField(ref _isForRetail, value);
		}

		public bool IsForSalesDepartment
		{
			get => _isForSalesDepartment;
			set
			{
				if(SetField(ref _isForSalesDepartment, value))
				{
					GuiltyItemVM.IsForSalesDepartment = value;
				}
			}
		}

		public void SelectMyComplaint()
		{
			if(EmployeeService == null)
			{
				throw new NullReferenceException("Отсутствует ссылка на EmployeeService");
			}

			Subdivision = null;
			ComplaintStatus = null;
			ComplaintType = null;
			StartDate = DateTime.Now.AddMonths(-3);
			EndDate = DateTime.Now.AddMonths(3);
			Employee = EmployeeService.GetEmployeeForUser(UoW, _commonServices.UserService.CurrentUserId);
		}

		public IList<ComplaintKind> ComplaintKindSource
		{
			get => _complaintKindSource;
			set => SetField(ref _complaintKindSource, value);
		}

		public IEnumerable<ComplaintObject> ComplaintObjectSource =>
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().ToList());

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				ComplaintDetalizationEntiryEntryViewModel = BuildComplaintDetalizationViewModel(value);
				CurrentSubdivisionViewModel = BuildCurrentSubdivisionViewModel(value);
				AtWorkInSubdivisionViewModel = BuildAtWorkInSubdivisionViewModel(value);
				AuthorEntiryEntryViewModel = BuildAuthorViewModel(value);

				GuiltyItemVM.SubdivisionViewModel = BuildeGuiltyItemSubdivisionViewModel(value);
			}
		}

		private IEntityEntryViewModel BuildeGuiltyItemSubdivisionViewModel(DialogViewModelBase journal)
		{
			return new CommonEEVMBuilderFactory<ComplaintGuiltyItem>(journal, GuiltyItemVM.Entity, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.Subdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}

		private IEntityEntryViewModel BuildComplaintDetalizationViewModel(DialogViewModelBase journal)
		{
			var complaintDetalizationViewModel =
				new CommonEEVMBuilderFactory<ComplaintFilterViewModel>(journal, this, UoW, _navigationManager, _lifetimeScope)
				.ForProperty(x => x.ComplainDetalization)
				.UseViewModelDialog<ComplaintDetalizationViewModel>()
				.UseViewModelJournalAndAutocompleter<ComplaintDetalizationJournalViewModel, ComplaintDetalizationJournalFilterViewModel>(
					filter =>
					{
						filter.RestrictComplaintObject = ComplaintObject;
						filter.RestrictComplaintKind = ComplaintKind;
					}
				)
				.Finish();

			complaintDetalizationViewModel.CanViewEntity = false;

			return complaintDetalizationViewModel;
		}

		private void OpenNumberOfComplaintsAgainstDriversReportTab()
		{
			_navigationManager.OpenViewModel<NumberOfComplaintsAgainstDriversReportViewModel>(JournalViewModel);
		}
		
		private void InitializeComplaintKindAutocompleteSelectorFactory()
		{
			_complaintKindJournalFilter = _lifetimeScope.Resolve<ComplaintKindJournalFilterViewModel>();
			_complaintKindJournalFilter.IsShow = true;
			ComplaintKindSelectorFactory =
				_lifetimeScope.Resolve<IComplaintKindJournalFactory>(
						new TypedParameter(typeof(ComplaintKindJournalFilterViewModel), _complaintKindJournalFilter))
					.CreateComplaintKindAutocompleteSelectorFactory();
		}
	}
}
