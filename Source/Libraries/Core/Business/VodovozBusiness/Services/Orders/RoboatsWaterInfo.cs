namespace VodovozBusiness.Services.Orders
{
	public partial class CreateOrderRequest
	{
		public class RoboatsWaterInfo
		{
			public int NomenclatureId { get; }
			public int BottlesCount { get; }

			public RoboatsWaterInfo(int nomenclatureId, int bottlesCount)
			{
				NomenclatureId = nomenclatureId;
				BottlesCount = bottlesCount;
			}
		}
	}
}
