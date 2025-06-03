using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Responses.V1
{
	/// <summary>
	/// Контрагент
	/// </summary>
	public class CounterpartyDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		[JsonPropertyName("id")]
		public int Id { get; set; }

		/// <summary>
		/// Тип
		/// </summary>
		[JsonPropertyName("type")]
		public PersonType Type { get; set; }

		/// <summary>
		/// Фамилия Имя Отчетство
		/// </summary>
		[JsonPropertyName("fio")]
		public string Fio { get; set; }

		/// <summary>
		/// ИНН
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// Точки доставки
		/// </summary>
		[JsonPropertyName("delivery_points")]
		public IEnumerable<DeliveryPointDto> DeliveryPoints { get; set; }
	}
}
