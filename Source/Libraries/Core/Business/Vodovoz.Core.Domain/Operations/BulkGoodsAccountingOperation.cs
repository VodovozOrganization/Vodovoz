using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров(объемный учет)",
		Nominative = "операция передвижение товаров(объемный учет)")]
	public abstract class BulkGoodsAccountingOperation : GoodsAccountingOperation
	{
		
	}
}

