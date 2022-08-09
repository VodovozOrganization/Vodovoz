namespace Vodovoz.EntityRepositories.Roboats
{
	public class NomenclatureQuantity
	{
		public int NomenclatureId { get; }
		public int Quantity { get; }

		public NomenclatureQuantity(int nomenclatureId, int quantity)
		{
			NomenclatureId = nomenclatureId;
			Quantity = quantity;
		}
	}
}
