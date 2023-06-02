using System;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания с автомобиля(экземплярный учет)",
		Nominative = "строка списания с автомобиля(экземплярный учет)")]
	[HistoryTrace]
	public class InstanceWriteOffFromCarDocumentItem : InstanceWriteOffDocumentItem
	{
		public override WriteOffDocumentItemType Type => WriteOffDocumentItemType.InstanceWriteOffFromCarDocumentItem;

		public virtual CarInstanceGoodsAccountingOperation CarInstanceGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as CarInstanceGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		#region Функции

		public virtual void CreateOperation(Car car, DateTime time)
		{
			CarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation
			{
				Car = car,
				OperationTime = time,
			};
			
			CarInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			FillOperation();
		}

		#endregion
	}
}

