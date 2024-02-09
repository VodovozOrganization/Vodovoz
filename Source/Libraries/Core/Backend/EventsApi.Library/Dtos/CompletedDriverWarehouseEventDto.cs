using System;

namespace EventsApi.Library.Dtos
{
	/// <summary>
	/// Даннные о завершеном событии
	/// </summary>
	public class CompletedDriverWarehouseEventDto
	{
		/// <summary>
		/// Название события, которое было завершено
		/// </summary>
		public string EventName { get; set; }
		/// <summary>
		/// Время в Utc формате
		/// </summary>
		public DateTime CompletedDate { get; set; }
		/// <summary>
		/// Фамилия и инициалы сотрудника, завершившего событие
		/// </summary>
		public string EmployeeName { get; set; }
	}
}
