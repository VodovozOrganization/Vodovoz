using System;

namespace LogisticsEventsApi.Contracts
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
		/// Время события в Utc формате
		/// </summary>
		public DateTime CompletedDate { get; set; }
		/// <summary>
		/// Фамилия и инициалы сотрудника, завершившего событие
		/// </summary>
		public string EmployeeName { get; set; }
	}
}
