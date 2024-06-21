using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные множества топливных карт
	/// </summary>
	public class FuelCardsDataDto
	{
		/// <summary>
		/// Количество топливных карт
		/// </summary>
		[JsonPropertyName("total_count")]
		public int FuelCardsCount { get; set; }

		/// <summary>
		/// Топливные карты
		/// </summary>
		[JsonPropertyName("result")]
		public IEnumerable<FuelCardDto> FuelCards { get; set; }
	}
}
