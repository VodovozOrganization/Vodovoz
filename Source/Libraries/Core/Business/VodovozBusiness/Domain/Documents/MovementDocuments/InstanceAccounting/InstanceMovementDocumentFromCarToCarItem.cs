﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения с автомобиля на автомобиль(экземплярный учет)",
		Nominative = "строка перемещения с автомобиля на автомобиль(экземплярный учет)")]
	public class InstanceMovementDocumentFromCarToCarItem : InstanceMovementDocumentToCarItem
	{
		public override MovementDocumentItemType MovementDocumentItemType =>
			MovementDocumentItemType.InstanceMovementDocumentFromCarToCarItem;
		
		public virtual CarInstanceGoodsAccountingOperation WriteOffCarInstanceGoodsAccountingOperation
		{
			get => WriteOffOperation as CarInstanceGoodsAccountingOperation;
			set => WriteOffOperation = value;
		}
		
		protected override void CreateOrUpdateWriteOffOperation()
		{
			if(WriteOffOperation is null)
			{
				WriteOffCarInstanceGoodsAccountingOperation = new CarInstanceGoodsAccountingOperation();
			}

			WriteOffCarInstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			WriteOffCarInstanceGoodsAccountingOperation.Car = Document.FromCar;
		}
	}
}
