using DriverAPI.DTOs.V4;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Library.Models
{
	public interface IDriverWarehouseEventsModel
	{
		/// <summary>
		/// Завершение события нахождения на складе
		/// </summary>
		/// <returns></returns>
		void CompleteDriverWarehouseEvent(DriverWarehouseEventData eventData, Employee driver);
	}
}
