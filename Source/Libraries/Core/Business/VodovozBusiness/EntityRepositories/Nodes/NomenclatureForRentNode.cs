namespace Vodovoz.EntityRepositories.Nodes
{
	public class NomenclatureForRentNode
	{
		public int Id { get; set; }
		public string NomenclatureName { get; set; }
		public int TypeId { get; set; }
		public string EquipmentKindName { get; set; }
		public decimal InStock { get; set; }
		public int? Reserved { get; set; }
		public decimal Available => InStock - Reserved.GetValueOrDefault();
		public string UnitName { get; set; }
		public short UnitDigits { get; set; }
		private string Format(decimal value)
		{
			return string.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);
		}
		public string InStockText => Format(InStock);
		public string ReservedText => Format(Reserved.GetValueOrDefault());
		public string AvailableText => Format(Available);
	}
}
