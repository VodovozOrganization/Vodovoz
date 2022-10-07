namespace Vodovoz.Models.Orders
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
