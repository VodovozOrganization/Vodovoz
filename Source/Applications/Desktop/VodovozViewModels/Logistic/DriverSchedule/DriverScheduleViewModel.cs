using ClosedXML.Excel;
using MoreLinq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using Vodovoz.Services;
using Vodovoz.Settings.Logistics;
using VodovozBusiness.EntityRepositories.Logistic;
using VodovozBusiness.Nodes;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.ViewModels.ViewModels.Logistic.DriverSchedule
{
	public class DriverScheduleViewModel : DialogTabViewModelBase
	{
		private const int _firstDayColumn = 14;
		private const int _columnsPerDay = 5;
		private const int _daysInWeek = 7;
		private const int _commentColumn = _firstDayColumn + _daysInWeek * _columnsPerDay;

		private readonly IInteractiveService _interactiveService;
		private readonly ICurrentPermissionService _currentPermissionService;
		private readonly ICarEventSettings _carEventSettings;
		private readonly IEmployeeService _employeeService;
		private readonly IUserService _userService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ILogisticRepository _logisticRepository;
		private readonly ICarRepository _carRepository;
		private readonly IRouteListRepository _routeListRepository;

		private ObservableList<SubdivisionNode> _subdivisions;
		private IList<CarTypeOfUse> _selectedCarTypeOfUse;
		private IList<CarOwnType> _selectedCarOwnTypes;
		private IList<int> _selectedSubdivisionIds;
		private DateTime _startDate;
		private DateTime _endDate;

		public DriverScheduleViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			ICarEventSettings carEventSettings,
			INavigationManager navigation,
			IStringHandler stringHandler,
			IDatePickerViewModelFactory weekPickerViewModelFactory,
			IEmployeeService employeeService,
			IUserService userService,
			IFileDialogService fileDialogService,
			ILogisticRepository logisticRepository,
			ICarRepository carRepository,
			IRouteListRepository routeListRepository
			) : base(unitOfWorkFactory, interactiveService, navigation)
		{

			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_currentPermissionService = currentPermissionService ?? throw new ArgumentNullException(nameof(currentPermissionService));
			_carEventSettings = carEventSettings ?? throw new ArgumentNullException(nameof(carEventSettings));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_logisticRepository = logisticRepository ?? throw new ArgumentNullException(nameof(logisticRepository));
			_carRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));

			InitializeWeekPicker(weekPickerViewModelFactory);

			Title = "График водителей";

			SetPermissions();
			var typesOfUse = EnumHelper.GetValuesList<CarTypeOfUse>().ToList();
			typesOfUse.Remove(CarTypeOfUse.Loader);
			typesOfUse.Remove(CarTypeOfUse.Truck);

			var carOwnTypes = EnumHelper.GetValuesList<CarOwnType>();

			SelectedCarTypeOfUse = typesOfUse;
			SelectedCarOwnTypes = carOwnTypes;

			InitializeSubdivisions();

			DriverScheduleRows = GenerateRows();
			LoadAvailableCarEventTypes();

			SaveCommand = new DelegateCommand(SaveDriverSchedule, () => CanSave);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanSave);
			CancelCommand = new DelegateCommand(() => Close(AskSaveOnClose, CloseSource.Cancel));
			ExportCommand = new DelegateCommand(() => Export());
			InfoCommand = new DelegateCommand(() => ShowInfoMessage());
			ApplyFiltersCommand = new DelegateCommand(() =>
			{
				DriverScheduleRows = GenerateRows();
				OnPropertyChanged(nameof(DriverScheduleRows));
			});
		}

		public DatePickerViewModel WeekPickerViewModel { get; private set; }

		public IInteractiveService InteractiveService => _interactiveService;

		public IList<CarTypeOfUse> SelectedCarTypeOfUse
		{
			get => _selectedCarTypeOfUse;
			set => SetField(ref _selectedCarTypeOfUse, value);
		}

		public IList<CarOwnType> SelectedCarOwnTypes
		{
			get => _selectedCarOwnTypes;
			set => SetField(ref _selectedCarOwnTypes, value);
		}

		public IList<int> SelectedSubdivisionIds
		{
			get => _selectedSubdivisionIds;
			set => SetField(ref _selectedSubdivisionIds, value);
		}

		public DateTime StartDate
		{
			get => _startDate;
			private set => SetField(ref _startDate, value);
		}

		public DateTime EndDate
		{
			get => _endDate;
			private set => SetField(ref _endDate, value);
		}

		public ObservableList<SubdivisionNode> Subdivisions
		{
			get => _subdivisions;
			private set => SetField(ref _subdivisions, value);
		}

		public bool CanEdit;
		public bool CanEditAfter13;
		public bool CanSave => CanEdit;
		public bool AskSaveOnClose => CanEdit;

		public ObservableList<DriverScheduleNode> DriverScheduleRows { get; private set; }
		public List<CarEventType> AvailableCarEventTypes { get; } = new List<CarEventType>();

		public IStringHandler StringHandler { get; }

		public DelegateCommand SaveCommand { get; }
		public DelegateCommand CancelCommand { get; }
		public DelegateCommand ExportCommand { get; }
		public DelegateCommand InfoCommand { get; }
		public DelegateCommand ApplyFiltersCommand { get; }

		private void InitializeWeekPicker(IDatePickerViewModelFactory weekPickerViewModelFactory)
		{
			WeekPickerViewModel = weekPickerViewModelFactory.CreateNewDatePickerViewModel(
				DateTime.Now,
				ChangeDateType.Week);

			WeekPickerViewModel.DateChanged += (s, e) => UpdateDateRange();
			UpdateDateRange();
		}

		private void UpdateDateRange()
		{
			StartDate = WeekPickerViewModel.SelectedDate;
			EndDate = WeekPickerViewModel.SelectedDate.AddDays(6);
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

		private void SetPermissions()
		{
			CanEdit = _currentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.LogisticPermissions.CanWorkWithDriverSchedule);
			CanEditAfter13 = _currentPermissionService.ValidatePresetPermission(Core.Domain.Permissions.LogisticPermissions.CanEditEventsAndCapacitiesAfter13);
		}

		private void InitializeSubdivisions()
		{
			var carTypeOfUseArray = SelectedCarTypeOfUse.ToArray();

			var subdivisions = _logisticRepository.GetSubdivisionsForDriverSchedule(UoW, carTypeOfUseArray, StartDate, EndDate);

			Subdivisions = new ObservableList<SubdivisionNode>(
				subdivisions.Select(subdivision => new SubdivisionNode(subdivision) { Selected = true })
			);
		}

		private ObservableList<DriverScheduleNode> GenerateRows()
		{
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			Phone phoneAlias = null;
			DriverScheduleNode resultAlias = null;
			CarVersion carVersionAlias = null;
			VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule driverScheduleAlias = null;

			var selectedSubdivisionIds = Subdivisions
				.Where(s => s.Selected)
				.Select(s => s.SubdivisionId)
				.ToArray();

			var driversQuery = _logisticRepository.GetDriversQueryWithJoins(UoW, selectedSubdivisionIds, StartDate, EndDate);

			if(SelectedCarOwnTypes != null && SelectedCarOwnTypes.Any())
			{
				driversQuery.WhereRestrictionOn(() => employeeAlias.DriverOfCarOwnType).IsIn(SelectedCarOwnTypes.ToArray());
			}
			else
			{
				driversQuery.Where(Restrictions.IsNull(Projections.Property(() => employeeAlias.DriverOfCarOwnType)));
			}

			if(SelectedCarTypeOfUse != null && SelectedCarTypeOfUse.Any())
			{
				driversQuery.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(SelectedCarTypeOfUse.ToArray());
			}
			else
			{
				driversQuery.Where(Restrictions.IsNull(Projections.Property(() => carModelAlias.CarTypeOfUse)));
			}

			var phoneSubquery = QueryOver.Of(() => phoneAlias)
				.Where(() => phoneAlias.Employee.Id == employeeAlias.Id)
				.OrderBy(() => phoneAlias.Id).Asc
				.Select(Projections.Property(() => phoneAlias.Number))
				.Take(1);

			var result = driversQuery
				.SelectList(list => list
					.Select(e => e.Id).WithAlias(() => resultAlias.DriverId)
					.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => resultAlias.CarTypeOfUse)
					.Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => resultAlias.RegNumber)
					.Select(e => e.LastName).WithAlias(() => resultAlias.LastName)
					.Select(e => e.Name).WithAlias(() => resultAlias.Name)
					.Select(e => e.Patronymic).WithAlias(() => resultAlias.Patronymic)
					.Select(e => e.DriverOfCarOwnType).WithAlias(() => resultAlias.DriverCarOwnType)
					.Select(e => e.District).WithAlias(() => resultAlias.District)
					.Select(() => driverScheduleAlias.ArrivalTime).WithAlias(() => resultAlias.ArrivalTime)
					.Select(() => driverScheduleAlias.MorningAddressesPotential).WithAlias(() => resultAlias.MorningAddresses)
					.Select(() => driverScheduleAlias.MorningBottlesPotential).WithAlias(() => resultAlias.MorningBottles)
					.Select(() => driverScheduleAlias.EveningAddressesPotential).WithAlias(() => resultAlias.EveningAddresses)
					.Select(() => driverScheduleAlias.EveningBottlesPotential).WithAlias(() => resultAlias.EveningBottles)
					.Select(() => driverScheduleAlias.LastChangeTime).WithAlias(() => resultAlias.LastModifiedDateTime)
					.Select(() => driverScheduleAlias.Comment).WithAlias(() => resultAlias.Comment)
					.Select(() => employeeAlias.DateFired).WithAlias(() => resultAlias.DateFired)
					.Select(() => employeeAlias.DateCalculated).WithAlias(() => resultAlias.DateCalculated)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32,
							"FLOOR(COALESCE(?1, 0) / 20)"),
						NHibernateUtil.Int32,
						Projections.Property(() => carModelAlias.MaxWeight)))
					.WithAlias(() => resultAlias.MaxBottles)
					.SelectSubQuery(phoneSubquery).WithAlias(() => resultAlias.DriverPhone)
				)
				.OrderBy(e => e.LastName).Asc
				.TransformUsing(Transformers.AliasToBean<DriverScheduleNode>())
				.List<DriverScheduleNode>()
				.GroupBy(x => x.DriverId)
				.Select(g => g.First())
				.ToList();

			var filteredResult = result
				.Where(r =>
				{
					var dismissalDate = r.GetDismissalDate();
					return !dismissalDate.HasValue || dismissalDate.Value.Date > StartDate.Date;
				})
				.ToList();


			var driverIds = filteredResult.Select(r => r.DriverId).ToArray();

			var driversWithActiveRouteList = _routeListRepository.GetDriverIdsWithActiveRouteList(UoW, driverIds);

			if(driverIds.Any())
			{
				var carEvents = _logisticRepository.GetCarEventsByDriverIds(UoW, driverIds, StartDate, EndDate);
				var scheduleItems = _logisticRepository.GetDriverScheduleItemsByDriverIds(UoW, driverIds, StartDate, EndDate);

				foreach(var node in filteredResult)
				{
					node.StartDate = StartDate;
					node.CanEditAfter13 = CanEditAfter13;
					node.HasActiveRouteList = driversWithActiveRouteList.Contains(node.DriverId);

					for(int dayIndex = 0; dayIndex < 7; dayIndex++)
					{
						if(node.Days[dayIndex].Date == default)
						{
							node.Days[dayIndex].Date = StartDate.AddDays(dayIndex);
						}
						node.Days[dayIndex].ParentNode = node;
					}

					node.InitializeEmptyCarEventTypes();

					node.IsCarAssigned = !string.IsNullOrEmpty(node.RegNumber);

					var dismissalDate = node.GetDismissalDate();
					var isFiredOrCalculated = dismissalDate.HasValue
						&& dismissalDate.Value.Date >= StartDate
						&& dismissalDate.Value.Date <= EndDate;

					if(isFiredOrCalculated)
					{
						ProcessFiredDriverDays(node, dismissalDate.Value);
					}

					var driverCarEvents = carEvents
						.Where(ce => ce.Driver.Id == node.DriverId)
						.ToList();

					for(int dayIndex = 0; dayIndex < 7; dayIndex++)
					{
						var dayDate = StartDate.AddDays(dayIndex);
						var applicableEvent = driverCarEvents.FirstOrDefault(ce =>
							ce.StartDate.Date <= dayDate && ce.EndDate.Date >= dayDate);

						if(applicableEvent != null && !node.Days[dayIndex].IsVirtualCarEventType)
						{
							node.Days[dayIndex].CarEventType = applicableEvent.CarEventType;
							node.Days[dayIndex].IsCarEventTypeFromJournal = true;
						}
					}

					var driverScheduleItems = scheduleItems
						.Where(si => si.DriverSchedule.Driver.Id == node.DriverId)
						.ToList();

					foreach(var item in driverScheduleItems)
					{
						int dayIndex = (int)(item.Date - StartDate).TotalDays;
						if(dayIndex >= 0 && dayIndex < 7)
						{
							node.Days[dayIndex].Date = item.Date;
							node.Days[dayIndex].ParentNode = node;

							if(node.Days[dayIndex].IsCarEventTypeFromJournal
								|| node.Days[dayIndex].IsVirtualCarEventType
								|| item.CarEventType != null)
							{
								node.Days[dayIndex].MorningAddresses = 0;
								node.Days[dayIndex].MorningBottles = 0;
								node.Days[dayIndex].EveningAddresses = 0;
								node.Days[dayIndex].EveningBottles = 0;
								continue;
							}
							else
							{
								node.Days[dayIndex].MorningAddresses = item.MorningAddresses;
								node.Days[dayIndex].MorningBottles = item.MorningBottles;
								node.Days[dayIndex].EveningAddresses = item.EveningAddresses;
								node.Days[dayIndex].EveningBottles = item.EveningBottles;
							}
						}
					}
				}
			}

			return new ObservableList<DriverScheduleNode>(filteredResult);
		}

		private void ProcessFiredDriverDays(DriverScheduleNode driverNode, DateTime dismissalDate)
		{
			var eventType = GetDismissalEventType(driverNode);
			if(eventType == null)
			{
				return;
			}

			int dismissalDayIndex = (int)(dismissalDate - StartDate).TotalDays;

			for(int dayIndex = dismissalDayIndex; dayIndex < 7; dayIndex++)
			{
				if(dayIndex >= 0)
				{
					driverNode.Days[dayIndex].CarEventType = eventType;
					driverNode.Days[dayIndex].MorningAddresses = 0;
					driverNode.Days[dayIndex].MorningBottles = 0;
					driverNode.Days[dayIndex].EveningAddresses = 0;
					driverNode.Days[dayIndex].EveningBottles = 0;
					driverNode.Days[dayIndex].IsVirtualCarEventType = true;
				}
			}
		}

		private CarEventType GetDismissalEventType(DriverScheduleNode driverNode)
		{
			string eventName = driverNode.DateFired.HasValue && driverNode.DateCalculated.HasValue
				? (driverNode.DateFired.Value <= driverNode.DateCalculated.Value ? "Уволен" : "На расчете")
				: driverNode.DateFired.HasValue
					? "Уволен"
					: driverNode.DateCalculated.HasValue
						? "На расчете"
						: null;

			if(string.IsNullOrEmpty(eventName))
			{
				return null;
			}

			var eventType = AvailableCarEventTypes.FirstOrDefault(x => x.ShortName == eventName) 
				?? new CarEventType
				{
					Id = -1,
					ShortName = eventName,
					Name = eventName
				};

			return eventType;
		}

		private void LoadAvailableCarEventTypes()
		{
			var noneEventType = new CarEventType { Id = -1, ShortName = "Нет", Name = "Нет" };

			AvailableCarEventTypes.Add(noneEventType);

			var allowedIds = _carEventSettings.AllowedCarEventTypeIdsForDriverSchedule;

			AvailableCarEventTypes.AddRange(UoW.GetAll<CarEventType>()
				.Where(x => !x.IsArchive && allowedIds.Contains(x.Id))
				.ToList());
		}

		private void SaveDriverSchedule()
		{
			try
			{
				var driverIds = DriverScheduleRows.Select(r => r.DriverId).Distinct().ToArray();
				if(driverIds.Length == 0)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Info, "Нет данных для сохранения");
					return;
				}

				var existingSchedules = _logisticRepository.GetDriverSchedules(UoW, driverIds, StartDate, EndDate);

				var schedulesByDriverId = existingSchedules
					.Where(s => s.Driver != null)
					.ToDictionary(s => s.Driver.Id, s => s);

				var existingItemsByKey = new Dictionary<(int ScheduleId, DateTime Date), DriverScheduleItem>();

				foreach(var schedule in existingSchedules)
				{
					if(schedule.Days != null)
					{
						foreach(var item in schedule.Days)
						{
							if(item.Date != default)
							{
								existingItemsByKey[(schedule.Id, item.Date)] = item;
							}
						}
					}
				}

				foreach(var driverNode in DriverScheduleRows)
				{
					if(schedulesByDriverId.TryGetValue(driverNode.DriverId, out VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule driverSchedule))
					{
						if(driverSchedule.Days == null)
						{
							driverSchedule.Days = new List<DriverScheduleItem>();
						}

						bool hasChanges = driverSchedule.MorningAddressesPotential != driverNode.MorningAddresses ||
										  driverSchedule.MorningBottlesPotential != driverNode.MorningBottles ||
										  driverSchedule.EveningAddressesPotential != driverNode.EveningAddresses ||
										  driverSchedule.EveningBottlesPotential != driverNode.EveningBottles;

						if(hasChanges)
						{
							driverSchedule.MorningAddressesPotential = driverNode.MorningAddresses;
							driverSchedule.MorningBottlesPotential = driverNode.MorningBottles;
							driverSchedule.EveningAddressesPotential = driverNode.EveningAddresses;
							driverSchedule.EveningBottlesPotential = driverNode.EveningBottles;

							bool hasNonZeroValues = driverNode.MorningAddresses != 0 ||
													driverNode.MorningBottles != 0 ||
													driverNode.EveningAddresses != 0 ||
													driverNode.EveningBottles != 0;

							if(hasNonZeroValues)
							{
								driverSchedule.LastChangeTime = DateTime.Now;
							}
						}

						driverSchedule.ArrivalTime = driverNode.ArrivalTime;
						driverSchedule.Comment = driverNode.Comment;
					}
					else
					{
						bool hasNonZeroValues = driverNode.MorningAddresses != 0 ||
												driverNode.MorningBottles != 0 ||
												driverNode.EveningAddresses != 0 ||
												driverNode.EveningBottles != 0;

						driverSchedule = new VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule
						{
							Driver = UoW.GetById<Employee>(driverNode.DriverId),
							MorningAddressesPotential = driverNode.MorningAddresses,
							MorningBottlesPotential = driverNode.MorningBottles,
							EveningAddressesPotential = driverNode.EveningAddresses,
							EveningBottlesPotential = driverNode.EveningBottles,
							Comment = driverNode.Comment,
							LastChangeTime = hasNonZeroValues ? (DateTime?)DateTime.Now : null,
							Days = new List<DriverScheduleItem>()
						};

						UoW.Session.Save(driverSchedule);

						schedulesByDriverId[driverNode.DriverId] = driverSchedule;
					}

					FillDayScheduleItems(driverSchedule, driverNode);

					ProcessDriverCarEvents(driverNode);

					UoW.Save(driverSchedule);

					UpdateDriverNodeFromSchedule(driverNode, driverSchedule);

					OnPropertyChanged(nameof(DriverScheduleRows));
				}

				_interactiveService.ShowMessage(ImportanceLevel.Info, "График водителей успешно сохранен");
				UoW.Commit();
			}
			catch(Exception ex)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Error,
					$"Ошибка при сохранении графика водителей:\n{ex.Message}\n{ex.InnerException?.Message}");
			}
		}

		private void FillDayScheduleItems(VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule driverSchedule, DriverScheduleNode driverNode)
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
					UoW.Session.Save(scheduleItem);
				}

				scheduleItem.CarEventType = (dayScheduleNode.CarEventType?.Id ?? 0) > 0
					? dayScheduleNode.CarEventType 
					: null;

				if(scheduleItem.CarEventType != null)
				{
					scheduleItem.MorningAddresses = 0;
					scheduleItem.MorningBottles = 0;
					scheduleItem.EveningAddresses = 0;
					scheduleItem.EveningBottles = 0;
				}
				else
				{
					scheduleItem.MorningAddresses = dayScheduleNode.MorningAddresses;
					scheduleItem.MorningBottles = dayScheduleNode.MorningBottles;
					scheduleItem.EveningAddresses = dayScheduleNode.EveningAddresses;
					scheduleItem.EveningBottles = dayScheduleNode.EveningBottles;
				}
			}
		}

		private void UpdateDriverNodeFromSchedule(DriverScheduleNode driverNode, VodovozBusiness.Domain.Logistic.Drivers.DriverSchedule driverSchedule)
		{
			if(driverSchedule.Days != null)
			{
				foreach(var scheduleItem in driverSchedule.Days)
				{
					int dayIndex = (int)(scheduleItem.Date - StartDate).TotalDays;
					if(dayIndex >= 0 && dayIndex < 7)
					{
						switch(dayIndex)
						{
							case 0:
								driverNode.MondayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.MondayMorningBottles = scheduleItem.MorningBottles;
								driverNode.MondayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.MondayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 1:
								driverNode.TuesdayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.TuesdayMorningBottles = scheduleItem.MorningBottles;
								driverNode.TuesdayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.TuesdayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 2:
								driverNode.WednesdayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.WednesdayMorningBottles = scheduleItem.MorningBottles;
								driverNode.WednesdayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.WednesdayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 3:
								driverNode.ThursdayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.ThursdayMorningBottles = scheduleItem.MorningBottles;
								driverNode.ThursdayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.ThursdayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 4:
								driverNode.FridayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.FridayMorningBottles = scheduleItem.MorningBottles;
								driverNode.FridayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.FridayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 5:
								driverNode.SaturdayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.SaturdayMorningBottles = scheduleItem.MorningBottles;
								driverNode.SaturdayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.SaturdayEveningBottles = scheduleItem.EveningBottles;
								break;
							case 6:
								driverNode.SundayMorningAddress = scheduleItem.MorningAddresses;
								driverNode.SundayMorningBottles = scheduleItem.MorningBottles;
								driverNode.SundayEveningAddress = scheduleItem.EveningAddresses;
								driverNode.SundayEveningBottles = scheduleItem.EveningBottles;
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Обрабатывает события ТС для водителя - создает/обновляет CarEvent для подряд идущих одинаковых событий
		/// </summary>
		private void ProcessDriverCarEvents(DriverScheduleNode driverNode)
		{
			if(!driverNode.IsCarAssigned)
			{
				return;
			}

			var car = _carRepository.GetCarByDriverId(UoW, driverNode.DriverId);

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

				CreateOrUpdateCarEvent(car, driverNode, group);
			}
		}

		private List<CarEventGroup> GroupConsecutiveCarEvents(DriverScheduleNode driverNode)
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
							Comment = day.ParentNode.Comment,
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

		private void CreateOrUpdateCarEvent(Car car, DriverScheduleNode driverNode, CarEventGroup group)
		{
			if(group.CarEventType?.Id <= 0)
			{
				return;
			}

			var endOfDay = new DateTime(group.EndDate.Year, group.EndDate.Month, group.EndDate.Day, 23, 59, 59);

			var existingEvent = _logisticRepository.GetCarEventByCarId(UoW, car.Id, group, endOfDay);

			if(existingEvent == null)
			{
				var newEvent = new CarEvent
				{
					Car = car,
					CarEventType = group.CarEventType,
					Driver = UoW.GetById<Employee>(driverNode.DriverId),
					StartDate = group.StartDate,
					EndDate = group.EndDate.AddHours(23).AddMinutes(59).AddSeconds(59),
					Comment = group.Comment,
					Foundation = "Создано из графика водителей",
					CreateDate = DateTime.Now,
					Author = _employeeService.GetEmployeeForUser(UoW, _userService.CurrentUserId)
				};

				UoW.Session.Save(newEvent);
			}
			else
			{
				existingEvent.EndDate = endOfDay;
				existingEvent.Comment = group.Comment;
				UoW.Save(existingEvent);
			}
		}

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

		private void Export()
		{
			if(DriverScheduleRows == null || DriverScheduleRows.Count == 0)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Нет данных для экспорта");
				return;
			}

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"График_водителей_{StartDate:dd.MM.yyyy}_{EndDate:dd.MM.yyyy}.xlsx"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				try
				{
					using(var workbook = new XLWorkbook())
					{
						var worksheet = workbook.Worksheets.Add("График водителей");

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
							var date = StartDate.AddDays(dayIndex);
							int dayColumn = _firstDayColumn + dayIndex * _columnsPerDay;

							worksheet.Cell(row, dayColumn).Value = "'" + GetShortDayString(date);
							worksheet.Cell(row, dayColumn + 1).Value = "Адр У";
							worksheet.Cell(row, dayColumn + 2).Value = "Бут У";
							worksheet.Cell(row, dayColumn + 3).Value = "Адр В";
							worksheet.Cell(row, dayColumn + 4).Value = "Бут В";
						}

						worksheet.Cell(row, _commentColumn).Value = "Комментарий";

						row++;
						foreach(var node in DriverScheduleRows)
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

								worksheet.Cell(row, dayColumn).Value = day.CarEventType?.ShortName ?? "Нет";
								worksheet.Cell(row, dayColumn + 1).Value = day.MorningAddresses;
								worksheet.Cell(row, dayColumn + 2).Value = day.MorningBottles;
								worksheet.Cell(row, dayColumn + 3).Value = day.EveningAddresses;
								worksheet.Cell(row, dayColumn + 4).Value = day.EveningBottles;
							}

							worksheet.Cell(row, _commentColumn).Value = node.Comment ?? "";

							row++;
						}

						worksheet.Columns().AdjustToContents();
						workbook.SaveAs(result.Path);

						_interactiveService.ShowMessage(
							ImportanceLevel.Info,
							"Файл успешно сохранен");
					}
				}
				catch(Exception ex)
				{
					_interactiveService.ShowMessage(
						ImportanceLevel.Error,
						$"Ошибка при экспорте:\n{ex.Message}");
				}
			}
		}

		private void ShowInfoMessage()
		{
			var infoMessage =
				"Пояснения к столбцам:\n" +
				"\"П\" - принадлежность\n" +
				"\"Т\" - тип ТС\n" +
				"Принадлежность - принадлежность из карточки сотрудника\n" +
				"Дата посл. изм. - дата последнего изменения потенциала водителя\n" +
				"\n" +
				"Условные обозначения отчёта:\n" +
				"\"К\" - ТС компании\n" +
				"\"В\" - ТС водителя\n" +
				"\"Р\" - ТС в раскате\n" +
				"\"Л\" - Легковой (Ларгус)\n" +
				"\"Г\" - Грузовой (Газель)\n" +
				"\"Т\" - Фургон (Transit Mini)\n" +
				"\n" +
				"Логика работы со столбцами:\n" +
				"\n" +
				"1) После проставления чисел в столбцы Утро и Вечер (потенциала водителя) данные автоматически подтягиваются в столбцы Утро и Вечер, которые привязаны к дням недели.\n" +
				"\n" +
				"2) Если в столбце с днём недели проставлено любое событие, кроме \"Нет\" - в ячейках Утро и Вечер, привязанным к этому дню, автоматически выставляются значения, равные 0.\n" +
				"\n" +
				"3) Разрешено вручную редактировать данные ячейки только если статус выставлен \"Нет\". После редактирования ячеек, данные в них обновятся только после смены статуса, либо после изменения столбцов Утро и Вечер (которые идут до дней недели).\n" +
				"\n" +
				"4) Столбцы 8 и 9 изменяют остальные столбцы только на текущий день и далее, данные ПРОШЛЫХ дней не перезаписываются.\n" +
				"\n" +
				"График \"Отчёт по простою\" учитывает информацию о событиях из графика водителей.\n" +
				"\n" +
				"Если событие создано через Журнал событий (Например, ремонт) - оно добавляется в график водителей в проставленный период в сокращенном виде и его нельзя изменить.\n" +
				"\n" +
				"Вы можете перемещаться с помощью стрелочек на клавиатуре между ячейками для ввода данных." +
				"\n" +
				"Вертикальную прокрутку столбцов с днями недели можно выполнять с зажатой клавишей Shift.";

			_interactiveService.ShowMessage(ImportanceLevel.Info, infoMessage);
		}
	}
}
