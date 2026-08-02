using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.V2.Dto
{
	/// <summary>
	/// Данные по пакету аренды
	/// </summary>
	public class FreeRentPackageDto
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Наименование для ИПЗ
		/// </summary>
		public string OnlineName { get; set; }
		/// <summary>
		/// Доступность для продажи
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? OnlineAvailability { get; set; }
		/// <summary>
		/// Минимальное кол-во воды
		/// </summary>
		public int MinWaterAmount { get; set; }
	}
}
