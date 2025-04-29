using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные множества товарных ограничителей по карте
	/// </summary>
	public class FuelCardProductRestrictionsDataDto
	{
		/// <summary>
		/// Количество товарных ограничителей
		/// </summary>
		[JsonPropertyName("total_count")]
		public int FuelCardProductRestrictionsCount { get; set; }

		/// <summary>
		/// Товарные ограничители
		/// </summary>
		[JsonPropertyName("result")]
		public IEnumerable<FuelCardProductRestrictionDto> FuelCardProductRestrictions { get; set; }
	}
}
