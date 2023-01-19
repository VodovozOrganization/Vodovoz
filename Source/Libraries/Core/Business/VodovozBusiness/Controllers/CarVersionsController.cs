using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using QS.DomainModel.UoW;
using QS.Utilities.Enums;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Controllers
{
	public class CarVersionsController : ICarVersionsController
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly Dictionary<int, (DateTime StartDate, DateTime? EndDate)> _carVersionPeriodsCache;

		public CarVersionsController(Car car, IRouteListRepository routeListRepository)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			Car = car ?? throw new ArgumentNullException(nameof(car));

			_carVersionPeriodsCache = new Dictionary<int, (DateTime StartDate, DateTime? EndDate)>();
			foreach(var version in Car.CarVersions)
			{
				_carVersionPeriodsCache.Add(version.Id, (version.StartDate, version.EndDate));
			}
		}

		public Car Car { get; }

		/// <summary>
		///  Создаёт и добавляет новую версию автомобиля в список версий.
		/// </summary>
		/// <param name="startDate">Дата начала действия новой версии. Если равно null, берётся текущая дата</param>
		public void CreateAndAddVersion(DateTime? startDate = null)
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
		public void AddNewVersion(CarVersion newCarVersion, DateTime startDate)
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

			if(Car.CarVersions.Any())
			{
				var currentLatestVersion = Car.CarVersions.MaxBy(x => x.StartDate).First();
				if(startDate < currentLatestVersion.StartDate.AddDays(1))
				{
					throw new ArgumentException(
						"Дата начала действия новой версии должна быть минимум на день позже, чем дата начала действия предыдущей версии",
						nameof(startDate));
				}
				currentLatestVersion.EndDate = startDate.AddMilliseconds(-1);
			}

			newCarVersion.StartDate = startDate;
			Car.ObservableCarVersions.Insert(0, newCarVersion);
		}

		/// <summary>
		/// Добавляет новую версию автомобиля в список версий c датой начала действия равной дате начала действия предыдущей версии плюс 1 день
		/// </summary>
		/// <param name="newCarVersion">Новая версия автомобиля. Свойство StartDate в newCarVersion игнорируется</param>
		public void AddNewVersionOnMinimumPossibleDate(CarVersion newCarVersion)
		{
			var startDate = default(DateTime);
			if(Car.CarVersions.Any())
			{
				startDate = Car.CarVersions.Max(x => x.StartDate).AddDays(1);
			}
			AddNewVersion(newCarVersion, startDate);
		}

		public void ChangeVersionStartDate(CarVersion version, DateTime newStartDate)
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

		/// <summary>
		/// Возвращает список доступных принадлежностей для версии, основываясь на более старой версии отностиельно переданной
		/// </summary>
		/// <param name="version">
		///		Если не равна null, то версия обязательно должна быть добавлена в коллекцию сущности.<br/>
		///		Если равна null, то подбирает доступные принадлежности как для новой версии
		/// </param>
		/// <returns>Список доступных принадлежностей</returns>
		public IList<CarOwnType> GetAvailableCarOwnTypesForVersion(CarVersion version = null)
		{
			if(version != null && !Car.CarVersions.Contains(version))
			{
				throw new InvalidOperationException("Переданная версия не была найдена в коллекции сущности");
			}

			var list = EnumHelper.GetValuesList<CarOwnType>();
			if(!Car.CarVersions.Any())
			{
				return list;
			}
			if(version == null)
			{
				list.Remove(Car.CarVersions.Single(x => x.EndDate == null).CarOwnType);
			}
			else
			{
				var previousVersion = GetPreviousVersionOrNull(version);
				if(previousVersion != null)
				{
					list.Remove(previousVersion.CarOwnType);
				}
			}
			return list;
		}

		public bool IsValidDateForVersionStartDateChange(CarVersion version, DateTime newStartDate)
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

		public bool IsValidDateForNewCarVersion(DateTime dateTime)
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
	}
}
