using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные множества продуктовых лимитов
	/// </summary>
	public class FuelLimitsDataDto
	{
		/// <summary>
		/// Количество лимитов
		/// </summary>
		[JsonPropertyName("total_count")]
		public int FuelLimitsCount { get; set; }

		/// <summary>
		/// Продуктовые лимиты
		/// </summary>
		[JsonPropertyName("result")]
		public IEnumerable<FuelLimitResponseDto> FuelLimits { get; set; }
	}
}
