using MoreLinq;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.ViewModels.Widgets.Cars.CarVersions
{
	public class CarVersionsManagementViewModel : WidgetViewModelBase, IDisposable
	{
		private readonly ICommonServices _commonServices;
		private readonly IRouteListRepository _routeListRepository;

		private Dictionary<int, (DateTime StartDate, DateTime? EndDate)> _carVersionPeriodsCache;
		private Car _car;
		private DialogViewModelBase _parentDialog;
		private CarVersion _editingCarVersion;

		public CarVersionsManagementViewModel(
			ICommonServices commonServices,
			IRouteListRepository routeListRepository,
			CarVersionsViewModel carVersionsViewModel,
			CarVersionEditingViewModel carVersionsEditingViewModel)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			CarVersionsViewModel = carVersionsViewModel ?? throw new ArgumentNullException(nameof(carVersionsViewModel));
			CarVersionEditingViewModel = carVersionsEditingViewModel ?? throw new ArgumentNullException(nameof(carVersionsEditingViewModel));

			CarVersionsViewModel.AddNewVersionClicked += OnAddNewVersionClicked;
			CarVersionsViewModel.ChangeStartDateClicked += OnEditStartDateClicked;
			CarVersionsViewModel.EditCarOwnerClicked += OnEditCarOwnerClicked;

			CarVersionEditingViewModel.SaveCarVersionClicked += OnSaveCarVersionClicked;
			CarVersionEditingViewModel.CancelEditingClicked += OnCancelEditingClicked;
		}

		public CarVersionsViewModel CarVersionsViewModel { get; }
		public CarVersionEditingViewModel CarVersionEditingViewModel { get; }

		public bool IsNewCar => !(Car is null) && Car.Id == 0;

		public virtual bool CanRead =>
			_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanRead;
		public virtual bool CanCreate =>
			(_commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Car)).CanCreate
			&& !(Car is null) && Car.Id == 0)
			|| _commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeCarVersion);
		public virtual bool CanEdit =>
			_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.LogisticPermissions.Car.CanChangeCarVersionDate);
		public bool IsInsuranceEditingInProgress => CarVersionEditingViewModel.IsWidgetVisible;

		public Car Car
		{
			get => _car;
			private set
			{
				if(!(_car is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(Car)} уже установлено");
				}

				SetField(ref _car, value);
				SetCarVersionPeriodsCache();
			}
		}

		public DialogViewModelBase ParentDialog
		{
			get => _parentDialog;
			private set
			{
				if(!(_parentDialog is null))
				{
					throw new InvalidOperationException($"Свойство {nameof(ParentDialog)} уже установлено");
				}

				SetField(ref _parentDialog, value);
			}
		}

		public bool CanAddNewVersion(DateTime? selectedDate) =>
			CanCreate
			&& selectedDate.HasValue
			&& !(Car is null)
			&& Car.CarVersions.All(x => x.Id != 0)
			&& !IsInsuranceEditingInProgress
			&& IsValidDateForNewCarVersion(selectedDate.Value);

		public bool CanChangeVersionDate(DateTime? selectedDate, CarVersion selectedCarVersion) =>
			selectedDate.HasValue
			&& selectedCarVersion != null
			&& (CanEdit || selectedCarVersion.Id == 0)
			&& !IsInsuranceEditingInProgress
			&& IsValidDateForVersionStartDateChange(selectedCarVersion, selectedDate.Value);

		public bool CanEditCarOwner(CarVersion selectedCarVersion) =>
			selectedCarVersion != null
			&& (CanEdit || selectedCarVersion.Id == 0)
			&& !IsInsuranceEditingInProgress;

		public void Initialize(Car car, DialogViewModelBase parentDialog)
		{
			Car = car ?? throw new ArgumentNullException(nameof(car));
			ParentDialog = parentDialog ?? throw new ArgumentNullException(nameof(parentDialog));

			InitializeCarVersionsViewModel();
			CarVersionEditingViewModel.ParentDialog = parentDialog;
		}

		private void InitializeCarVersionsViewModel()
		{
			CarVersionsViewModel.Initialize(
				CanAddNewVersion,
				CanChangeVersionDate,
				CanEditCarOwner,
				Car.Id == 0,
				CanRead);

			RefreshCarVersionsList();
		}

		private void SetCarVersionPeriodsCache()
		{
			_carVersionPeriodsCache = new Dictionary<int, (DateTime StartDate, DateTime? EndDate)>();
			foreach(var version in Car.CarVersions)
			{
				_carVersionPeriodsCache.Add(version.Id, (version.StartDate, version.EndDate));
			};
		}

		/// <summary>
		///  Создаёт и добавляет новую версию автомобиля в список версий.
		/// </summary>
		/// <param name="startDate">Дата начала действия новой версии. Если равно null, берётся текущая дата</param>
		private void CreateAndAddVersion(DateTime? startDate = null)
		{
			if(startDate == null)
			{
				startDate = DateTime.Now;
			}

			var availableOwnTypes = GetAvailableCarOwnTypesForVersion();
			var newVersion = new CarVersion
			{
				Car = Car,
				CarOwnType = availableOwnTypes.Any() ? availableOwnTypes.First() : CarOwnType.Company
			};

			AddNewVersion(newVersion, startDate.Value);
		}

		///  <summary>
		///  Добавляет новую версию автомобиля в список версий автомобиля контроллера.
		///  Если предыдущая версия не имела дату окончания или заканчивалась позже даты начала новой версии,
		///  то этой версии выставляется дата окончания, равная дате начала новой версии минус 1 миллисекунду
		///  </summary>
		///  <param name="newCarVersion">Новая версия автомобиля. Свойство StartDate в newCarVersion игнорируется</param>
		///  <param name="startDate">
		/// 	Дата начала действия новой версии. Должна быть минимум на день позже, чем дата начала действия предыдущей версии.
		/// 	Время должно равняться 00:00:00
		///  </param>
		private void AddNewVersion(CarVersion newCarVersion, DateTime startDate)
		{
			if(newCarVersion == null)
			{
				throw new ArgumentNullException(nameof(newCarVersion));
			}
			if(startDate.Date != startDate)
			{
				throw new ArgumentException("Время даты начала действия новой версии не равно 00:00:00", nameof(startDate));
			}
			if(newCarVersion.Car == null || newCarVersion.Car.Id != Car.Id)
			{
				newCarVersion.Car = Car;
			}

			newCarVersion.StartDate = startDate;
			newCarVersion.CarOwnerOrganization = GetPreviousVersionOrNull(newCarVersion)?.CarOwnerOrganization;
			CarVersionsViewModel.SelectedCarVersion = newCarVersion;
			EditCarVersion(newCarVersion);
			RefreshCarVersionsList();
		}

		private void ChangeVersionStartDate(CarVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.Car == null || version.Car.Id != Car.Id)
			{
				throw new ArgumentException("Неверно заполнен автомобиль в переданной версии");
			}

			var previousVersion = GetPreviousVersionOrNull(version);
			if(previousVersion != null)
			{
				var newEndDate = newStartDate.AddMilliseconds(-1);
				previousVersion.EndDate = newEndDate;
			}
			version.StartDate = newStartDate;
		}

		private void EditCarVersion(CarVersion selectedCarVersion)
		{
			var availableCarOwnTypes = new List<CarOwnType>();
			var previousVersion = GetPreviousVersionOrNull(selectedCarVersion);

			if(selectedCarVersion.Id > 0)
			{
				availableCarOwnTypes.Add(selectedCarVersion.CarOwnType);
			}
			else
			{
				availableCarOwnTypes =
					Car.ObservableCarVersions.Contains(selectedCarVersion)
					? GetAvailableCarOwnTypesForVersion(selectedCarVersion).ToList()
					: GetAvailableCarOwnTypesForVersion().ToList();
			}

			_editingCarVersion = selectedCarVersion;
			CarVersionEditingViewModel.SetWidgetProperties(selectedCarVersion, availableCarOwnTypes, previousVersion);
		}

		/// <summary>
		/// Возвращает список доступных принадлежностей авто для версии
		/// </summary>
		/// <param name="version">
		///	Редактируемая версия авто. Если не null, то выполняется проверка, что данная версия принадлежит авто
		/// </param>
		/// <returns>Список доступных принадлежностей авто</returns>
		private IList<CarOwnType> GetAvailableCarOwnTypesForVersion(CarVersion version = null)
		{
			if(version != null && !Car.CarVersions.Contains(version))
			{
				throw new InvalidOperationException("Переданная версия не была найдена в коллекции сущности");
			}

			var list = EnumHelper.GetValuesList<CarOwnType>();

			return list;
		}

		private bool IsValidDateForVersionStartDateChange(CarVersion version, DateTime newStartDate)
		{
			if(version == null)
			{
				throw new ArgumentNullException(nameof(version));
			}
			if(version.StartDate == newStartDate)
			{
				return false;
			}
			if(newStartDate >= version.EndDate)
			{
				return false;
			}
			var previousVersion = GetPreviousVersionOrNull(version);
			return previousVersion == null || newStartDate > previousVersion.StartDate;
		}

		private bool IsValidDateForNewCarVersion(DateTime dateTime)
		{
			return Car.CarVersions.All(x => x.StartDate < dateTime);
		}

		/// <summary>
		/// Возвращает список всех МЛ, затронутых добавлением/изменением даты версий авто
		/// </summary>
		public IList<RouteList> GetAllAffectedRouteLists(IUnitOfWork uow)
		{
			var periodsForRecalculation = GetPeriodsForRouteListsRecalculation();
			return periodsForRecalculation.Any()
				? _routeListRepository.GetRouteListsForCarInPeriods(uow, Car.Id, periodsForRecalculation)
				: new List<RouteList>();
		}

		private IList<(DateTime StartDate, DateTime? EndDate)> GetPeriodsForRouteListsRecalculation()
		{
			IList<(DateTime StartDate, DateTime? EndDate)> periods = new List<(DateTime StartDate, DateTime? EndDate)>();

			foreach(var version in Car.CarVersions)
			{
				if(version.Id != 0)
				{
					var oldVersionNode = _carVersionPeriodsCache[version.Id];
					if(oldVersionNode.StartDate > version.StartDate)
					{
						periods.Add((version.StartDate, oldVersionNode.StartDate.AddDays(-1)));
					}
					else if(oldVersionNode.StartDate < version.StartDate)
					{
						periods.Add((oldVersionNode.StartDate, version.StartDate.AddDays(-1)));
					}
				}
				else
				{
					periods.Add((version.StartDate, null));
				}
			}
			foreach(var period in periods.ToList())
			{
				var overlapping = periods.Where(x => x != period && x.StartDate > period.StartDate && x.StartDate < period.EndDate).ToList();
				overlapping.Add(period);
				foreach(var o in overlapping)
				{
					periods.Remove(o);
				}
				periods.Add((overlapping.Min(x => x.StartDate), overlapping.Max(x => x.EndDate)));
			}
			return periods;
		}

		private CarVersion GetPreviousVersionOrNull(CarVersion currentVersion)
		{
			return Car.CarVersions
				.Where(x => x.StartDate < currentVersion.StartDate)
				.OrderByDescending(x => x.StartDate)
				.FirstOrDefault();
		}

		private void RefreshCarVersionsList()
		{
			CarVersionsViewModel.CarVersions = Car.ObservableCarVersions;
		}

		private void InsertEditingCarVersionIntoCarVersionsList()
		{
			if(_editingCarVersion is null)
			{
				return;
			}

			var startDate = _editingCarVersion.StartDate;

			if(Car.CarVersions.Any())
			{
				var currentLatestVersion = Car.CarVersions.MaxBy(x => x.StartDate).First();

				if(_editingCarVersion.Id == 0
					&& _editingCarVersion.CarOwnerOrganization?.Id == currentLatestVersion.CarOwnerOrganization?.Id
					&& _editingCarVersion.CarOwnType == currentLatestVersion.CarOwnType)
				{
					throw new ArgumentException(
						"В новой версии должен отличаться либо собственник авто, либо принадлежность авто",
						nameof(_editingCarVersion.CarOwnerOrganization));
				}

				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(_editingCarVersion.StartDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			Car.ObservableCarVersions.Insert(0, _editingCarVersion);
		}

		private void OnAddNewVersionClicked(object sender, AddNewVersionEventArgs e)
		{
			var startDate = e.StartDateTime;
			CreateAndAddVersion(startDate);
		}

		private void OnEditStartDateClicked(object sender, ChangeStartDateEventArgs e)
		{
			ChangeVersionStartDate(e.CarVersion, e.StartDate);
		}

		private void OnEditCarOwnerClicked(object sender, EditCarOwnerEventArgs e)
		{
			var selectedCarVersion = e.CarVersion;
			if(selectedCarVersion is null)
			{
				return;
			}

			EditCarVersion(selectedCarVersion);
		}

		private void OnSaveCarVersionClicked(object sender, EventArgs e)
		{
			if(_editingCarVersion is null)
			{
				return;
			}

			if(_editingCarVersion.Id == 0 && !Car.CarVersions.Any(v => v.Id == 0))
			{
				InsertEditingCarVersionIntoCarVersionsList();
			}

			_editingCarVersion = null;
			CarVersionsViewModel.UpdateAccessibilityProperties();
		}

		private void OnCancelEditingClicked(object sender, EventArgs e)
		{
			_editingCarVersion = null;
			CarVersionsViewModel.UpdateAccessibilityProperties();
			RefreshCarVersionsList();
		}

		public void Dispose()
		{
			if(!(CarVersionsViewModel is null))
			{
				CarVersionsViewModel.AddNewVersionClicked -= OnAddNewVersionClicked;
				CarVersionsViewModel.ChangeStartDateClicked -= OnEditStartDateClicked;
				CarVersionsViewModel.EditCarOwnerClicked -= OnEditCarOwnerClicked;
			}

			if(!(CarVersionEditingViewModel is null))
			{
				CarVersionEditingViewModel.SaveCarVersionClicked -= OnSaveCarVersionClicked;
				CarVersionEditingViewModel.CancelEditingClicked -= OnCancelEditingClicked;
			}
		}
	}
}
