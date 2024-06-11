using NHibernate.Linq;
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
using DateTimeHelpers;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	[Appellative(Nominative = "Отчёт по простою")]
	public partial class CarIsNotAtLineReport : IClosedXmlReport
	{
		private const string _defaultDateTimeFormat = "dd.MM.yyyy";

		private static readonly CarTypeOfUse[] _excludeTypesOfUse = new CarTypeOfUse[] { CarTypeOfUse.Truck, CarTypeOfUse.Loader };
		private static readonly CarOwnType[] _carOwnTypes = new CarOwnType[] { CarOwnType.Company, CarOwnType.Raskat };

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
			IGenericRepository<CarEvent> carEventRepository,
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents,
			IEnumerable<int> excludeCarsIds,
			int carTransferEventTypeId,
			int carReceptionEventTypeId)
		{
			var startDate = date.AddDays(-countDays);

			var cars = (from car in unitOfWork.Session.Query<Car>()
						let carOwnType =
							(CarOwnType?)(from carVersion in unitOfWork.Session.Query<CarVersion>()
										  where carVersion.Car.Id == car.Id
											&& carVersion.EndDate == null
											&& _carOwnTypes.Contains( carVersion.CarOwnType)
										  select carVersion.CarOwnType)
							.FirstOrDefault()
						where !excludeCarsIds.Contains(car.Id)
							&& !car.IsArchive
							&& carOwnType != null
							&& !_excludeTypesOfUse.Contains(car.CarModel.CarTypeOfUse)
						select car)
				.Fetch(c => c.Driver)
				.ToList();

			var carIds = cars
				.Select(c => c.Id)
				.ToArray();

			var carsWithLastRouteLists =
				(from car in unitOfWork.Session.Query<Car>()
				 let lastRouteListDate =
					(DateTime?)(from routeList in unitOfWork.Session.Query<RouteList>()
								where routeList.Car.Id == car.Id
								orderby routeList.Date descending
								select routeList.Date)
					.FirstOrDefault()
				 where carIds.Contains(car.Id)
				 select new
				 {
					 car,
					 lastRouteListDate
				 })
				.ToArray();

			foreach(var carsWithRouteList in carsWithLastRouteLists)
			{
				if(carsWithRouteList.lastRouteListDate > startDate)
				{
					cars.Remove(cars.FirstOrDefault(c => c.Id == carsWithRouteList.car.Id));
				}
			}

			var carIdsWithoutRouteListsAfterStartDate = cars
				.Select(c => c.Id)
				.ToArray();

			var includedEventsIds = includedEvents.Select(iep => iep.Id).ToArray();
			var excludedEventsIds = excludedEvents.Select(iep => iep.Id).ToArray();

			var events = carEventRepository.Get(
				unitOfWork,
				ce => ce.StartDate <= date
					&& ce.EndDate >= date
					&& (!includedEventsIds.Any() || includedEventsIds.Contains(ce.CarEventType.Id))
					&& (!excludedEventsIds.Any() || !excludedEventsIds.Contains(ce.CarEventType.Id)));

			var filteredEvents = events.Where(ce => carIdsWithoutRouteListsAfterStartDate.Contains(ce.Car.Id));

			var notTransferRecieveEvents = filteredEvents
				.Where(ce => ce.CarEventType.Id != carTransferEventTypeId
					&& ce.CarEventType.Id != carReceptionEventTypeId)
				.OrderByDescending(ce => ce.EndDate)
				.ThenBy(ce => ce.StartDate)
				.GroupBy(ce => ce.Car.Id)
				.ToArray();

			var filteredTransferEvents = events
				.Where(e =>
					carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == carTransferEventTypeId
					&& e.CreateDate >= startDate.AddDays(-1)
					&& e.CreateDate <= startDate.LatestDayTime())
				.ToArray();

			var filteredRecieveEvents = events
				.Where(e =>
					carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == carReceptionEventTypeId
					&& e.CreateDate >= startDate.AddDays(-1)
					&& e.CreateDate <= startDate.LatestDayTime())
				.ToArray();

			var rows = new List<Row>();
			var carTransferRows = new List<CarTransferRow>();
			var carReceptionRows = new List<CarReceptionRow>();

			var carsModels = events
				.Select(fe => fe.Car.CarModel.Id)
				.Distinct()
				.ToArray();

			var eventsGrouppedByCarModel = events.GroupBy(fe => fe.Car.CarModel.Id);

			for(var i = 0; i < cars.Count; i++)
			{
				var carEventGroup = notTransferRecieveEvents.FirstOrDefault(x => x.Key == cars[i].Id);

				if(carEventGroup == null)
				{
					rows.Add(new Row
					{
						Id = i + 1,
						RegistationNumber = cars[i].RegistrationNumber,
						DowntimeStartedAt = carsWithLastRouteLists.FirstOrDefault(cwlrl => cwlrl.car.Id == cars[i].Id)?.lastRouteListDate?.AddDays(1),
						CarType = cars[i].CarModel.Name,
						CarTypeWithGeographicalGroup =
							$"{cars[i].CarModel.Name} {GetGeoGroupFromCar(cars[i])}",
						TimeAndBreakdownReason = "Простой",
						PlannedReturnToLineDate = null,
						PlannedReturnToLineDateAndReschedulingReason = "",
					});

					continue;
				}

				rows.Add(new Row
				{
					Id = i + 1,
					RegistationNumber = cars[i].RegistrationNumber,
					DowntimeStartedAt = carsWithLastRouteLists.FirstOrDefault(cwlrl => cwlrl.car.Id == cars[i].Id)?.lastRouteListDate?.AddDays(1),
					CarType = cars[i].CarModel.Name,
					CarTypeWithGeographicalGroup =
						$"{cars[i].CarModel.Name} {GetGeoGroupFromCar(cars[i])}",
					TimeAndBreakdownReason = string.Join(", ", carEventGroup.Select(ce => $"{ce.StartDate.ToString(_defaultDateTimeFormat)} {ce.CarEventType.Name}")),
					PlannedReturnToLineDate = carEventGroup.First().EndDate,
					PlannedReturnToLineDateAndReschedulingReason = string.Join(", ", carEventGroup.Select(ce => ce.Comment)),
				});
			}

			if((!includedEvents.Any() || includedEvents.Any(pair => pair.Id == carTransferEventTypeId))
				&& (!excludedEvents.Any() || !excludedEvents.Any(pair => pair.Id == carTransferEventTypeId)))
			{
				for(var i = 0; i < filteredTransferEvents.Length; i++)
				{
					carTransferRows.Add(new CarTransferRow
					{
						Id = i + 1,
						RegistationNumber = filteredTransferEvents[i].Car.RegistrationNumber,
						CarTypeWithGeographicalGroup =
							$"{filteredTransferEvents[i].Car.CarModel.Name} {GetGeoGroupFromCarEvent(filteredTransferEvents[i])}",
						Comment = filteredTransferEvents[i].Comment,
						TransferedAt = filteredTransferEvents[i].CreateDate,
					});
				}
			}

			if((!includedEvents.Any() || includedEvents.Any(pair => pair.Id == carReceptionEventTypeId))
				&& (!excludedEvents.Any() || !excludedEvents.Any(pair => pair.Id == carReceptionEventTypeId)))
			{
				for(var i = 0; i < filteredRecieveEvents.Length; i++)
				{
					carReceptionRows.Add(new CarReceptionRow
					{
						Id = i + 1,
						RegistationNumber = filteredRecieveEvents[i].Car.RegistrationNumber,
						CarTypeWithGeographicalGroup =
							$"{filteredRecieveEvents[i].Car.CarModel.Name} {GetGeoGroupFromCarEvent(filteredRecieveEvents[i])}",
						Comment = filteredRecieveEvents[i].Comment,
						RecievedAt = filteredRecieveEvents[i].CreateDate,
					});
				}
			}

			var summaryByCarModel = rows
				.GroupBy(row => row.CarType)
				.Select(g => $"{g.Key}: {g.Count()}");

			var eventsSummary =
				$"Всего {rows.Count()} авто.\n" +
				string.Join("\n", summaryByCarModel);

			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents, rows, carTransferRows, carReceptionRows, eventsSummary);
		}

		private static string GetGeoGroupFromCarEvent(CarEvent carEvent) =>
			GetGeoGroupFromCar(carEvent.Car);

		private static string GetGeoGroupFromCar(Car car)
		{
			var geoGroupName = car.Driver?.Subdivision?.GeographicGroup?.Name;

			if(string.IsNullOrWhiteSpace(geoGroupName))
			{
				return string.Empty;
			}

			return $"({geoGroupName})";
		}
	}
}
