using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания со склада(экземплярный учет)",
		Nominative = "строка списания со склада(экземплярный учет)")]
	[HistoryTrace]
	public class InstanceWriteOffFromWarehouseDocumentItem : InstanceWriteOffDocumentItem
	{
		public override WriteOffDocumentItemType Type => WriteOffDocumentItemType.InstanceWriteOffFromWarehouseDocumentItem;

		public virtual WarehouseInstanceGoodsAccountingOperation WarehouseInstanceGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as WarehouseInstanceGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			WarehouseInstanceGoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation
			{
				Warehouse = warehouse,
				OperationTime = time,
			};

			WarehouseInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			FillOperation();
		}

		#endregion
	}
}

