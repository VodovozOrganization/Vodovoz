using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Presentation.ViewModels.Common;

namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel
	{
		[Appellative(Nominative = "Отчет по количеству рекламаций на водителей")]
		public partial class NumberOfComplaintsAgainstDriversReport
		{
			private NumberOfComplaintsAgainstDriversReport(DateTime startDate, DateTime endDate, List<Row> driverRows, List<SubdivisionRow> subdivisionRows)
			{
				CreatedAt = DateTime.Now;
				StartDate = startDate;
				EndDate = endDate;
				DriverRows = driverRows;
				SubdivisionRows = subdivisionRows;
			}

			public DateTime	CreatedAt { get; }
			public DateTime StartDate { get; }
			public DateTime EndDate { get; }
			public List<Row> DriverRows { get; }
			public List<SubdivisionRow> SubdivisionRows { get; }

			public static NumberOfComplaintsAgainstDriversReport Generate(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate,
				int geoGroupId, int complaintResultId, ReportSortOrder sortOrder,
				IncludeExludeFiltersViewModel includeExcludeFilterViewModel)
			{
				var subdivisionsFilter = includeExcludeFilterViewModel.GetFilter<IncludeExcludeEntityFilter<Subdivision>>();
				var includedSubdivisions = subdivisionsFilter.GetIncluded().ToArray();
				var excludedSubdivisions = subdivisionsFilter.GetExcluded().ToArray();

				var complaints =
				(
					from complaint in unitOfWork.Session.Query<Complaint>()
					join complaintKind in unitOfWork.Session.Query<ComplaintKind>()
						on complaint.ComplaintKind.Id equals complaintKind.Id
					join driver in unitOfWork.Session.Query<Employee>()
						on complaint.Driver.Id equals driver.Id
					join order in unitOfWork.Session.Query<Order>()
						on complaint.Order.Id equals order.Id
					join guilty in unitOfWork.Session.Query<ComplaintGuiltyItem>()
						on complaint.Id equals guilty.Complaint.Id into guiltyLeft
					from guiltes in guiltyLeft.DefaultIfEmpty()
					join subdivision in unitOfWork.Session.Query<Subdivision>()
						on guiltes.Subdivision.Id equals subdivision.Id into subdivisionLeft
					from subdivisions in subdivisionLeft.DefaultIfEmpty()

					where complaint.CreationDate >= startDate
						&& complaint.CreationDate <= endDate
						&& complaint.ComplaintType != ComplaintType.Driver
						&& (geoGroupId == 0 || order.DeliveryPoint.District.GeographicGroup.Id == geoGroupId)
						&& (complaintResultId == 0
							|| complaint.ComplaintResultOfCounterparty.Id == complaintResultId
							|| complaint.ComplaintResultOfEmployees.Id == complaintResultId)
					
					let driverFullName = $"{driver.LastName} {driver.Name} {driver.Patronymic}"

					where (!includedSubdivisions.Any() || includedSubdivisions.Contains(guiltes.Subdivision.Id))
						&& (!excludedSubdivisions.Any() || !excludedSubdivisions.Contains(guiltes.Subdivision.Id))

					select new
					{
						ComplaintId = complaint.Id,
						DriverId = driver.Id,
						DriverFullName = driverFullName,
						ComplaintsKind = complaintKind.Name,
						Subdivision = subdivisions.ShortName
					}
				)
				.ToList()
				.OrderBy(x => x.DriverFullName);

				#region Driver

				var preGroupedByDriverComplaintsForCounting = complaints.GroupBy(x => new { x.DriverId, x.ComplaintId });
				var driverComplaintsList = preGroupedByDriverComplaintsForCounting.Select(x => x.First()).ToList();
				var groupedByDriverComplaints = driverComplaintsList.GroupBy(x => x.DriverId).ToList();

				var driverResult = new List<Row>();

				foreach(var group in groupedByDriverComplaints)
				{
					driverResult.Add(new Row
					{
						DriverFullName = group.First().DriverFullName,
						ComplaintsCount = group.Count(),
						ComplaintsList = string.Join(",\n", group.Select(x => $"{x.ComplaintId} - {x.ComplaintsKind}"))
					});
				}

				driverResult = sortOrder == ReportSortOrder.DriverName
					? driverResult.OrderBy(x => x.DriverFullName).ToList()
					: driverResult.OrderByDescending(x => x.ComplaintsCount).ToList();

				#endregion Driver

				#region Subdivision

				var grouppedBySubdivisionComplaints = complaints
					.Where(c => c.Subdivision != null)
					.GroupBy(x => x.Subdivision);

				var subdivisionResult = grouppedBySubdivisionComplaints.Select
					(
						group =>
							new SubdivisionRow
							{
								Subdivision = group.First().Subdivision,
								ComplaintsCount = group.Count()
							}
					)
					.OrderByDescending(x => x.ComplaintsCount).ToList();

				#endregion Subdivision

				return new NumberOfComplaintsAgainstDriversReport(startDate, endDate, driverResult, subdivisionResult);
			}
		}
	}
}
