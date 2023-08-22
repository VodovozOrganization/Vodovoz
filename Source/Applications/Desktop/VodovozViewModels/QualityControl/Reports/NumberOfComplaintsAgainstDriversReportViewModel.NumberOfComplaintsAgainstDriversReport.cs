using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel
	{
		[Appellative(Nominative = "Отчет по количеству рекламаций на водителей")]
		public partial class NumberOfComplaintsAgainstDriversReport
		{
			private NumberOfComplaintsAgainstDriversReport(DateTime startDate, DateTime endDate, List<Row> rows)
			{
				CreatedAt = DateTime.Now;
				StartDate = startDate;
				EndDate = endDate;
				Rows = rows;
			}

			public DateTime	CreatedAt { get; }
			public DateTime StartDate { get; }
			public DateTime EndDate { get; }
			public List<Row> Rows { get; }

			public static NumberOfComplaintsAgainstDriversReport Generate(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate)
			{
				var complaints = (from complaint in unitOfWork.Session.Query<Complaint>()
								  join complaintKind in unitOfWork.Session.Query<ComplaintKind>()
								  on complaint.ComplaintKind.Id equals complaintKind.Id
								  join driver in unitOfWork.Session.Query<Employee>()
								  on complaint.Driver.Id equals driver.Id
								  where complaint.CreationDate >= startDate
									&& complaint.CreationDate <= endDate
									&& complaint.ComplaintType != ComplaintType.Driver
								  let driverFullName = $"{driver.LastName} {driver.Name} {driver.Patronymic}"
								  select new
								  {
									  ComplaintId = complaint.Id,
									  DriverId = driver.Id,
									  DriverFullName = driverFullName,
									  ComplaintsKind = complaintKind.Name
								  }).ToList();

				var groupedComplaints = complaints.GroupBy(x => x.DriverId);

				var result = new List<Row>();

				foreach(var group in groupedComplaints)
				{
					result.Add(new Row
					{
						DriverFullName = group.First().DriverFullName,
						ComplaintsCount = group.Count(),
						ComplaintsList = string.Join(",\n", group.Select(x => $"{x.ComplaintId} - {x.ComplaintsKind}"))
					});
				}

				return new NumberOfComplaintsAgainstDriversReport(startDate, endDate, result);
			}
		}
	}
}
