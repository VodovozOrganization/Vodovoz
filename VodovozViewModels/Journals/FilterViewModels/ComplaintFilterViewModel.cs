using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Project.Filter;
using QS.Project.Journal.EntitySelector;
using QS.Report;
using QS.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.FilterViewModels
{
	public class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
	{
		public ISubdivisionService SubdivisionService { get; set; }

		public IEmployeeRepository EmployeeRepository { get; set; }

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
				x => x.ComplaintKind
			);
		}

		public ComplaintFilterViewModel(
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			IEntityAutocompleteSelectorFactory employeeSelectorFactory
		)
		{
			GuiltyItemVM = new GuiltyItemViewModel(
				new ComplaintGuiltyItem(),
				commonServices,
				subdivisionRepository,
				employeeSelectorFactory
			) {
				UoW = UoW
			};

			GuiltyItemVM.Entity.OnGuiltyTypeChange = () => {
				if(GuiltyItemVM.Entity.GuiltyType != ComplaintGuiltyTypes.Employee)
					GuiltyItemVM.Entity.Employee = null;
				if(GuiltyItemVM.Entity.GuiltyType != ComplaintGuiltyTypes.Subdivision)
					GuiltyItemVM.Entity.Subdivision = null;
			};
			GuiltyItemVM.OnGuiltyItemReady += (sender, e) => Update();

			UpdateWith(
				x => x.ComplaintType,
				x => x.ComplaintStatus,
				x => x.Employee,
				x => x.StartDate,
				x => x.EndDate,
				x => x.Subdivision,
				x => x.FilterDateType,
				x => x.ComplaintKind
			);
		}

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

		private DateFilterType filterDateType = DateFilterType.PlannedCompletionDate;
		public virtual DateFilterType FilterDateType {
			get => filterDateType;
			set => SetField(ref filterDateType, value);
		}

		private ComplaintType? complaintType;
		public virtual ComplaintType? ComplaintType {
			get => complaintType;
			set => SetField(ref complaintType, value, () => ComplaintType);
		}

		private ComplaintStatuses? complaintStatus;
		public virtual ComplaintStatuses? ComplaintStatus {
			get => complaintStatus;
			set => SetField(ref complaintStatus, value, () => ComplaintStatus);
		}

		private Employee employee;
		public virtual Employee Employee {
			get { return employee; }
			set { SetField(ref employee, value); }
		}

		private Subdivision subdivision;
		public virtual Subdivision Subdivision {
			get => subdivision;
			set {
				if(value?.Id == SubdivisionService?.GetOkkId())
					ComplaintStatus = ComplaintStatuses.Checking;
				else if(value?.Id != null)
					ComplaintStatus = ComplaintStatuses.InProcess;

				SetField(ref subdivision, value, () => Subdivision);
			}
		}

		private DateTime? startDate;
		public virtual DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime endDate = DateTime.Now;
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}

		public void SelectMyComplaint()
		{
			if(EmployeeRepository == null)
				throw new NullReferenceException("Отсутствует ссылка на EmployeeRepository");

			Subdivision = null;
			ComplaintStatus = null;
			ComplaintType = null;
			StartDate = DateTime.Now.AddMonths(-3);
			EndDate = DateTime.Now.AddMonths(3);
			Employee = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
		}

		List<ComplaintKind> complaintKindSorce;
		public IEnumerable<ComplaintKind> ComplaintKindSource {
			get {
				if(complaintKindSorce == null)
					complaintKindSorce = UoW.GetAll<ComplaintKind>().ToList();
				return complaintKindSorce;
			}
		}

		public ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = "Рекламации",
				Identifier = "Complaints",
				Parameters = new Dictionary<string, object>
				{
					{ "subdivision_id", Subdivision?.Id ?? 0},
					{ "start_date", StartDate ?? null},
					{ "end_date", EndDate},
					{ "employee_id", Employee?.Id ?? 0},
					{ "status", ComplaintStatus?.ToString() ?? String.Empty},
					{ "date_type", filterDateType},
					{ "type", ComplaintType?.ToString() ?? String.Empty},
					{ "guilty_type", guiltyItemVM.Entity.GuiltyType?.ToString() ?? String.Empty},
					{ "complaint_kind", complaintKind?.Name ?? String.Empty}
				}
			};
		}
	}


	public enum DateFilterType
	{
		[Display(Name = "план. завершения")]
		PlannedCompletionDate,
		[Display(Name = "создания")]
		CreationDate
	}
}
