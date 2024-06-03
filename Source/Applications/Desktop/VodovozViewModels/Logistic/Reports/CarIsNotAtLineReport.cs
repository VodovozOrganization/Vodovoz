using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
			IEnumerable<CarReceptionRow> carReceptionRows,
			string eventsSummary)
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
			EventsSummary = eventsSummary
				?? throw new ArgumentNullException(nameof(eventsSummary));

			CreatedAt = DateTime.Now;
		}

		public string TemplatePath => @".\Reports\Logistic\CarIsNotAtLineReport.xlsx";

		#region Параметры отчета

		public DateTime CreatedAt { get; }

		public DateTime Date { get; }

		public int CountDays { get; }

		public IEnumerable<(int Id, string Title)> IncludedEvents { get; }

		public IEnumerable<(int Id, string Title)> ExcludedEvents { get; }

		public string EventsSummary { get; }

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
			var startDate = date.AddDays(-countDays);

			var includedEventsIds = includedEvents.Select(iep => iep.Id).ToArray();
			var excludedEventsIds = excludedEvents.Select(iep => iep.Id).ToArray();

			var events = carEventRepository.Get(
				unitOfWork,
				ce => ce.StartDate >= date
					&& (!includedEventsIds.Any() || includedEventsIds.Contains(ce.CarEventType.Id))
					&& (!excludedEventsIds.Any() || !excludedEventsIds.Contains(ce.CarEventType.Id)));

			if(!events.Any())
			{
				return Result.Failure<CarIsNotAtLineReport>(new Error("DataNotFound", "Нет данных для отчета"));
			}

			var carIds = events.Select(e => e.Car.Id).ToArray();

			var carsWithLastRouteLists =
				(from car in unitOfWork.Session.Query<Car>()
				let lastRouteListDate =
					(DateTime?)(from routeList in unitOfWork.Session.Query<RouteList>()
								where routeList.Car.Id == car.Id
								orderby routeList.Date descending
								select routeList.Date)
					.FirstOrDefault()
				where carIds.Contains(car.Id)
					&& lastRouteListDate <= startDate
				select new 
				{
					car,
					lastRouteListDate
				})
				.ToArray();

			carIds = carsWithLastRouteLists
				.Select(cewrl => cewrl.car.Id)
				.ToArray();

			var filteredEvents = events
				.Where(e => carIds.Contains(e.Car.Id))
				.ToArray();

			if(!filteredEvents.Any())
			{
				return Result.Failure<CarIsNotAtLineReport>(new Error("DataNotFound", "Нет данных для отчета"));
			}

			var rows = new List<Row>();
			var carTransferRows = new List<CarTransferRow>();
			var carReceptionRows = new List<CarReceptionRow>();

			var carsModels = filteredEvents.Select(fe => fe.Car.CarModel.Id).Distinct().ToArray();

			var eventsGrouppedByCarModel = filteredEvents.GroupBy(fe => fe.Car.CarModel.Id);

			var summaryByCarModel = eventsGrouppedByCarModel.Select(g =>
				$"{g.Count()} {g.FirstOrDefault().Car.CarModel.Name}");

			var eventsSummary =
				"Всего авто " +
				filteredEvents
					.Select(ce => ce.Car.Id)
					.Distinct()
					.Count() +
				", из них " +
				string.Join(", ", summaryByCarModel);

			for(var i = 0; i < filteredEvents.Length; i++)
			{
				rows.Add(new Row
				{
					Id = i + 1,
					RegistationNumber = filteredEvents[i].Car.RegistrationNumber,
					DowntimeStartedAt = carsWithLastRouteLists.First(cwlrl => cwlrl.car.Id == filteredEvents[i].Car.Id).lastRouteListDate.Value,
					CarTypeWithGeographicalGroup =
						filteredEvents[i].Car.CarModel.Name
						+ " " +
						(filteredEvents[i].Car.Driver.Subdivision.Id == 13 ? "ЮГ" :
						filteredEvents[i].Car.Driver.Subdivision.Id == 51 ? "север" : ""),
					TimeAndBreakdownReason = filteredEvents[i].Comment,
					PlannedReturnToLineDate = filteredEvents[i].EndDate,
					PlannedReturnToLineDateAndReschedulingReason = "Test Resheduling reason",
				});
			}

			carTransferRows.Add(new CarTransferRow
			{
				Id = 1,
				CarTypeWithGeographicalGroup = "Test North",
				Comment = "Test Comment",
				RegistationNumber = "dasda24ad",
				TransferedAt = DateTime.Now
			});

			carReceptionRows.Add(new CarReceptionRow
			{
				Id = 1,
				CarTypeWithGeographicalGroup = "Test North",
				Comment = "Test Comment",
				RegistationNumber = "dasda24ad",
				RecievedAt = DateTime.Now
			});

			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents, rows, carTransferRows, carReceptionRows, eventsSummary);
		}
	}
}
