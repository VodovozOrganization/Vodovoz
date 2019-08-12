using System;
using System.Collections.Generic;
using QS.Project.Filter;
using QS.Report;
using QS.Services;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.FilterViewModels
{
	public class ComplaintFilterViewModel : FilterViewModelBase<ComplaintFilterViewModel>
	{
		public ComplaintFilterViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
			UpdateWith(
				x => x.ComplaintType,
				x => x.ComplaintStatus,
				x => x.Employee,
				x => x.StartDate,
				x => x.EndDate,
				x => x.Subdivision
			);
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
			set => SetField(ref subdivision, value, () => Subdivision);
		}

		private DateTime startDate = DateTime.Now;
		public virtual DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime endDate = DateTime.Now;
		public virtual DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}


		public ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Title = "Жалобы",
				Identifier = "Complaints",
				Parameters = new Dictionary<string, object>
				{
					{ "subdivision_id", Subdivision?.Id ?? 0},
					{ "start_date", StartDate},
					{ "end_date", EndDate},
					{ "employee_id", Employee?.Id ?? 0},
					{ "type", ComplaintType?.ToString() ?? String.Empty},
					{ "status", ComplaintStatus?.ToString() ?? String.Empty}
				}
			};
		}
	}

}
