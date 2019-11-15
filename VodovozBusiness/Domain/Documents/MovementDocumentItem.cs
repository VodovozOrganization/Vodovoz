using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения")]
	[HistoryTrace]
	public class MovementDocumentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual MovementDocument Document { get; set; }

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		private decimal sendedAmount;
		[Display(Name = "Отправлено")]
		public virtual decimal SendedAmount {
			get => sendedAmount;
			set => SetField(ref sendedAmount, value);
		}

		private decimal receivedAmount;
		[Display(Name = "Принято")]
		public virtual decimal ReceivedAmount {
			get => receivedAmount;
			set => SetField(ref receivedAmount, value);
		}

		decimal amountOnSource = 99999999;

		[Display(Name = "Имеется на складе")]
		public virtual decimal AmountOnSource {
			get => amountOnSource;
			set => SetField(ref amountOnSource, value);
		}

		WarehouseMovementOperation warehouseWriteoffOperation;
		public virtual WarehouseMovementOperation WarehouseWriteoffOperation {
			get => warehouseWriteoffOperation;
			set => SetField(ref warehouseWriteoffOperation, value);
		}

		WarehouseMovementOperation incomeOperation;
		public virtual WarehouseMovementOperation WarehouseIncomeOperation {
			get => incomeOperation;
			set => SetField(ref incomeOperation, value);
		}

		#region Функции

		public virtual string Title {
			get {
				return String.Format("[{2}] {0} - {1}",
					Document.Title,
					Nomenclature.Name,
					Nomenclature.Unit.MakeAmountShortStr(SendedAmount));
			}
		}

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : "";

		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual bool HasDiscrepancy => SendedAmount != ReceivedAmount;

		public virtual void UpdateWriteoffOperation()
		{
			if(Document == null) {
				throw new InvalidOperationException("Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(Document.Status == MovementDocumentStatus.Sended) {
				if(WarehouseWriteoffOperation == null) {
					WarehouseWriteoffOperation = new WarehouseMovementOperation();
				}

				WarehouseWriteoffOperation.WriteoffWarehouse = Document.FromWarehouse;
				WarehouseWriteoffOperation.IncomingWarehouse = null;
				//Предполагается что если документ находиться в статусе отправлен, то время доставки обязательно установлено
				WarehouseWriteoffOperation.OperationTime = Document.SendTime.Value;
				WarehouseWriteoffOperation.Nomenclature = Nomenclature;
				WarehouseWriteoffOperation.Amount = SendedAmount;
				return;
			}

			if(Document.IsDelivered) {
				if(WarehouseWriteoffOperation == null) {
					WarehouseWriteoffOperation = new WarehouseMovementOperation();
				}

				WarehouseWriteoffOperation.WriteoffWarehouse = Document.FromWarehouse;
				WarehouseWriteoffOperation.IncomingWarehouse = null;
				//Предполагается что если документ доставлен, то время доставки обязательно установлено
				WarehouseWriteoffOperation.OperationTime = Document.SendTime.Value;
				WarehouseWriteoffOperation.Nomenclature = Nomenclature;
				WarehouseWriteoffOperation.Amount = ReceivedAmount;
			}
		}

		public virtual void UpdateIncomeOperation()
		{
			if(Document == null) {
				throw new InvalidOperationException("Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(!Document.IsDelivered) {
				WarehouseIncomeOperation = null;
				return;
			}

			if(WarehouseIncomeOperation == null) {
				WarehouseIncomeOperation = new WarehouseMovementOperation();
			}

			WarehouseIncomeOperation.WriteoffWarehouse = null;
			WarehouseIncomeOperation.IncomingWarehouse = Document.ToWarehouse;
			//Предполагается что если документ находиться в одном из принятых статусов, то время доставки обязательно установлено
			WarehouseIncomeOperation.OperationTime = Document.ReceiveTime.Value;
			WarehouseIncomeOperation.Nomenclature = Nomenclature;
			WarehouseIncomeOperation.Amount = ReceivedAmount;

			UpdateWriteoffOperation();
		}

		#endregion
	}
}