using System;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	public class WarehouseBulkInventoryDocumentItem : InventoryDocumentItem
	{
		public override InventoryDocumentType Type => InventoryDocumentType.WarehouseInventory;

		protected override void CreateOperation(DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = Document.Warehouse
			};
			FillOperation(time);
		}

		protected override void UpdateOperation()
		{
			WarehouseBulkChangeOperation.Warehouse = Document.Warehouse;
			base.UpdateOperation();
		}

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseBulkChangeOperation
		{
			get => GoodsAccountingOperation as WarehouseBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}
	}
}
