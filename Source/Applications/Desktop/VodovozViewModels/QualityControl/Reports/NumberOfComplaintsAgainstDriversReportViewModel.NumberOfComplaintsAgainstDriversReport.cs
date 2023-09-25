using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Common;
using Order = Vodovoz.Domain.Orders.Order;

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

			public static NumberOfComplaintsAgainstDriversReport Generate(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate,
				int geoGroupId, int complaintResultId, ReportSortOrder sortOrder,
				IncludeExludeFiltersViewModel includeExcludeFilterViewModel)
			{
				var subdivisionsFilter = includeExcludeFilterViewModel.GetFilter<IncludeExcludeEntityFilter<Subdivision>>();
				var includedSubdivisions = subdivisionsFilter.GetIncluded().ToArray();
				var excludedSubdivisions = subdivisionsFilter.GetExcluded().ToArray();

				var complaints = (from complaint in unitOfWork.Session.Query<Complaint>()
						join complaintKind in unitOfWork.Session.Query<ComplaintKind>()
							on complaint.ComplaintKind.Id equals complaintKind.Id
						join driver in unitOfWork.Session.Query<Employee>()
							on complaint.Driver.Id equals driver.Id
						join order in unitOfWork.Session.Query<Order>()
							on complaint.Order.Id equals order.Id
						where complaint.CreationDate >= startDate
						      && complaint.CreationDate <= endDate
						      && complaint.ComplaintType != ComplaintType.Driver
						      && (geoGroupId == 0 || order.DeliveryPoint.District.GeographicGroup.Id == geoGroupId)
						      && (complaintResultId == 0
						          || complaint.ComplaintResultOfCounterparty.Id == complaintResultId
						          || complaint.ComplaintResultOfEmployees.Id == complaintResultId)
						let driverFullName = $"{driver.LastName} {driver.Name} {driver.Patronymic}"

						let guiltyRestriction = (from guilty in unitOfWork.Session.Query<ComplaintGuiltyItem>() 
							where guilty.Complaint.Id == complaint.Id 
							      &&  (!includedSubdivisions.Any() || includedSubdivisions.Contains(guilty.Subdivision.Id)) 
							      &&  (!excludedSubdivisions.Any() || !excludedSubdivisions.Contains(guilty.Subdivision.Id)) 
							select guilty.Id)
							.FirstOrDefault() 
						
						where guiltyRestriction != null

						select new
						{
							ComplaintId = complaint.Id,
							DriverId = driver.Id,
							DriverFullName = driverFullName,
							ComplaintsKind = complaintKind.Name
						})
					.ToList()
					.OrderBy(x => x.DriverFullName);

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

				result = sortOrder == ReportSortOrder.DriverName
					? result.OrderBy(x => x.DriverFullName).ToList()
					: result.OrderByDescending(x => x.ComplaintsCount).ToList();

				return new NumberOfComplaintsAgainstDriversReport(startDate, endDate, result);
			}
		}
	}
}
