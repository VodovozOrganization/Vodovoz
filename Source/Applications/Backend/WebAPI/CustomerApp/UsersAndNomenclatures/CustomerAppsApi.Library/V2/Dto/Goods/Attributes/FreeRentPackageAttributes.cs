using System.Collections.Generic;

namespace CustomerAppsApi.Library.V2.Dto.Goods.Attributes
{
	/// <summary>
	/// Характеристики пакета аренды
	/// </summary>
	public class FreeRentPackageAttributes
	{
		/// <summary>
		/// Минимальное количество воды
		/// </summary>
		public int MinWaterAmount { get; set; }
		/// <summary>
		/// Идентификаторы доступной воды
		/// </summary>
		public IEnumerable<int> AvailableWaterIds { get; set; }
	}
}
