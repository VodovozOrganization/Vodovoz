using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Common;

namespace Vodovoz.Services.Logistics
{
	/// <summary>Проверка и ручное добавление водителей в стоп-лист.</summary>
	public class DriverStopListService : IDriverStopListService
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IGeneralSettings _generalSettings;

		/// <summary>Создаёт сервис стоп-листов водителей.</summary>
		/// <param name="routeListRepository">Репозиторий маршрутных листов.</param>
		/// <param name="generalSettings">Настройки порогов задолженности.</param>
		public DriverStopListService(IRouteListRepository routeListRepository, IGeneralSettings generalSettings)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_generalSettings = generalSettings ?? throw new ArgumentNullException(nameof(generalSettings));
		}

		/// <summary>Блокирует водителя до ближайшей полуночи, прекращая действующие снятия. Фиксацию выполняет вызывающий код.</summary>
		/// <param name="uow">Единица работы для сохранения изменений.</param>
		/// <param name="driver">Водитель.</param>
		public void AddDriverToStopList(IUnitOfWork uow, Employee driver)
		{
			var now = DateTime.Now;
			foreach(var removal in _routeListRepository.GetActiveDriverStopListRemovals(uow, driver.Id, now))
			{
				removal.DateTo = now;
				uow.Save(removal);
			}

			driver.DriverManualStopListUntil = now.Date.AddDays(1);
			uow.Save(driver);
		}

		/// <summary>Проверяет статус по правилам журнала с учётом ручной блокировки и временного снятия.</summary>
		/// <param name="uow">Единица работы.</param>
		/// <param name="driver">Водитель; отсутствие водителя не блокирует выбор.</param>
		/// <param name="routeListId">Маршрутный лист, исключаемый из расчёта долгов.</param>
		/// <returns>Водитель находится в стоп-листе.</returns>
		public bool IsDriverInStopList(IUnitOfWork uow, Employee driver, int routeListId)
		{
			if(driver == null || driver.IsDriverHasActiveStopListRemoval(uow))
			{
				return false;
			}

			var count = _routeListRepository.GetUnclosedRouteListsCountHavingDebtByDriver(uow, driver.Id, routeListId);
			var sum = _routeListRepository.GetUnclosedRouteListsDebtsSumByDriver(uow, driver.Id, routeListId);
			var maxCount = _generalSettings.DriversUnclosedRouteListsHavingDebtMaxCount;
			var maxSum = _generalSettings.DriversRouteListsMaxDebtSum;

			return driver.DriverManualStopListUntil > DateTime.Now
				|| (maxCount > 0 && count >= maxCount)
				|| (maxSum > 0 && sum >= maxSum);
		}
	}
}
