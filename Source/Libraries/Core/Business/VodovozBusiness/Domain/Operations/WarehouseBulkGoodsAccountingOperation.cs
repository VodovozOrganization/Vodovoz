using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Operations
{
	/// <summary>
	/// Операция передвижения товаров по складу(объемный учет)
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "операции передвижения товаров по складу(объемный учет)",
		Nominative = "операция передвижения товаров по складу(объемный учет)")]
	public class WarehouseBulkGoodsAccountingOperation : BulkGoodsAccountingOperation
	{
		private Warehouse _warehouse;

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => SetField(ref _warehouse, value);
		}

		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		[Display(Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}

		public override OperationType OperationType => OperationType.WarehouseBulkGoodsAccountingOperation;
	}
}

