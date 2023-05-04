using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания с автомобиля(объемный учет)",
		Nominative = "строка списания с автомобиля(объемный учет)")]
	[HistoryTrace]
	public class BulkWriteOffFromCarDocumentItem : BulkWriteOffDocumentItem
	{
		public override WriteOffDocumentItemType Type => WriteOffDocumentItemType.BulkWriteOffFromCarDocumentItem;

		public virtual CarBulkGoodsAccountingOperation CarBulkGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as CarBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Car car, DateTime time)
		{
			CarBulkGoodsAccountingOperation = new CarBulkGoodsAccountingOperation
			{
				Car = car,
				OperationTime = time,
			};
			
			FillOperation();
		}

		#endregion
	}
}

