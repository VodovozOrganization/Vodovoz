using ClosedXML.Excel;
using QS.DomainModel.UoW;
using QS.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services;
using VodovozBusiness.EntityRepositories.Logistic;
using VodovozBusiness.Nodes;
using Schedule = VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule;

namespace Vodovoz.ViewModels.Services.DriverSchedule
{
	public class DriverScheduleService : IDriverScheduleService
	{
		private const int _firstDayColumn = 14;
		private const int _columnsPerDay = 5;
		private const int _daysInWeek = 7;
		private const int _commentColumn = _firstDayColumn + _daysInWeek * _columnsPerDay;
		private const string _driverScheduleCarEventFoundation = "Создано из графика водителей";
		private static readonly HashSet<string> _carEventTypeNamesAllowedToCreateFromDriverSchedule =
			CreateCarEventTypeNamesSet("вод/тел", "отпуск", "больничный", "выходной");
		private static readonly HashSet<string> _carEventTypeNamesAllowedToShowInDriverSchedule = CreateCarEventTypeNamesSet(
			"вод/тел",
			"отпуск",
			"больничный",
			"выходной",
			"ремонт",
			"ДТП",
			"ТО",
			"куз. ремонт",
			"куз ремонт",
			"кузовной ремонт",
			"КР",
			"М - мойка",
			"М",
			"мойка");
		private static readonly HashSet<string> _carEventTypeNamesNotClearingPotentialInDriverSchedule =
			CreateCarEventTypeNamesSet("М - мойка", "М", "мойка");

		private enum ExportColumn
		{
			CarTypeOfUse = 1,
			CarOwnType = 2,
			RegNumber = 3,
			DriverFullName = 4,
			DriverCarOwnType = 5,
			Phone = 6,
			District = 7,
			ArrivalTime = 8,
			MorningAddressesPotential = 9,
			MorningBottlesPotential = 10,
			EveningAddressesPotential = 11,
			EveningBottlesPotential = 12,
			LastModifiedDateTime = 13
		}

		private readonly ILogisticRepository _logisticRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICarRepository _carRepository;
		private readonly IEmployeeService _employeeService;

		public DriverScheduleService(
			ILogisticRepository logisticRepository,
			IRouteListRepository routeListRepository,
			ICarRepository carRepository,
			IEmployeeService employeeService
			)
		{
			_logisticRepository = logisticRepository ?? throw new ArgumentNullException(nameof(logisticRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
		}

		public IEnumerable<DriverScheduleRow> LoadScheduleData(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			int[] selectedSubdivisionIds,
			CarOwnType[] selectedCarOwnTypes,
			CarTypeOfUse[] selectedCarTypeOfUse,
			bool canEditAfter13,
			List<CarEventType> availableCarEventTypes)
		{
			var driverRows = _logisticRepository.GetDriverScheduleRows(
				uow,
				selectedSubdivisionIds,
				startDate,
				endDate,
				selectedCarOwnTypes,
				selectedCarTypeOfUse);

			var filteredResult = FilterByDismissalDate(driverRows, startDate);
			var driverIds = filteredResult.Select(r => r.DriverId).ToArray();

			var driversActiveRouteListDates = _routeListRepository.GetDriverIdsWithActiveRouteListByDates(
				uow, driverIds, startDate, endDate);

			var carEvents = _logisticRepository.GetCarEventsByDriverIds(uow, driverIds, startDate, endDate);
			var scheduleItems = _logisticRepository.GetDriverScheduleItemsByDriverIds(uow, driverIds, startDate, endDate);

			foreach(var node in filteredResult)
			{
				ProcessDriverNode(
					node,
					startDate,
					endDate,
					canEditAfter13,
					availableCarEventTypes,
					driversActiveRouteListDates,
					carEvents,
					scheduleItems);

				node.HasChanges = false;
			}

			var resultWithTotals = AddTotalRows(filteredResult, startDate);

			return resultWithTotals;
		}

		private void ProcessDriverNode(
			DriverScheduleRow node,
			DateTime startDate,
			DateTime endDate,
			bool canEditAfter13,
			List<CarEventType> availableCarEventTypes,
			Dictionary<int, HashSet<DateTime>> driversActiveRouteListDates,
			IList<CarEvent> carEvents,
			IList<DriverScheduleItem> scheduleItems)
		{
			node.StartDate = startDate;
			node.CanEditAfter13 = canEditAfter13;

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var dayDate = startDate.AddDays(dayIndex);
				var dayNode = node.Days[dayIndex];

				if(dayNode.Date == default)
				{
					dayNode.Date = dayDate;
				}

				dayNode.ParentRow = node;
				dayNode.HasActiveRouteList = driversActiveRouteListDates.ContainsKey(node.DriverId) &&
										   driversActiveRouteListDates[node.DriverId].Contains(dayDate);
			}

			node.InitializeEmptyCarEventTypes();
			node.IsCarAssigned = !string.IsNullOrEmpty(node.RegNumber);

			var dismissalDate = node.GetDismissalDate();
			if(dismissalDate.HasValue && dismissalDate.Value.Date >= startDate && dismissalDate.Value.Date <= endDate)
			{
				ProcessFiredDriverDays(node, dismissalDate.Value, availableCarEventTypes);
			}

			ApplyCarEventsToNode(node, carEvents, startDate);
			ApplyScheduleItemsToNode(node, scheduleItems, startDate);
		}

		private void ProcessFiredDriverDays(
			DriverScheduleRow driverNode,
			DateTime dismissalDate,
			List<CarEventType> availableCarEventTypes)
		{
			var eventType = GetDismissalEventType(driverNode, availableCarEventTypes);
			if(eventType == null)
			{
				return;
			}

			int dismissalDayIndex = (int)(dismissalDate - driverNode.StartDate).TotalDays;

			for(int dayIndex = dismissalDayIndex; dayIndex < 7; dayIndex++)
			{
				if(dayIndex >= 0)
				{
					var day = driverNode.Days[dayIndex];
					day.CarEventType = eventType;
					day.MorningAddresses = 0;
					day.MorningBottles = 0;
					day.EveningAddresses = 0;
					day.EveningBottles = 0;
					day.IsVirtualCarEventType = true;
				}
			}
		}

		private CarEventType GetDismissalEventType(
			DriverScheduleRow driverNode,
			List<CarEventType> availableCarEventTypes)
		{
			string eventName = driverNode.DateFired.HasValue && driverNode.DateCalculated.HasValue
				? (driverNode.DateFired.Value <= driverNode.DateCalculated.Value ? "Уволен" : "На расчете")
				: driverNode.DateFired.HasValue
					? "Уволен"
					: driverNode.DateCalculated.HasValue
						? "На расчете"
						: null;

			return !string.IsNullOrEmpty(eventName)
				? availableCarEventTypes.FirstOrDefault(x => x.ShortName == eventName) ??
				   new CarEventType { Id = -1, ShortName = eventName, Name = eventName }
				: null;
		}

		private void ApplyCarEventsToNode(
			DriverScheduleRow node,
			IList<CarEvent> carEvents,
			DateTime startDate)
		{
			var driverCarEvents = carEvents
				.Where(ce => ce.Driver?.Id == node.DriverId)
				.Where(ce => IsAllowedToShowInDriverSchedule(ce.CarEventType))
				.ToList();

			var eventWithComment = driverCarEvents.FirstOrDefault(carEvent => !string.IsNullOrEmpty(carEvent.Comment));

			if(eventWithComment != null)
			{
				node.OriginalComment = eventWithComment.Comment;
			}

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var dayDate = startDate.AddDays(dayIndex);
				var applicableEvent = driverCarEvents
					.Where(ce => ce.StartDate.Date <= dayDate && ce.EndDate.Date >= dayDate)
					.OrderByDescending(ce => IsAllowedToCreateFromDriverSchedule(ce.CarEventType))
					.ThenByDescending(ce => ce.CarEventType?.AreaOfResponsibility == AreaOfResponsibility.LogisticDepartment)
					.ThenByDescending(ce => ce.StartDate)
					.FirstOrDefault();

				if(applicableEvent != null && !node.Days[dayIndex].IsVirtualCarEventType)
				{
					node.Days[dayIndex].CarEventType = applicableEvent.CarEventType;
					node.Days[dayIndex].IsCarEventTypeFromJournal = true;

					if(ShouldClearDayPotential(applicableEvent.CarEventType))
					{
						ClearDayPotential(node.Days[dayIndex]);
					}
				}
			}
		}

		private void ApplyScheduleItemsToNode(
			DriverScheduleRow node,
			IList<DriverScheduleItem> scheduleItems,
			DateTime startDate)
		{
			var driverScheduleItems = scheduleItems
				.Where(si => si.DriverSchedule?.Driver?.Id == node.DriverId)
				.ToList();

			foreach(var item in driverScheduleItems)
			{
				int dayIndex = (int)(item.Date - startDate).TotalDays;
				if(dayIndex >= 0 && dayIndex < 7)
				{
					var dayNode = node.Days[dayIndex];
					dayNode.Date = item.Date;
					dayNode.ParentRow = node;

					if(dayNode.IsCarEventTypeFromJournal || dayNode.IsVirtualCarEventType || item.CarEventType != null)
					{
						if(ShouldClearDayPotential(dayNode.CarEventType ?? item.CarEventType))
						{
							ClearDayPotential(dayNode);
						}
						else
						{
							ApplyScheduleItemPotential(dayNode, item);
						}
					}
					else
					{
						ApplyScheduleItemPotential(dayNode, item);
					}
				}
			}
		}

		private static bool ShouldClearDayPotential(CarEventType eventType)
		{
			return !IsAllowedCarEventType(eventType, _carEventTypeNamesNotClearingPotentialInDriverSchedule);
		}

		private static void ApplyScheduleItemPotential(DriverScheduleDayRow dayNode, DriverScheduleItem item)
		{
			dayNode.MorningAddresses = item.MorningAddresses;
			dayNode.MorningBottles = item.MorningBottles;
			dayNode.EveningAddresses = item.EveningAddresses;
			dayNode.EveningBottles = item.EveningBottles;
		}

		private static void ClearDayPotential(DriverScheduleDayRow dayNode)
		{
			dayNode.MorningAddresses = 0;
			dayNode.MorningBottles = 0;
			dayNode.EveningAddresses = 0;
			dayNode.EveningBottles = 0;
		}

		private List<DriverScheduleRow> AddTotalRows(
			IEnumerable<DriverScheduleRow> driverRows,
			DateTime startDate)
		{
			var totalAddresses = new DriverScheduleTotalAddressesRow();
			var totalBottles = new DriverScheduleTotalBottlesRow();

			totalAddresses.Days = new DriverScheduleDayRow[7];
			totalBottles.Days = new DriverScheduleDayRow[7];

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				totalAddresses.Days[dayIndex] = new DriverScheduleDayRow();
				totalBottles.Days[dayIndex] = new DriverScheduleDayRow();
			}

			totalAddresses.StartDate = startDate;
			totalBottles.StartDate = startDate;

			var result = new List<DriverScheduleRow>(driverRows)
			{
				totalAddresses,
				totalBottles
			};

			RecalculateTotalRows(result);

			return result;
		}

		public void RecalculateTotalRows(IEnumerable<DriverScheduleRow> allRows)
		{
			var totalRows = allRows.OfType<DriverScheduleTotalRow>().ToList();
			var driverRows = allRows
				.Where(r => !(r is DriverScheduleTotalRow))
				.ToList();

			if(!totalRows.Any() || !driverRows.Any())
			{
				return;
			}

			var totalAddresses = totalRows.OfType<DriverScheduleTotalAddressesRow>().FirstOrDefault();
			var totalBottles = totalRows.OfType<DriverScheduleTotalBottlesRow>().FirstOrDefault();

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				if(totalAddresses != null)
				{
					totalAddresses.Days[dayIndex].MorningAddresses = driverRows.Sum(r => r.Days[dayIndex].MorningAddresses);
					totalAddresses.Days[dayIndex].EveningAddresses = driverRows.Sum(r => r.Days[dayIndex].EveningAddresses);

					int total = totalAddresses.Days[dayIndex].MorningAddresses + totalAddresses.Days[dayIndex].EveningAddresses;
					totalAddresses.Days[dayIndex].CarEventType = new CarEventType { ShortName = total.ToString() };
				}

				if(totalBottles != null)
				{
					totalBottles.Days[dayIndex].MorningBottles = driverRows.Sum(r => r.Days[dayIndex].MorningBottles);
					totalBottles.Days[dayIndex].EveningBottles = driverRows.Sum(r => r.Days[dayIndex].EveningBottles);

					int total = totalBottles.Days[dayIndex].MorningBottles + totalBottles.Days[dayIndex].EveningBottles;
					totalBottles.Days[dayIndex].CarEventType = new CarEventType { ShortName = total.ToString() };
				}
			}
		}

		public void SaveScheduleChanges(
			IUnitOfWork uow,
			IEnumerable<DriverScheduleRow> changedRows,
			DateTime startDate,
			DateTime endDate,
			int currentUserId)
		{
			var driverRows = changedRows.ToList();
			var driverIds = driverRows.Select(r => r.DriverId).Distinct().ToArray();

			if(!driverIds.Any())
			{
				return;
			}

			var existingSchedules = _logisticRepository.GetDriverSchedules(uow, driverIds, startDate, endDate);
			var schedulesByDriverId = existingSchedules
				.Where(s => s.Driver != null)
				.ToDictionary(s => s.Driver.Id, s => s);

			foreach(var driverNode in driverRows)
			{
				if(schedulesByDriverId.TryGetValue(driverNode.DriverId, out var driverSchedule))
				{
					UpdateExistingSchedule(driverSchedule, driverNode);
				}
				else
				{
					driverSchedule = CreateNewSchedule(uow, driverNode);
					schedulesByDriverId[driverNode.DriverId] = driverSchedule;
				}

				FillDayScheduleItems(driverSchedule, driverNode);
				ProcessDriverCarEvents(uow, driverNode, currentUserId);

				uow.Save(driverSchedule);
				driverNode.HasChanges = false;
			}
		}

		public IList<Schedule> GetDriverSchedulesAtDay(IUnitOfWork uow, IEnumerable<int> driverIds, DateTime date)
		{
			var driverIdsArray = driverIds?.ToArray() ?? Array.Empty<int>();

			return driverIdsArray.Any()
				? _logisticRepository.GetDriverSchedulesAtDay(uow, driverIdsArray, date)
				: new List<Schedule>();
		}

		public IList<int> GetDriverIdsWithDriverScheduleEventsAtDay(IUnitOfWork uow, IEnumerable<int> driverIds, DateTime date)
		{
			var driverIdsArray = driverIds?.ToArray() ?? Array.Empty<int>();

			if(!driverIdsArray.Any())
			{
				return new List<int>();
			}

			return _logisticRepository.GetCarEventsByDriverIds(uow, driverIdsArray, date.Date, date.Date)
				.Where(carEvent => carEvent.Driver != null)
				.Where(carEvent => carEvent.StartDate.Date <= date.Date && carEvent.EndDate.Date >= date.Date)
				.Where(carEvent => IsAllowedToShowInDriverSchedule(carEvent.CarEventType))
				.Select(carEvent => carEvent.Driver.Id)
				.Distinct()
				.ToList();
		}

		public DriverScheduleTotals GetDriverScheduleTotalsAtDay(
			IUnitOfWork uow,
			DateTime date,
			bool canEditAfter13)
		{
			var startDate = GetWeekStart(date);
			var endDate = startDate.AddDays(_daysInWeek - 1);
			var selectedCarTypeOfUse = EnumHelper.GetValuesList<CarTypeOfUse>()
				.Where(typeOfUse => typeOfUse != CarTypeOfUse.Loader && typeOfUse != CarTypeOfUse.Truck)
				.ToArray();
			var selectedCarOwnTypes = EnumHelper.GetValuesList<CarOwnType>().ToArray();
			var selectedSubdivisionIds = _logisticRepository
				.GetSubdivisionsForDriverSchedule(uow, selectedCarTypeOfUse, startDate, endDate)
				.Select(subdivision => subdivision.Id)
				.ToArray();

			if(!selectedSubdivisionIds.Any())
			{
				return new DriverScheduleTotals(0, 0);
			}

			var rows = LoadScheduleData(
					uow,
					startDate,
					endDate,
					selectedSubdivisionIds,
					selectedCarOwnTypes,
					selectedCarTypeOfUse,
					canEditAfter13,
					new List<CarEventType>())
				.ToList();

			var dayIndex = (int)(date.Date - startDate).TotalDays;
			var totalBottlesRow = rows.OfType<DriverScheduleTotalBottlesRow>().FirstOrDefault();
			var totalAddressesRow = rows.OfType<DriverScheduleTotalAddressesRow>().FirstOrDefault();

			return new DriverScheduleTotals(
				GetBottlesTotal(totalBottlesRow, dayIndex),
				GetAddressesTotal(totalAddressesRow, dayIndex));
		}

		private static DateTime GetWeekStart(DateTime date)
		{
			var daysFromMonday = ((int)date.DayOfWeek + 6) % _daysInWeek;
			return date.Date.AddDays(-daysFromMonday);
		}

		private static int GetBottlesTotal(DriverScheduleRow row, int dayIndex)
		{
			return IsValidDay(row, dayIndex)
				? row.Days[dayIndex].MorningBottles + row.Days[dayIndex].EveningBottles
				: 0;
		}

		private static int GetAddressesTotal(DriverScheduleRow row, int dayIndex)
		{
			return IsValidDay(row, dayIndex)
				? row.Days[dayIndex].MorningAddresses + row.Days[dayIndex].EveningAddresses
				: 0;
		}

		private static bool IsValidDay(DriverScheduleRow row, int dayIndex)
		{
			return row?.Days != null
				&& dayIndex >= 0
				&& dayIndex < row.Days.Length
				&& row.Days[dayIndex] != null;
		}

		public bool CanCreateCarEventTypeFromDriverSchedule(CarEventType eventType)
		{
			return IsAllowedToCreateFromDriverSchedule(eventType);
		}

		private void UpdateExistingSchedule(
			Schedule schedule,
			DriverScheduleRow node)
		{
			if(schedule.Days == null)
			{
				schedule.Days = new List<DriverScheduleItem>();
			}

			bool hasChanges = schedule.MorningAddressesPotential != node.MorningAddresses ||
							  schedule.MorningBottlesPotential != node.MorningBottles ||
							  schedule.EveningAddressesPotential != node.EveningAddresses ||
							  schedule.EveningBottlesPotential != node.EveningBottles;

			if(hasChanges)
			{
				schedule.MorningAddressesPotential = node.MorningAddresses;
				schedule.MorningBottlesPotential = node.MorningBottles;
				schedule.EveningAddressesPotential = node.EveningAddresses;
				schedule.EveningBottlesPotential = node.EveningBottles;

				bool hasNonZeroValues = node.MorningAddresses != 0 ||
										node.MorningBottles != 0 ||
										node.EveningAddresses != 0 ||
										node.EveningBottles != 0;

				if(hasNonZeroValues)
				{
					schedule.LastChangeTime = DateTime.Now;
				}
			}

			var commentToUpdate = node.IsCommentEdited
				? node.EditedComment
				: node.OriginalComment;

			schedule.ArrivalTime = node.ArrivalTime;
			schedule.Comment = commentToUpdate;
		}

		/// <summary>
		/// Обрабатывает события ТС для водителя - создает/обновляет CarEvent для подряд идущих одинаковых событий
		/// </summary>
		private void ProcessDriverCarEvents(IUnitOfWork uow, DriverScheduleRow driverNode, int currentUserId)
		{
			if(!driverNode.IsCarAssigned)
			{
				return;
			}

			var car = _carRepository.GetCarByDriverId(uow, driverNode.DriverId);

			if(car == null)
			{
				return;
			}

			var eventGroups = GroupConsecutiveCarEvents(driverNode);
			var existingCarEvents = _logisticRepository
				.GetCarEventsByDriverIds(
					uow,
					new[] { driverNode.DriverId },
					driverNode.StartDate,
					driverNode.StartDate.AddDays(_daysInWeek - 1))
				.Where(carEvent => carEvent.Car?.Id == car.Id)
				.ToList();

			foreach(var group in eventGroups)
			{
				if(group.CarEventType == null || group.CarEventType.Id == 0)
				{
					continue;
				}

				CreateOrUpdateCarEvent(uow, car, driverNode, group, currentUserId, existingCarEvents);
			}
		}

		private List<CarEventGroup> GroupConsecutiveCarEvents(DriverScheduleRow driverNode)
		{
			var groups = new List<CarEventGroup>();
			CarEventGroup currentGroup = null;

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var day = driverNode.Days[dayIndex];
				var eventType = day.CarEventType;

				if(eventType == null
					|| eventType.Id <= 0
					|| day.HasActiveRouteList
					|| day.IsCarEventTypeFromJournal
					|| day.IsVirtualCarEventType
					|| !IsAllowedToCreateFromDriverSchedule(eventType))
				{
					if(currentGroup != null)
					{
						groups.Add(currentGroup);
						currentGroup = null;
					}
				}
				else
				{
					if(currentGroup == null || currentGroup.CarEventType.Id != eventType.Id)
					{
						if(currentGroup != null)
						{
							groups.Add(currentGroup);
						}

						currentGroup = new CarEventGroup
						{
							CarEventType = eventType,
							StartDate = day.Date,
							EndDate = day.Date,
							Comment = day.ParentRow.OriginalComment,
							DayIndices = new List<int> { dayIndex }
						};
					}
					else
					{
						currentGroup.EndDate = day.Date;
						currentGroup.DayIndices.Add(dayIndex);
					}
				}
			}

			if(currentGroup != null)
			{
				groups.Add(currentGroup);
			}

			return groups;
		}

		private void CreateOrUpdateCarEvent(
			IUnitOfWork uow,
			Car car,
			DriverScheduleRow driverNode,
			CarEventGroup group,
			int currentUserId,
			IList<CarEvent> existingCarEvents)
		{
			if(group.CarEventType?.Id <= 0 || !IsAllowedToCreateFromDriverSchedule(group.CarEventType))
			{
				return;
			}

			var endOfDay = new DateTime(group.EndDate.Year, group.EndDate.Month, group.EndDate.Day, 23, 59, 59);
			var existingSameTypeEvents = existingCarEvents
				.Where(carEvent => carEvent.CarEventType?.Id == group.CarEventType.Id)
				.Where(carEvent => carEvent.StartDate <= endOfDay && carEvent.EndDate >= group.StartDate.Date)
				.ToList();

			var existingEvent = existingSameTypeEvents.FirstOrDefault(IsCreatedFromDriverSchedule);
			var existingNotDriverScheduleEvent = existingSameTypeEvents.FirstOrDefault(carEvent => !IsCreatedFromDriverSchedule(carEvent));
			var existingDriverScheduleEventCoversGroup = existingEvent != null
				&& existingEvent.StartDate.Date <= group.StartDate.Date
				&& existingEvent.EndDate >= endOfDay;

			if(existingNotDriverScheduleEvent != null || existingDriverScheduleEventCoversGroup)
			{
				return;
			}

			var commentToSave = driverNode.IsCommentEdited
				? driverNode.EditedComment
				: driverNode.OriginalComment;

			if(existingEvent == null)
			{
				var newEvent = new CarEvent
				{
					Car = car,
					CarEventType = group.CarEventType,
					Driver = uow.GetById<Employee>(driverNode.DriverId),
					StartDate = group.StartDate,
					EndDate = group.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59),
					Comment = commentToSave,
					Foundation = _driverScheduleCarEventFoundation,
					CreateDate = DateTime.Now,
					Author = _employeeService.GetEmployeeForUser(uow, currentUserId)
				};

				uow.Session.Save(newEvent);
				existingCarEvents.Add(newEvent);
			}
			else
			{
				existingEvent.StartDate = group.StartDate.Date;
				existingEvent.EndDate = endOfDay;
				existingEvent.Comment = commentToSave;
				uow.Save(existingEvent);
			}
		}

		private static bool IsCreatedFromDriverSchedule(CarEvent carEvent)
		{
			return string.Equals(
				carEvent?.Foundation,
				_driverScheduleCarEventFoundation,
				StringComparison.Ordinal);
		}

		private bool IsAllowedToCreateFromDriverSchedule(CarEventType eventType)
		{
			return IsAllowedCarEventType(eventType, _carEventTypeNamesAllowedToCreateFromDriverSchedule);
		}

		private bool IsAllowedToShowInDriverSchedule(CarEventType eventType)
		{
			return IsAllowedCarEventType(eventType, _carEventTypeNamesAllowedToShowInDriverSchedule);
		}

		private static bool IsAllowedCarEventType(CarEventType eventType, HashSet<string> allowedNames)
		{
			if(eventType == null)
			{
				return false;
			}

			return IsAllowedCarEventTypeName(eventType.ShortName, allowedNames)
				|| IsAllowedCarEventTypeName(eventType.Name, allowedNames);
		}

		private static bool IsAllowedCarEventTypeName(string eventTypeName, HashSet<string> allowedNames)
		{
			var normalizedEventTypeName = NormalizeCarEventTypeName(eventTypeName);

			return !string.IsNullOrEmpty(normalizedEventTypeName)
				&& allowedNames.Contains(normalizedEventTypeName);
		}

		private static HashSet<string> CreateCarEventTypeNamesSet(params string[] names)
		{
			return new HashSet<string>(
				names.Select(NormalizeCarEventTypeName),
				StringComparer.OrdinalIgnoreCase);
		}

		private static string NormalizeCarEventTypeName(string eventTypeName)
		{
			return eventTypeName?
				.Trim()
				.Replace('ё', 'е')
				.Replace('Ё', 'Е');
		}

		private Schedule CreateNewSchedule(
			IUnitOfWork uow,
			DriverScheduleRow node)
		{
			bool hasNonZeroValues = node.MorningAddresses != 0 ||
									node.MorningBottles != 0 ||
									node.EveningAddresses != 0 ||
									node.EveningBottles != 0;

			var schedule = new Schedule
			{
				Driver = uow.GetById<Employee>(node.DriverId),
				MorningAddressesPotential = node.MorningAddresses,
				MorningBottlesPotential = node.MorningBottles,
				EveningAddressesPotential = node.EveningAddresses,
				EveningBottlesPotential = node.EveningBottles,
				Comment = node.OriginalComment,
				LastChangeTime = hasNonZeroValues ? (DateTime?)DateTime.Now : null,
				Days = new List<DriverScheduleItem>()
			};

			uow.Session.Save(schedule);
			return schedule;
		}

		private void FillDayScheduleItems(
			Schedule driverSchedule,
			DriverScheduleRow driverNode)
		{
			driverSchedule.Days = driverSchedule.Days ?? new List<DriverScheduleItem>();

			for(int dayIndex = 0; dayIndex < driverNode.Days.Length; dayIndex++)
			{
				var dayScheduleNode = driverNode.Days[dayIndex];
				if(dayScheduleNode == null || dayScheduleNode.Date == default)
				{
					continue;
				}

				var scheduleItem = driverSchedule.Days.FirstOrDefault(si => si.Date == dayScheduleNode.Date);

				if(scheduleItem == null)
				{
					scheduleItem = new DriverScheduleItem
					{
						DriverSchedule = driverSchedule,
						Date = dayScheduleNode.Date
					};
					driverSchedule.Days.Add(scheduleItem);
				}

				if(dayScheduleNode.IsCarEventTypeFromJournal || dayScheduleNode.IsVirtualCarEventType)
				{
					scheduleItem.CarEventType = null;

					if(ShouldClearDayPotential(dayScheduleNode.CarEventType))
					{
						ClearScheduleItemPotential(scheduleItem);
					}
					else
					{
						ApplyDayPotential(scheduleItem, dayScheduleNode);
					}
				}
				else if((dayScheduleNode.CarEventType?.Id ?? 0) > 0)
				{
					scheduleItem.CarEventType = dayScheduleNode.CarEventType;

					if(ShouldClearDayPotential(dayScheduleNode.CarEventType))
					{
						ClearScheduleItemPotential(scheduleItem);
					}
					else
					{
						ApplyDayPotential(scheduleItem, dayScheduleNode);
					}
				}
				else
				{
					scheduleItem.CarEventType = null;
					ApplyDayPotential(scheduleItem, dayScheduleNode);
				}
			}
		}

		private static void ApplyDayPotential(DriverScheduleItem scheduleItem, DriverScheduleDayRow dayNode)
		{
			scheduleItem.MorningAddresses = dayNode.MorningAddresses;
			scheduleItem.MorningBottles = dayNode.MorningBottles;
			scheduleItem.EveningAddresses = dayNode.EveningAddresses;
			scheduleItem.EveningBottles = dayNode.EveningBottles;
		}

		private static void ClearScheduleItemPotential(DriverScheduleItem scheduleItem)
		{
			scheduleItem.MorningAddresses = 0;
			scheduleItem.MorningBottles = 0;
			scheduleItem.EveningAddresses = 0;
			scheduleItem.EveningBottles = 0;
		}

		public byte[] ExportToExcel(
			IEnumerable<DriverScheduleRow> scheduleRows,
			DateTime startDate,
			DateTime endDate)
		{
			if(scheduleRows == null)
			{
				throw new ArgumentNullException(nameof(scheduleRows));
			}

			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("График водителей");

				FillExcelHeaders(worksheet, startDate);
				FillExcelData(worksheet, scheduleRows, startDate);

				using(var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					return stream.ToArray();
				}
			}
		}

		private void FillExcelHeaders(IXLWorksheet worksheet, DateTime startDate)
		{
			int row = 1;

			worksheet.Cell(row, (int)ExportColumn.CarTypeOfUse).Value = "Т";
			worksheet.Cell(row, (int)ExportColumn.CarOwnType).Value = "П";
			worksheet.Cell(row, (int)ExportColumn.RegNumber).Value = "Гос. номер";
			worksheet.Cell(row, (int)ExportColumn.DriverFullName).Value = "ФИО водителя";
			worksheet.Cell(row, (int)ExportColumn.DriverCarOwnType).Value = "Принадлежность";
			worksheet.Cell(row, (int)ExportColumn.Phone).Value = "Телефон";
			worksheet.Cell(row, (int)ExportColumn.District).Value = "Район проживания";
			worksheet.Cell(row, (int)ExportColumn.ArrivalTime).Value = "Время приезда";
			worksheet.Cell(row, (int)ExportColumn.MorningAddressesPotential).Value = "Потенциал Утро (адр.)";
			worksheet.Cell(row, (int)ExportColumn.MorningBottlesPotential).Value = "Потенциал Утро (бут.)";
			worksheet.Cell(row, (int)ExportColumn.EveningAddressesPotential).Value = "Потенциал Вечер (адр.)";
			worksheet.Cell(row, (int)ExportColumn.EveningBottlesPotential).Value = "Потенциал Вечер (бут.)";
			worksheet.Cell(row, (int)ExportColumn.LastModifiedDateTime).Value = "Дата посл. изм.";

			for(int dayIndex = 0; dayIndex < _daysInWeek; dayIndex++)
			{
				var date = startDate.AddDays(dayIndex);
				int dayColumn = _firstDayColumn + dayIndex * _columnsPerDay;

				worksheet.Cell(row, dayColumn).Value = "'" + GetShortDayString(date);
				worksheet.Cell(row, dayColumn + 1).Value = "Адр У";
				worksheet.Cell(row, dayColumn + 2).Value = "Бут У";
				worksheet.Cell(row, dayColumn + 3).Value = "Адр В";
				worksheet.Cell(row, dayColumn + 4).Value = "Бут В";
			}

			worksheet.Cell(row, _commentColumn).Value = "Комментарий";
		}

		private void FillExcelData(IXLWorksheet worksheet, IEnumerable<DriverScheduleRow> scheduleRows, DateTime startDate)
		{
			int row = 2;

			foreach(var node in scheduleRows)
			{
				worksheet.Cell(row, (int)ExportColumn.CarTypeOfUse).Value = node.CarTypeOfUseString;
				worksheet.Cell(row, (int)ExportColumn.CarOwnType).Value = node.CarOwnTypeString;
				worksheet.Cell(row, (int)ExportColumn.RegNumber).Value = node.RegNumber ?? "";
				worksheet.Cell(row, (int)ExportColumn.DriverFullName).Value = node.DriverFullName;
				worksheet.Cell(row, (int)ExportColumn.DriverCarOwnType).Value = node.DriverCarOwnTypeString;
				worksheet.Cell(row, (int)ExportColumn.Phone).Value = node.DriverPhone ?? "";
				worksheet.Cell(row, (int)ExportColumn.District).Value = node.DistrictString;
				worksheet.Cell(row, (int)ExportColumn.ArrivalTime).Value = node.ArrivalTime.HasValue
					? node.ArrivalTime.Value.ToString(@"hh\:mm")
					: "";
				worksheet.Cell(row, (int)ExportColumn.MorningAddressesPotential).Value = node.MorningAddresses;
				worksheet.Cell(row, (int)ExportColumn.MorningBottlesPotential).Value = node.MorningBottles;
				worksheet.Cell(row, (int)ExportColumn.EveningAddressesPotential).Value = node.EveningAddresses;
				worksheet.Cell(row, (int)ExportColumn.EveningBottlesPotential).Value = node.EveningBottles;
				worksheet.Cell(row, (int)ExportColumn.LastModifiedDateTime).Value = node.LastModifiedDateTimeString;

				for(int dayIndex = 0; dayIndex < _daysInWeek; dayIndex++)
				{
					var day = node.Days[dayIndex];
					int dayColumn = _firstDayColumn + dayIndex * _columnsPerDay;

					if(node is DriverScheduleTotalAddressesRow)
					{
						worksheet.Cell(row, dayColumn).Value = day.CarEventType?.ShortName ?? "";
					}
					else if(node is DriverScheduleTotalBottlesRow)
					{
						worksheet.Cell(row, dayColumn).Value = day.CarEventType?.ShortName ?? "";
					}
					else
					{
						worksheet.Cell(row, dayColumn).Value = day.CarEventType?.ShortName ?? "Нет";
					}

					worksheet.Cell(row, dayColumn + 1).Value = day.MorningAddresses;
					worksheet.Cell(row, dayColumn + 2).Value = day.MorningBottles;
					worksheet.Cell(row, dayColumn + 3).Value = day.EveningAddresses;
					worksheet.Cell(row, dayColumn + 4).Value = day.EveningBottles;
				}

				worksheet.Cell(row, _commentColumn).Value = node.OriginalComment ?? "";
				row++;
			}

			worksheet.Columns().AdjustToContents();
		}

		public IEnumerable<SubdivisionNode> GetSubdivisions(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			CarTypeOfUse[] carTypeOfUse)
		{
			var subdivisions = _logisticRepository.GetSubdivisionsForDriverSchedule(
				uow, carTypeOfUse, startDate, endDate);

			return subdivisions.Select(subdivision =>
				new SubdivisionNode(subdivision) { Selected = true });
		}

		private List<DriverScheduleRow> FilterByDismissalDate(
			IList<DriverScheduleRow> driverRows,
			DateTime startDate)
		{
			return driverRows.Where(r =>
			{
				var dismissalDate = r.GetDismissalDate();
				return !dismissalDate.HasValue || dismissalDate.Value.Date > startDate.Date;
			}).ToList();
		}

		public string GetShortDayString(DateTime date)
		{
			string dayOfWeek;
			switch(date.DayOfWeek)
			{
				case DayOfWeek.Monday:
					dayOfWeek = "Пн";
					break;
				case DayOfWeek.Tuesday:
					dayOfWeek = "Вт";
					break;
				case DayOfWeek.Wednesday:
					dayOfWeek = "Ср";
					break;
				case DayOfWeek.Thursday:
					dayOfWeek = "Чт";
					break;
				case DayOfWeek.Friday:
					dayOfWeek = "Пт";
					break;
				case DayOfWeek.Saturday:
					dayOfWeek = "Сб";
					break;
				case DayOfWeek.Sunday:
					dayOfWeek = "Вс";
					break;
				default:
					dayOfWeek = "";
					break;
			}

			return $"{dayOfWeek}, {date:dd.MM.yyyy}";
		}
	}
}
