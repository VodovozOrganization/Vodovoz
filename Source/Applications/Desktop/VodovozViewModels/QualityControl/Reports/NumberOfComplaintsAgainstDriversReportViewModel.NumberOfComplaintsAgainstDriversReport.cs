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
		[Appellative(Nominative = "Jтчет по количеству рекламаций на водителей")]
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
								  group complaint by driver.Id into complaintsGroup
								  let driver = complaintsGroup.FirstOrDefault().Driver
								  let driverFullName = $"{driver.Name} {driver.LastName} {driver.Patronymic}"
								  let complaintsList = string.Join(",\n", complaintsGroup.SelectMany(x => $"{x.Id} - {x.ComplaintKind.Name}"))
								  select new Row
								  {
									  DriverFullName = driverFullName,
									  ComplaintsCount = complaintsGroup.Count(),
									  ComplaintsList = complaintsList
								  }).ToList();

				return new NumberOfComplaintsAgainstDriversReport(startDate, endDate, complaints);
			}
		}
	}
}
