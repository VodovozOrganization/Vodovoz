using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Errors;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	[Appellative(Nominative = "Отчёт по простою")]
	public partial class CarIsNotAtLineReport : IClosedXmlReport
	{
		private const string _defaultDateTimeFormat = "dd.MM.yyyy";

		private CarIsNotAtLineReport(
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents,
			IEnumerable<Row> rows,
			IEnumerable<CarTransferRow> carTransferRows,
			IEnumerable<CarReceptionRow> carReceptionRows)
		{
			Date = date;
			CountDays = countDays;
			IncludedEvents = includedEvents
				?? throw new ArgumentNullException(nameof(includedEvents));
			ExcludedEvents = excludedEvents
				?? throw new ArgumentNullException(nameof(excludedEvents));
			Rows = rows
				?? throw new ArgumentNullException(nameof(rows));
			CarTransferRows = carTransferRows
				?? throw new ArgumentNullException(nameof(carTransferRows));
			CarReceptionRows = carReceptionRows
				?? throw new ArgumentNullException(nameof(carReceptionRows));
		}

		public string TemplatePath => @".\Reports\Logistic\CarIsNotAtLineReport.xlsx";

		#region Параметры отчета

		public DateTime CreatedAt { get; }

		public DateTime Date { get; }

		public int CountDays { get; }

		public IEnumerable<(int Id, string Title)> IncludedEvents { get; }

		public IEnumerable<(int Id, string Title)> ExcludedEvents { get; }

		#endregion Параметры отчета

		#region Строки отчета

		public IEnumerable<Row> Rows { get; set; }

		public IEnumerable<CarTransferRow> CarTransferRows { get; set; }

		public IEnumerable<CarReceptionRow> CarReceptionRows { get; set; }

		#endregion Строки отчета

		public static Result<CarIsNotAtLineReport> Generate(
			IUnitOfWork unitOfWork,
			IGenericRepository<RouteList> routeListRepository,
			IGenericRepository<CarEvent> carEventRepository,
			IGenericRepository<Car> carRepository,
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents)
		{
			var carsHasRouteListInDays = routeListRepository
				.GetValue(
					unitOfWork,
					rl => rl.Car.Id,
					rl => rl.Date >= date.Add(TimeSpan.FromDays(-countDays)))
				.Distinct()
				.ToArray();

			var carsWithoutRouteLists = carRepository
				.GetValue(unitOfWork, c => c, c => !carsHasRouteListInDays.Contains(c.Id));

			// Fake Data to test!!

			var rows = new List<Row>();

			foreach(var car in carsWithoutRouteLists)
			{
				rows.Add(new Row
				{
					Id = 1,
					RegistationNumber = car.RegistrationNumber,
					DowntimeStartedAt = DateTime.Now,
					CarTypeWithGeographicalGroup = "Test South",
					PlannedReturnToLineDate = DateTime.Now.AddMonths(1),
					PlannedReturnToLineDateAndReschedulingReason = "Test Resheduling reason",
					TimeAndBreakdownReason = "Test Reason"
				});
			}

			var carTransferRows = new List<CarTransferRow>();

			carTransferRows.Add(new CarTransferRow
			{
				Id = 1,
				CarTypeWithGeographicalGroup = "Test North",
				Comment = "Test Comment",
				RegistationNumber = "dasda24ad",
				TransferedAt = DateTime.Now
			});

			var carReceptionRows = new List<CarReceptionRow>();

			carReceptionRows.Add(new CarReceptionRow
			{
				Id = 1,
				CarTypeWithGeographicalGroup = "Test North",
				Comment = "Test Comment",
				RegistationNumber = "dasda24ad",
				RecievedAt = DateTime.Now
			});

			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents, rows, carTransferRows, carReceptionRows);
		}
	}
}
