using System;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.InventoryDocuments
{
	public class CarBulkInventoryDocumentItem : InventoryDocumentItem
	{
		public override InventoryDocumentType Type => InventoryDocumentType.CarInventory;

		protected override void CreateOperation(DateTime time)
		{
			GoodsAccountingOperation = new CarBulkGoodsAccountingOperation
			{
				Car = Document.Car
			};
			FillOperation(time);
		}
		
		protected override void UpdateOperation()
		{
			CarBulkChangeOperation.Car = Document.Car;
			base.UpdateOperation();
		}
		
		public virtual CarBulkGoodsAccountingOperation CarBulkChangeOperation
		{
			get => GoodsAccountingOperation as CarBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}
	}
}
