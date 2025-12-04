using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Nodes
{
	public class FreeRentPackageWithOnlineParametersNode
	{
		public int Id { get; set; }
		public string OnlineName { get; set; }
		public GoodsOnlineAvailability OnlineAvailability { get; set; }
		public int MinWaterAmount { get; set; }
		public decimal Deposit { get; set; }
		public int DepositServiceId { get; set; }
	}
}
