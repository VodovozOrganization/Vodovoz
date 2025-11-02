using LogisticsEventsApi.Contracts;
using System.Collections.Generic;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Logistics;
using Vodovoz.Core.Domain.Employees;

namespace EventsApi.Library.Models
{
	public interface ILogisticsEventsService
	{
		/// <summary>
		/// Конвертация и проверка данных из Qr кода на корректность
		/// </summary>
		/// <param name="qrData">данные Qr кода</param>
		/// <returns></returns>
		DriverWarehouseEventQrData ConvertAndValidateQrData(string qrData);

		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		CompletedDriverWarehouseEventProxy CompleteDriverWarehouseEvent(
			DriverWarehouseEventQrData qrData,
			DriverWarehouseEventData eventData,
			EmployeeWithLogin employee,
			out int distanceMetersFromScanningLocation);

		/// <summary>
		/// Завершение события нахождения на складе для событий без координат
		/// </summary>
		/// <returns></returns>
		public CompletedDriverWarehouseEventProxy CompleteWarehouseEventWithoutCoordinates(
			DriverWarehouseEventQrData qrData,
			EmployeeWithLogin employee,
			out int distanceMetersFromScanningLocation);

		/// <summary>
		/// Получение списка завершенных событий за текущий день для сотрудника
		/// </summary>
		/// <param name="employee">сотрудник</param>
		/// <returns>список завершенных событий за день</returns>
		IEnumerable<CompletedEventDto> GetTodayCompletedEvents(EmployeeWithLogin employee);

		/// <summary>
		/// Получение прокси сотрудника, содержащем необходимые сведения
		/// </summary>
		/// <param name="userLogin">логин сотрудника</param>
		/// <param name="applicationType">тип приложения</param>
		/// <returns>необходимые сведения о сотруднике</returns>
		EmployeeWithLogin GetEmployeeProxyByApiLogin(
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp);
	}
}
