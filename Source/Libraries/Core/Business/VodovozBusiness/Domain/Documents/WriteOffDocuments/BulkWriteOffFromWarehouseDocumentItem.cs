using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания со склада(объемный учет)",
		Nominative = "строка списания со склада(объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffFromWarehouseDocumentItem : BulkWriteOffDocumentItem
	{
		public override WriteOffDocumentItemType Type => WriteOffDocumentItemType.BulkWriteOffFromWarehouseDocumentItem;

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseBulkGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as WarehouseBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			WarehouseBulkGoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
			{
				Warehouse = warehouse,
				OperationTime = time,
			};
			
			FillOperation();
		}

		#endregion
	}
}

