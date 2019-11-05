using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
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
			set => SetField(ref nomenclature, value, () => Nomenclature);
		}

		private decimal sendedAmount;
		[Display(Name = "Отправлено")]
		public virtual decimal SendedAmount {
			get => sendedAmount;
			set => SetField(ref sendedAmount, value, () => SendedAmount);
		}

		private decimal deliveredAmount;
		[Display(Name = "Принято")]
		public virtual decimal DeliveredAmount {
			get => deliveredAmount;
			set => SetField(ref deliveredAmount, value, () => DeliveredAmount);
		}

		decimal amountOnSource = 99999999;

		[Display(Name = "Имеется на складе")]
		public virtual decimal AmountOnSource {
			get => amountOnSource;
			set => SetField(ref amountOnSource, value, () => AmountOnSource);
		}

		WarehouseMovementOperation warehouseWriteoffOperation;
		public virtual WarehouseMovementOperation WarehouseWriteoffOperation {
			get => warehouseWriteoffOperation;
			set => SetField(ref warehouseWriteoffOperation, value, () => WarehouseWriteoffOperation);
		}

		WarehouseMovementOperation incomeOperation;
		public virtual WarehouseMovementOperation WarehouseIncomeOperation {
			get => incomeOperation;
			set => SetField(ref incomeOperation, value, () => WarehouseIncomeOperation);
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

		public virtual bool HasDiscrepancy => SendedAmount != DeliveredAmount;


		public virtual void UpdateWriteoffOperation()
		{
			if(Document == null) {
				throw new InvalidOperationException("Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(Document.Status != MovementDocumentStatus.Sended) {
				return;
			}

			if(WarehouseWriteoffOperation == null) {
				WarehouseWriteoffOperation = new WarehouseMovementOperation();
			}

			WarehouseWriteoffOperation.WriteoffWarehouse = Document.FromWarehouse;
			WarehouseWriteoffOperation.IncomingWarehouse = null;
			//Предполагается что если документ находиться в статусе отправлен, то время доставки обязательно установлено
			WarehouseWriteoffOperation.OperationTime = Document.SendTime.Value;
			WarehouseWriteoffOperation.Nomenclature = Nomenclature;
			WarehouseWriteoffOperation.Amount = SendedAmount;
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
			WarehouseIncomeOperation.OperationTime = Document.DeliveredTime.Value;
			WarehouseIncomeOperation.Nomenclature = Nomenclature;
			WarehouseIncomeOperation.Amount = SendedAmount;
		}

		#endregion
	}
}