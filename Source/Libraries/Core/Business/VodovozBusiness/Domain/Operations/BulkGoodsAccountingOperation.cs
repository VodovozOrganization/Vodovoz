using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public abstract class BulkGoodsAccountingOperation : GoodsAccountingOperation
	{
		
	}
}

