using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	//TODO поправить класс
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения")]
	[HistoryTrace]
	public class MovementDocumentItem : PropertyChangedBase, IDomainObject
	{
		private Nomenclature _nomenclature;
		private decimal _sentAmount;
		private decimal _receivedAmount;
		private decimal _amountOnSource = 99999999;
		private GoodsAccountingOperation _warehouseWriteoffOperation;
		private GoodsAccountingOperation _incomeOperation;

		public virtual int Id { get; set; }

		public virtual MovementDocument Document { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Отправлено")]
		public virtual decimal SentAmount
		{
			get => _sentAmount;
			set => SetField(ref _sentAmount, value);
		}

		[Display(Name = "Принято")]
		public virtual decimal ReceivedAmount
		{
			get => _receivedAmount;
			set => SetField(ref _receivedAmount, value);
		}

		[Display(Name = "Имеется на складе")]
		public virtual decimal AmountOnSource
		{
			get => _amountOnSource;
			set => SetField(ref _amountOnSource, value);
		}

		public virtual GoodsAccountingOperation WarehouseWriteoffOperation
		{
			get => _warehouseWriteoffOperation;
			set => SetField(ref _warehouseWriteoffOperation, value);
		}

		public virtual GoodsAccountingOperation WarehouseIncomeOperation
		{
			get => _incomeOperation;
			set => SetField(ref _incomeOperation, value);
		}

		#region Функции

		public virtual string Title =>
			String.Format("[{2}] {0} - {1}",
				Document.Title,
				Nomenclature.Name,
				Nomenclature.Unit.MakeAmountShortStr(SentAmount));

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";

		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual bool HasDiscrepancy => SentAmount != ReceivedAmount;

		public virtual void UpdateWriteoffOperation()
		{
			if(Document == null) {
				throw new InvalidOperationException("Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(Document.Status == MovementDocumentStatus.Sended)
			{
				if(WarehouseWriteoffOperation == null)
				{
					WarehouseWriteoffOperation = new GoodsAccountingOperation();
				}

				//WarehouseWriteoffOperation.WriteOffWarehouse = Document.FromWarehouse;
				//WarehouseWriteoffOperation.IncomingWarehouse = null;
				//Предполагается что если документ находиться в статусе отправлен, то время доставки обязательно установлено
				WarehouseWriteoffOperation.OperationTime = Document.SendTime.Value;
				WarehouseWriteoffOperation.Nomenclature = Nomenclature;
				WarehouseWriteoffOperation.Amount = SentAmount;
				return;
			}

			if(Document.IsDelivered)
			{
				if(WarehouseWriteoffOperation == null)
				{
					WarehouseWriteoffOperation = new GoodsAccountingOperation();
				}

				//WarehouseWriteoffOperation.WriteOffWarehouse = Document.FromWarehouse;
				//WarehouseWriteoffOperation.IncomingWarehouse = null;
				//Предполагается что если документ доставлен, то время доставки обязательно установлено
				WarehouseWriteoffOperation.OperationTime = Document.SendTime.Value;
				WarehouseWriteoffOperation.Nomenclature = Nomenclature;
				WarehouseWriteoffOperation.Amount = ReceivedAmount;
			}
		}

		public virtual void UpdateIncomeOperation()
		{
			if(Document == null)
			{
				throw new InvalidOperationException("Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(!Document.IsDelivered)
			{
				WarehouseIncomeOperation = null;
				return;
			}

			if(WarehouseIncomeOperation == null)
			{
				WarehouseIncomeOperation = new GoodsAccountingOperation();
			}

			//WarehouseIncomeOperation.WriteOffWarehouse = null;
			//WarehouseIncomeOperation.IncomingWarehouse = Document.ToWarehouse;
			//Предполагается что если документ находиться в одном из принятых статусов, то время доставки обязательно установлено
			WarehouseIncomeOperation.OperationTime = Document.ReceiveTime.Value;
			WarehouseIncomeOperation.Nomenclature = Nomenclature;
			WarehouseIncomeOperation.Amount = ReceivedAmount;

			UpdateWriteoffOperation();
		}

		#endregion
	}
}
