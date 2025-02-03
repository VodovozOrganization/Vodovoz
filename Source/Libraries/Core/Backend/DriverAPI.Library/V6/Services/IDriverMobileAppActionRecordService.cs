using DriverApi.Contracts.V6;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Интерфейс для записи действий в мобильном приложении водителя
	/// </summary>
	public interface IDriverMobileAppActionRecordService
	{
		/// <summary>
		/// Регистрация действия водителя
		/// </summary>
		/// <param name="driver">Водитель</param>
		/// <param name="driverAction">Действие водителя</param>
		void RegisterAction(Employee driver, DriverActionDto driverAction);

		/// <summary>
		/// Регистрация действия водителя
		/// </summary>
		/// <param name="driverId">Идентификатор водителя</param>
		/// <param name="actionType">Тип действия</param>
		/// <param name="actionTime">Время действия</param>
		/// <param name="recievedTime">Время получения</param>
		/// <param name="result">Результат</param>
		void RegisterAction(int driverId, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result);

		/// <summary>
		/// Регистрация действия водителя
		/// </summary>
		/// <param name="driver">Водитель</param>
		/// <param name="actionType">Тип действия</param>
		/// <param name="actionTime">Время действия</param>
		/// <param name="recievedTime">Время получения</param>
		/// <param name="result">Результат</param>
		void RegisterAction(Employee driver, DriverMobileAppActionType actionType, DateTime actionTime, DateTime recievedTime, string result);

		/// <summary>
		/// Регистрация диапазона действий водителя
		/// </summary>
		/// <param name="driver">Водитель</param>
		/// <param name="driverActions">Действия водителя</param>
		void RegisterActionsRangeForDriver(Employee driver, IEnumerable<DriverActionDto> driverActions);
	}
}
