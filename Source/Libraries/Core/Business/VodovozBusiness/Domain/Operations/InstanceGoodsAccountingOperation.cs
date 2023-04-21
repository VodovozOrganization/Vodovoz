using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class InstanceGoodsAccountingOperation : GoodsAccountingOperation
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;
		
		[Display(Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}
		
		public override GoodsAccountingOperationType GoodsAccountingOperationType =>
			GoodsAccountingOperationType.InstanceGoodsAccountingOperation;
	}
}

