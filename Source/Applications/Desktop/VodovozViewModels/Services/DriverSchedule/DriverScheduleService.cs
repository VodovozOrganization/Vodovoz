using ClosedXML.Excel;
using QS.DomainModel.UoW;
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
			var driverCarEvents = carEvents.Where(ce => ce.Driver?.Id == node.DriverId).ToList();
			var eventWithComment = driverCarEvents.FirstOrDefault(carEvent => !string.IsNullOrEmpty(carEvent.Comment));

			if(eventWithComment != null)
			{
				node.OriginalComment = eventWithComment.Comment;
			}

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				var dayDate = startDate.AddDays(dayIndex);
				var applicableEvent = driverCarEvents.FirstOrDefault(ce =>
					ce.StartDate.Date <= dayDate && ce.EndDate.Date >= dayDate);

				if(applicableEvent != null && !node.Days[dayIndex].IsVirtualCarEventType)
				{
					node.Days[dayIndex].CarEventType = applicableEvent.CarEventType;
					node.Days[dayIndex].IsCarEventTypeFromJournal = true;
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
						dayNode.MorningAddresses = 0;
						dayNode.MorningBottles = 0;
						dayNode.EveningAddresses = 0;
						dayNode.EveningBottles = 0;
					}
					else
					{
						dayNode.MorningAddresses = item.MorningAddresses;
						dayNode.MorningBottles = item.MorningBottles;
						dayNode.EveningAddresses = item.EveningAddresses;
						dayNode.EveningBottles = item.EveningBottles;
					}
				}
			}
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

			var filteredRows = driverRows.Where(n => !(n is DriverScheduleTotalAddressesRow) && !(n is DriverScheduleTotalBottlesRow));

			foreach(var node in filteredRows)
			{
				for(int dayIndex = 0; dayIndex < 7; dayIndex++)
				{
					var day = node.Days[dayIndex];
					if(day != null)
					{
						totalAddresses.Days[dayIndex].MorningAddresses += day.MorningAddresses;
						totalAddresses.Days[dayIndex].EveningAddresses += day.EveningAddresses;

						totalBottles.Days[dayIndex].MorningBottles += day.MorningBottles;
						totalBottles.Days[dayIndex].EveningBottles += day.EveningBottles;
					}
				}
			}

			for(int dayIndex = 0; dayIndex < 7; dayIndex++)
			{
				int totalAddressesForDay = totalAddresses.Days[dayIndex].MorningAddresses +
										 totalAddresses.Days[dayIndex].EveningAddresses;
				totalAddresses.Days[dayIndex].CarEventType = new CarEventType
				{
					ShortName = totalAddressesForDay.ToString()
				};

				int totalBottlesForDay = totalBottles.Days[dayIndex].MorningBottles +
									   totalBottles.Days[dayIndex].EveningBottles;
				totalBottles.Days[dayIndex].CarEventType = new CarEventType
				{
					ShortName = totalBottlesForDay.ToString()
				};
			}

			totalAddresses.StartDate = startDate;
			totalBottles.StartDate = startDate;

			var result = new List<DriverScheduleRow>();
			result.AddRange(filteredRows);
			result.Add(totalAddresses);
			result.Add(totalBottles);

			return result;
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

			schedule.ArrivalTime = node.ArrivalTime;
			schedule.Comment = node.OriginalComment;
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

			foreach(var group in eventGroups)
			{
				if(group.CarEventType == null || group.CarEventType.Id == 0)
				{
					continue;
				}

				CreateOrUpdateCarEvent(uow, car, driverNode, group, currentUserId);
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

				if(eventType == null || eventType.Id == 0)
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

		private void CreateOrUpdateCarEvent(IUnitOfWork uow, Car car, DriverScheduleRow driverNode, CarEventGroup group, int currentUserId)
		{
			if(group.CarEventType?.Id <= 0)
			{
				return;
			}

			var endOfDay = new DateTime(group.EndDate.Year, group.EndDate.Month, group.EndDate.Day, 23, 59, 59);

			var existingEvent = _logisticRepository.GetCarEventByCarId(uow, car.Id, group, endOfDay);

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
					Foundation = "Создано из графика водителей",
					CreateDate = DateTime.Now,
					Author = _employeeService.GetEmployeeForUser(uow, currentUserId)
				};

				uow.Session.Save(newEvent);
			}
			else
			{
				existingEvent.EndDate = endOfDay;
				existingEvent.Comment = commentToSave;
				uow.Save(existingEvent);
			}
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

				if((dayScheduleNode.CarEventType?.Id ?? 0) > 0)
				{
					scheduleItem.CarEventType = dayScheduleNode.CarEventType;
					scheduleItem.MorningAddresses = 0;
					scheduleItem.MorningBottles = 0;
					scheduleItem.EveningAddresses = 0;
					scheduleItem.EveningBottles = 0;
				}
				else
				{
					scheduleItem.CarEventType = null;
					scheduleItem.MorningAddresses = dayScheduleNode.MorningAddresses;
					scheduleItem.MorningBottles = dayScheduleNode.MorningBottles;
					scheduleItem.EveningAddresses = dayScheduleNode.EveningAddresses;
					scheduleItem.EveningBottles = dayScheduleNode.EveningBottles;
				}
			}
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
