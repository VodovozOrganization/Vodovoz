namespace VodovozBusiness.Services.Orders
{
	public partial class CreateOrderRequest
	{
		public class SaleItem
		{
			public int NomenclatureId { get; }
			public int BottlesCount { get; }

			public SaleItem(int nomenclatureId, int bottlesCount)
			{
				NomenclatureId = nomenclatureId;
				BottlesCount = bottlesCount;
			}
		}
	}
}
