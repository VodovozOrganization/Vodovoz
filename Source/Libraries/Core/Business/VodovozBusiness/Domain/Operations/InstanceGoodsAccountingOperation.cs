using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров(экземплярный учет)",
		Nominative = "операция передвижения товаров(экземплярный учет)")]
	public abstract class InstanceGoodsAccountingOperation : GoodsAccountingOperation
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;
		
		[Display(Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}
	}
}

