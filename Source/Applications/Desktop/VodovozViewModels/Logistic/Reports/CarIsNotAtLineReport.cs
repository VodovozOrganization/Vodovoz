using DateTimeHelpers;
using MoreLinq;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Reports;

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
			string eventsSummary,
			string eventsSummaryDetails)
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
			EventsSummaryDetails = eventsSummaryDetails;
			CreatedAt = DateTime.Now;

			UiRows =
				UiRow.CreateUiRows(Rows, CarTransferRows, CarReceptionRows, EventsSummary, EventsSummaryDetails);
		}

		public string TemplatePath => @".\Reports\Logistic\CarIsNotAtLineReport.xlsx";

		#region Параметры отчета

		public DateTime CreatedAt { get; }

		public DateTime Date { get; }

		public int CountDays { get; }

		public IEnumerable<(int Id, string Title)> IncludedEvents { get; }

		public IEnumerable<(int Id, string Title)> ExcludedEvents { get; }

		public string EventsSummary { get; }

		public string EventsSummaryDetails { get; }

		#endregion Параметры отчета

		#region Строки отчета

		public IEnumerable<Row> Rows { get; set; }

		public IEnumerable<CarTransferRow> CarTransferRows { get; set; }

		public IEnumerable<CarReceptionRow> CarReceptionRows { get; set; }

		public IList<UiRow> UiRows { get; private set; }

		#endregion Строки отчета

		public static async Task<Result<CarIsNotAtLineReport>> Generate(
			IUnitOfWork unitOfWork,
			IGenericRepository<CarEvent> carEventRepository,
			DateTime date,
			int countDays,
			IEnumerable<(int Id, string Title)> includedEvents,
			IEnumerable<(int Id, string Title)> excludedEvents,
			IEnumerable<int> excludeCarsIds,
			int carTransferEventTypeId,
			int carReceptionEventTypeId,
			CancellationToken cancellationToken)
		{
			var startDate = date.AddDays(-countDays).Date;

			var cars = await (from car in unitOfWork.Session.Query<Car>()
							  let carOwnType =
								  (CarOwnType?)(from carVersion in unitOfWork.Session.Query<CarVersion>()
												where carVersion.Car.Id == car.Id
												  && carVersion.EndDate == null
												  && _carOwnTypes.Contains(carVersion.CarOwnType)
												select carVersion.CarOwnType)
								  .FirstOrDefault()
							  where !excludeCarsIds.Contains(car.Id)
								  && !car.IsArchive
								  && carOwnType != null
								  && !_excludeTypesOfUse.Contains(car.CarModel.CarTypeOfUse)
								  && car.IsUsedInDelivery
							  select car)
				.Fetch(c => c.Driver)
				.ToListAsync(cancellationToken);

			var carIds = cars
				.Select(c => c.Id)
				.ToArray();

			var routeListItemNodDeliveredStatuses = RouteListItem.GetNotDeliveredStatuses();

			var carsWithLastRouteLists =
				await (from car in unitOfWork.Session.Query<Car>()
					   let lastRouteListDate =
						  (DateTime?)(from routeList in unitOfWork.Session.Query<RouteList>()
									  join routeListItem in unitOfWork.Session.Query<RouteListItem>()
										  on routeList.Id equals routeListItem.RouteList.Id
									  where routeList.Car.Id == car.Id
										  && routeList.Date <= date.LatestDayTime()
										  && !routeListItemNodDeliveredStatuses.Contains(routeListItem.Status)
									  orderby routeList.Date descending
									  select routeList.Date)
						  .FirstOrDefault()
					   where carIds.Contains(car.Id)
					   select new
					   {
						   car,
						   lastRouteListDate
					   })
				.ToListAsync(cancellationToken);

			var carIdsWithoutRouteListsAfterStartDate = cars
				.Where(c => !carsWithLastRouteLists.Any(cwrl => cwrl.lastRouteListDate > startDate && cwrl.car.Id == c.Id))
				.Select(c => c.Id)
				.ToArray();

			var includedEventsIds = includedEvents.Select(iep => iep.Id).ToArray();
			var excludedEventsIds = excludedEvents.Select(iep => iep.Id).ToArray();

			var events = (await carEventRepository.GetAsync(
				unitOfWork,
				ce => ce.StartDate <= date.Date
					&& ce.EndDate >= date.Date
					&& (!includedEventsIds.Any() || includedEventsIds.Contains(ce.CarEventType.Id))
					&& (!excludedEventsIds.Any() || !excludedEventsIds.Contains(ce.CarEventType.Id)),
				cancellationToken: cancellationToken))
				.Value
				.ToArray();

			var notTransferRecieveEvents = events
				.Where(ce => ce.CarEventType.Id != carTransferEventTypeId
					&& ce.CarEventType.Id != carReceptionEventTypeId)
				.OrderByDescending(ce => ce.EndDate)
				.ThenBy(ce => ce.StartDate)
				.GroupBy(ce => ce.Car.Id)
				.ToArray();

			var filteredTransferEvents = (await carEventRepository.GetAsync(
				unitOfWork,
				e => carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == carTransferEventTypeId
					&& (!includedEventsIds.Any() || includedEventsIds.Contains(carTransferEventTypeId))
					&& (!excludedEventsIds.Any() || !excludedEventsIds.Contains(carTransferEventTypeId))
					&& e.CreateDate >= date.AddDays(-1).Date
					&& e.CreateDate <= date.LatestDayTime(),
				cancellationToken: cancellationToken))
				.Value
				.ToArray();

			var filteredRecieveEvents = (await carEventRepository.GetAsync(
				unitOfWork,
				e => carIds.Contains(e.Car.Id)
					&& e.CarEventType.Id == carReceptionEventTypeId
					&& (!includedEventsIds.Any() || includedEventsIds.Contains(carReceptionEventTypeId))
					&& (!excludedEventsIds.Any() || !excludedEventsIds.Contains(carReceptionEventTypeId))
					&& e.CreateDate >= date.AddDays(-1).Date
					&& e.CreateDate <= date.LatestDayTime(),
				cancellationToken: cancellationToken))
				.Value
				.ToArray();

			var rows = new List<Row>();
			var carTransferRows = new List<CarTransferRow>();
			var carReceptionRows = new List<CarReceptionRow>();

			var carsModels = events
				.Select(fe => fe.Car.CarModel.Id)
				.Distinct()
				.ToArray();

			var rowsHavingEvents = new List<Row>();
			var rowsWithoutEvents = new List<Row>();

			foreach(var car in cars)
			{
				var carEventGroup = notTransferRecieveEvents.FirstOrDefault(x => x.Key == car.Id);

				if(!carIdsWithoutRouteListsAfterStartDate.Contains(car.Id))
				{
					continue;
				}

				if(carEventGroup == null)
				{
					rowsWithoutEvents.Add(new Row
					{
						RegistationNumber = car.RegistrationNumber,
						DowntimeStartedAt = carsWithLastRouteLists.FirstOrDefault(cwlrl => cwlrl.car.Id == car.Id)?.lastRouteListDate?.AddDays(1),
						CarType = car.CarModel.Name,
						CarOwnType = GetCarOwnType(date, car),
						CarTypeWithGeographicalGroup =
							$"{car.CarModel.Name} {GetGeoGroupFromCar(car)}",
						TimeAndBreakdownReason = "Простой без водителя",
						AreasOfResponsibility = null,
						PlannedReturnToLineDate = null,
						PlannedReturnToLineDateAndReschedulingReason = ""
					});

					continue;
				}

				var areas = carEventGroup
					.Select(ce => ce.CarEventType.AreaOfResponsibility)
					.Distinct()
					.OrderBy(area => area)
					.ToList();

				rowsHavingEvents.Add(new Row
				{
					RegistationNumber = car.RegistrationNumber,
					DowntimeStartedAt = carsWithLastRouteLists.FirstOrDefault(cwlrl => cwlrl.car.Id == car.Id)?.lastRouteListDate?.AddDays(1),
					CarType = car.CarModel.Name,
					CarOwnType = GetCarOwnType(date, car),
					CarTypeWithGeographicalGroup =
						$"{car.CarModel.Name} {GetGeoGroupFromCar(car)}",
					CarEventTypes = string.Join("/", carEventGroup.Select(ce => ce.CarEventType.Name)),
					TimeAndBreakdownReason = string.Join(", ", carEventGroup.Select(ce => $"{ce.StartDate.ToString(_defaultDateTimeFormat)} {ce.CarEventType.Name}")),
					AreasOfResponsibility = areas,
					PlannedReturnToLineDate = carEventGroup.First().EndDate,
					PlannedReturnToLineDateAndReschedulingReason = string.Join(", ", carEventGroup.Select(ce => ce.Comment)),
				});
			}

			rows.AddRange(
				rowsHavingEvents
					.OrderBy(x => x.AreasOfResponsibilityShortNames)
					.ThenBy(x => x.CarEventTypes.First())
					.ThenBy(x => x.DowntimeStartedAt)
			);
			rows.AddRange(
				rowsWithoutEvents
					.OrderBy(x => x.AreasOfResponsibilityShortNames)
					.ThenBy(x => x.DowntimeStartedAt)
			);

			var counter = 1;
			rows.ForEach(x => x.Id = counter++);
			
			for(var i = 0; i < filteredTransferEvents.Length; i++)
			{
				carTransferRows.Add(new CarTransferRow
				{
					Id = i + 1,
					RegistationNumber = filteredTransferEvents[i].Car.RegistrationNumber,
					CarOwnType = GetCarOwnType(date, filteredTransferEvents[i].Car),
					CarTypeWithGeographicalGroup =
						$"{filteredTransferEvents[i].Car.CarModel.Name} {GetGeoGroupFromCarEvent(filteredTransferEvents[i])}",
					Comment = filteredTransferEvents[i].Comment,
					TransferedAt = filteredTransferEvents[i].CreateDate,
				});
			}

			for(var i = 0; i < filteredRecieveEvents.Length; i++)
			{
				carReceptionRows.Add(new CarReceptionRow
				{
					Id = i + 1,
					RegistationNumber = filteredRecieveEvents[i].Car.RegistrationNumber,
					CarOwnType = GetCarOwnType(date, filteredRecieveEvents[i].Car),
					CarTypeWithGeographicalGroup =
						$"{filteredRecieveEvents[i].Car.CarModel.Name} {GetGeoGroupFromCarEvent(filteredRecieveEvents[i])}",
					Comment = filteredRecieveEvents[i].Comment,
					RecievedAt = filteredRecieveEvents[i].CreateDate,
				});
			}

			var summaryByCarModel = rows
				.GroupBy(row => row.CarType)
				.Select(g => $"{g.Key}: {g.Count()}");

			var eventsSummary =
				$"Всего {rows.Count()} авто.\n" +
				string.Join("\n", summaryByCarModel);

			var summaryByArea = rows
				.GroupBy(row => row.AreasOfResponsibilityShortNames)
				.Select(areaGroup =>
					$"{(string.IsNullOrWhiteSpace(areaGroup.Key) ? "Без зоны ответственности" : areaGroup.Key)}\n" +
					string.Join("\n",
						areaGroup
							.GroupBy(row => row.CarEventTypes)
							.Select(eventGroup =>
								$"{(string.IsNullOrWhiteSpace(eventGroup.Key) ? "Простой без водителя" : eventGroup.Key)}\n" +
								string.Join("\n",
									eventGroup
										.GroupBy(row => row.CarType)
										.Select(carGroup => $"{carGroup.Key}: {carGroup.Count()}"))
							)
					) + "\n"
				);

			var eventsSummaryDetails =
				string.Join("\n", summaryByArea);

			return new CarIsNotAtLineReport(date, countDays, includedEvents, excludedEvents, rows, carTransferRows, carReceptionRows, eventsSummary, eventsSummaryDetails);
		}

		private static string GetCarOwnType(DateTime date, Car car)
		{
			var version = car.GetActiveCarVersionOnDate(date);

			if(version is null)
			{
				return string.Empty;
			}

			switch(version.CarOwnType)
			{
				case CarOwnType.Company:
					return "К";
				case CarOwnType.Driver:
					return "В";
				case CarOwnType.Raskat:
					return "Р";
				default:
					return string.Empty;
			}
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
