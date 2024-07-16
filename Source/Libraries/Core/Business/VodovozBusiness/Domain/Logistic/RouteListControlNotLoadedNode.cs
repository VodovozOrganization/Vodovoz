using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListControlNotLoadedNode
	{
		public int NomenclatureId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public decimal CountNotLoaded { get; set; }
		public decimal CountTotal { get; set; }
		public decimal CountLoaded => CountTotal - CountNotLoaded;
	}
}
