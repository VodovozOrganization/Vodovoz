using System;
using System.Text.Json.Serialization;
using EventsApi.Library.Converters;

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
		/// Время события
		/// </summary>
		[JsonConverter(typeof(DateTimeJsonConverter))]
		public DateTime CompletedDate { get; set; }
		/// <summary>
		/// Фамилия и инициалы сотрудника, завершившего событие
		/// </summary>
		public string EmployeeName { get; set; }
	}
}
