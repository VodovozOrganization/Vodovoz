﻿using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.FilterViewModels
{
	public class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
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
		private ComplaintDetalizationJournalFilterViewModel _complaintDetalizationFilterViewModel;
		private ComplaintDetalization _complainDetalization;
		private DialogViewModelBase _journalViewModel;

		public ComplaintFilterViewModel()
		{
			UpdateWith(
				x => x.ComplaintType,
				x => x.ComplaintStatus,
				x => x.Employee,
				x => x.StartDate,
				x => x.EndDate,
				x => x.Subdivision,
				x => x.FilterDateType,
				x => x.ComplaintKind,
				x => x.ComplaintDiscussionStatus,
				x => x.ComplaintObject,
				x => x.CurrentUserSubdivision
			);
		}

		public ComplaintFilterViewModel(
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider,
			params Action<ComplaintFilterViewModel>[] filterParams)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider));
			CounterpartySelectorFactory =
				(counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory();
			EmployeeSelectorFactory =
				(employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory)))
				.CreateWorkingEmployeeAutocompleteSelectorFactory();
			GuiltyItemVM = new GuiltyItemViewModel(
				new ComplaintGuiltyItem(),
				commonServices,
				subdivisionRepository,
				employeeSelectorFactory.CreateEmployeeAutocompleteSelectorFactory(),
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

			SetAndRefilterAtOnce(filterParams);

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
		}

		public IEmployeeService EmployeeService { get; set; }

		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }

		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

		public IEntityEntryViewModel ComplaintDetalizationEntiryEntryViewModel { get; private set; }

		public virtual bool CanChangeSubdivision { get; }

		public virtual GuiltyItemViewModel GuiltyItemVM
		{
			get => _guiltyItemVM;
			set => SetField(ref _guiltyItemVM, value);
		}

		public virtual ComplaintKind ComplaintKind
		{
			get => _complaintKind;
			set
			{
				if(SetField(ref _complaintKind, value))
				{
					_complaintDetalizationFilterViewModel.RestrictComplaintKind = value;
				}
			}
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
					_complaintDetalizationFilterViewModel.RestrictComplaintObject = value;
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
				
				var entityEntryViewModel = 
					new CommonEEVMBuilderFactory<ComplaintFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope)
					.ForProperty(x => x.ComplainDetalization)
					.UseViewModelDialog<ComplaintDetalizationViewModel>()
					.UseViewModelJournalAndAutocompleter<ComplaintDetalizationJournalViewModel, ComplaintDetalizationJournalFilterViewModel>(
						filter => filter.ComplaintObject = ComplaintObject,
						filter => filter.ComplaintKind = ComplaintKind
					)
					.Finish();

				entityEntryViewModel.CanViewEntity = false;

				ComplaintDetalizationEntiryEntryViewModel = entityEntryViewModel;

				_complaintDetalizationFilterViewModel = new ComplaintDetalizationJournalFilterViewModel();
			}
		}
	}

	public enum DateFilterType
	{
		[Display(Name = "план. завершения")]
		PlannedCompletionDate,
		[Display(Name = "факт. завершения")]
		ActualCompletionDate,
		[Display(Name = "создания")]
		CreationDate
	}
}
