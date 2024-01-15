using System.Collections.Generic;
using EventsApi.Library.Dtos;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Logistics;

namespace EventsApi.Library.Models
{
	public interface ILogisticsEventsModel
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
			EmployeeWithLogin employee);

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
		/// <returns>необходимые сведения о сотруднике</returns>
		EmployeeWithLogin GetEmployeeProxyByApiLogin(string userLogin);
	}
}
