namespace Vodovoz.Core.Data.Goods
{
	public class Nomenclature
	{
		public int Id { get; set; }
		public string OfficialName { get; set; }
		public string Gtin { get; set; }
		public NomenclatureCategory Category { get; set; }
		public MeasurementUnit MeasurementUnit { get; set; }
	}
}
