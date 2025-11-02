using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureAmountNode
	{
		public int NomenclatureId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public decimal Amount { get; set; }
	}
}
