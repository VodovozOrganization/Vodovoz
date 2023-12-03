using DriverAPI.DTOs.V4;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Drivers;

namespace DriverAPI.Library.Models
{
	public interface IDriverWarehouseEventsModel
	{
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		CompletedDriverWarehouseEvent CompleteDriverWarehouseEvent(DriverWarehouseEventData eventData, Employee driver);
	}
}
