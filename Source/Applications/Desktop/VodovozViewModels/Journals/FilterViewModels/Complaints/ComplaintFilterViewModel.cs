using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.FilterViewModels
{
	public class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
	{
		private readonly ICommonServices commonServices;
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider;
		private IList<ComplaintObject> _complaintObjectSource;
		private ComplaintObject _complaintObject;
		private readonly IList<ComplaintKind> _complaintKinds;
		private bool _isForSalesDepartment;

		public IEmployeeService EmployeeService { get; set; }
		
		public IEntityAutocompleteSelectorFactory CounterpartySelectorFactory { get; }
		public IEntityAutocompleteSelectorFactory EmployeeSelectorFactory { get; }

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
			ISubdivisionRepository subdivisionRepository,
			IEmployeeJournalFactory employeeSelectorFactory,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			ISubdivisionParametersProvider subdivisionParametersProvider
		) {

			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_subdivisionParametersProvider = subdivisionParametersProvider ?? throw new ArgumentNullException(nameof(subdivisionParametersProvider)); ;
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
				true
			);

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

			_complaintKinds = complaintKindSource = UoW.GetAll<ComplaintKind>().ToList();

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
				x => x.ComplaintDiscussionStatus,
				x => x.ComplaintObject,
				x => x.CurrentUserSubdivision
			);
		}

		public virtual bool CanChangeSubdivision { get; }

		GuiltyItemViewModel guiltyItemVM;
		public virtual GuiltyItemViewModel GuiltyItemVM {
			get => guiltyItemVM;
			set => SetField(ref guiltyItemVM, value);
		}

		ComplaintKind complaintKind;
		public virtual ComplaintKind ComplaintKind {
			get => complaintKind;
			set => SetField(ref complaintKind, value);
		}

		private IList<Subdivision> allDepartments;
		public IList<Subdivision> AllDepartments
		{
			get => allDepartments;
			private set => SetField(ref allDepartments, value);
		}

		public virtual ComplaintObject ComplaintObject
		{
			get => _complaintObject;
			set
			{
				if(SetField(ref _complaintObject, value))
				{
					ComplaintKindSource = value == null ? _complaintKinds : _complaintKinds.Where(x => x.ComplaintObject == value).ToList();
				}
			}
		}

		private DateFilterType filterDateType = DateFilterType.CreationDate;
		public virtual DateFilterType FilterDateType {
			get => filterDateType;
			set => SetField(ref filterDateType, value);
		}

		private ComplaintType? complaintType = Domain.Complaints.ComplaintType.Client;
		public virtual ComplaintType? ComplaintType {
			get => complaintType;
			set => SetField(ref complaintType, value);
		}

		private ComplaintStatuses? complaintStatus;
		public virtual ComplaintStatuses? ComplaintStatus {
			get => complaintStatus;
			set => SetField(ref complaintStatus, value);
		}

		private ComplaintDiscussionStatuses? complaintDiscussionStatus;
		public virtual ComplaintDiscussionStatuses? ComplaintDiscussionStatus
		{
			get => complaintDiscussionStatus;
			set => SetField(ref complaintDiscussionStatus, value);
		}

		private Subdivision currentUserSubdivision;
		public virtual Subdivision CurrentUserSubdivision {
			get => currentUserSubdivision;
			set => SetField(ref currentUserSubdivision, value);
		}

		private Employee employee;
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private Counterparty counterparty;
		public virtual Counterparty Counterparty
		{
			get { return counterparty; }
			set { SetField(ref counterparty, value); }
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set => SetField(ref subdivision, value);
		}

		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime endDate = DateTime.Now;
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		private bool? isForRetail;
		public bool? IsForRetail
		{
			get => isForRetail;
			set => SetField(ref isForRetail, value);
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
				throw new NullReferenceException("Отсутствует ссылка на EmployeeService");

			Subdivision = null;
			ComplaintStatus = null;
			ComplaintType = null;
			StartDate = DateTime.Now.AddMonths(-3);
			EndDate = DateTime.Now.AddMonths(3);
			Employee = EmployeeService.GetEmployeeForUser(UoW, commonServices.UserService.CurrentUserId);
		}

		IList<ComplaintKind> complaintKindSource;
		public IList<ComplaintKind> ComplaintKindSource {
			get => complaintKindSource;
			set => SetField(ref complaintKindSource, value);
		}

		public IEnumerable<ComplaintObject> ComplaintObjectSource => 
			_complaintObjectSource ?? (_complaintObjectSource = UoW.GetAll<ComplaintObject>().ToList());
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
