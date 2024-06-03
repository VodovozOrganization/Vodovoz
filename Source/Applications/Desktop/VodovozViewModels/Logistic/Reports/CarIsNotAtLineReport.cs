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

		private const int _logisticsSubdivisionSefiyskaya = 13;
		private const int _logisticsSubdivisionBugri = 51;
		
		private const int _logisticsEventTransferId = 11;
		private const int _logisticsEventRecieveId = 40;

		private const string _northGeoGroupTitle = "север";
		private const string _southGeoGroupTitle = "ЮГ";

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

			var notTransferRecieveEvents = filteredEvents
				.Where(ce => ce.CarEventType.Id != _logisticsEventTransferId
					&& ce.CarEventType.Id != _logisticsEventRecieveId)
				.ToArray();

			var filteredTransferEvents = events
				.Where(e => carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == _logisticsEventTransferId)
				.ToArray();

			var filteredRecieveEvents = events
				.Where(e => carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == _logisticsEventRecieveId)
				.ToArray();

			if(!filteredEvents.Any())
			{
				return Result.Failure<CarIsNotAtLineReport>(new Error("DataNotFound", "Нет данных для отчета"));
			}

			var rows = new List<Row>();
			var carTransferRows = new List<CarTransferRow>();
			var carReceptionRows = new List<CarReceptionRow>();

			var carsModels = filteredEvents
				.Select(fe => fe.Car.CarModel.Id)
				.Distinct()
				.ToArray();

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

			for(var i = 0; i < notTransferRecieveEvents.Length; i++)
			{
				rows.Add(new Row
				{
					Id = i + 1,
					RegistationNumber = notTransferRecieveEvents[i].Car.RegistrationNumber,
					DowntimeStartedAt = carsWithLastRouteLists.First(cwlrl => cwlrl.car.Id == notTransferRecieveEvents[i].Car.Id).lastRouteListDate.Value,
					CarTypeWithGeographicalGroup =
						notTransferRecieveEvents[i].Car.CarModel.Name
						+ " " +
						GetGeoGroupFromCarEvent(notTransferRecieveEvents, i),
					TimeAndBreakdownReason = notTransferRecieveEvents[i].Comment,
					PlannedReturnToLineDate = notTransferRecieveEvents[i].EndDate,
					PlannedReturnToLineDateAndReschedulingReason = "Test Resheduling reason",
				});
			}

			for(var i = 0; i < filteredTransferEvents.Length; i++)
			{
				carTransferRows.Add(new CarTransferRow
				{
					Id = 1,
					RegistationNumber = filteredTransferEvents[i].Car.RegistrationNumber,
					CarTypeWithGeographicalGroup = filteredTransferEvents[i].Car.CarModel.Name
						+ " " +
						GetGeoGroupFromCarEvent(filteredTransferEvents, i),
					Comment = filteredTransferEvents[i].Comment,
					TransferedAt = filteredTransferEvents[i].EndDate,
				});
			}

			for(var i = 0; i < filteredRecieveEvents.Length; i++)
			{
				carReceptionRows.Add(new CarReceptionRow
				{
					Id = 1,
					RegistationNumber = filteredRecieveEvents[i].Car.RegistrationNumber,
					CarTypeWithGeographicalGroup = filteredRecieveEvents[i].Car.CarModel.Name
						+ " " +
						GetGeoGroupFromCarEvent(filteredRecieveEvents, i),
					Comment = filteredRecieveEvents[i].Comment,
					RecievedAt = filteredRecieveEvents[i].EndDate,
				});
			}

			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents, rows, carTransferRows, carReceptionRows, eventsSummary);
		}

		private static string GetGeoGroupFromCarEvent(CarEvent[] notTransferRecieveEvents, int i)
			=> notTransferRecieveEvents[i].Car.Driver.Subdivision.Id == _logisticsSubdivisionSefiyskaya
			? _southGeoGroupTitle
			: notTransferRecieveEvents[i].Car.Driver.Subdivision.Id == _logisticsSubdivisionBugri
				? _northGeoGroupTitle
				: "";
	}
}
