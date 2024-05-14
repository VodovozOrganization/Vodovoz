using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Длительность, период времени лимита
	/// </summary>
	public class LimitTimePeriodResponseDto
	{
		/// <summary>
		/// Период действия ограничения.
		/// 2 – Разовый,
		/// 3 – Сутки,
		/// 4 – Неделя,
		/// 5 – Месяц, 
		/// 6 – Квартал, 
		/// 7 – Год
		/// </summary>
		[JsonPropertyName("type")]
		public int Type { get; set; }

		/// <summary>
		/// Значение.
		/// Например, при выборе периода "Неделя" (Type=3) и значении "2" (Number=2), лимит будет действовать 2 недели
		/// </summary>
		[JsonPropertyName("number")]
		public int Number { get; set; }
	}
}
