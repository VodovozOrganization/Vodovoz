using System.Text.Json.Serialization;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerAppsApi.Library.Dto
{
	public class FreeRentPackageDto
	{
		public int ErpId { get; set; }
		public string OnlineName { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public GoodsOnlineAvailability? OnlineAvailability { get; set; }
		public int MinWaterAmount { get; set; }
		public decimal Deposit { get; set; }
		public int DepositServiceId { get; set; }
	}
}
