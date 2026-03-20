using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Goods;
using InvalidOperationException = System.InvalidOperationException;

namespace Vodovoz.Domain.Documents.MovementDocuments
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения")]
	[HistoryTrace]
	public abstract class MovementDocumentItem : PropertyChangedBase, IDomainObject
	{
		private NomenclatureEntity _nomenclature;
		private decimal _sentAmount;
		private decimal _receivedAmount;
		private decimal _amountOnSource = 99999999;
		private GoodsAccountingOperation _writeOffOperation;
		private GoodsAccountingOperation _incomeOperation;

		public virtual int Id { get; set; }

		public virtual MovementDocument Document { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
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

		public virtual GoodsAccountingOperation WriteOffOperation
		{
			get => _writeOffOperation;
			set => SetField(ref _writeOffOperation, value);
		}

		public virtual GoodsAccountingOperation IncomeOperation
		{
			get => _incomeOperation;
			set => SetField(ref _incomeOperation, value);
		}

		public abstract MovementDocumentItemType MovementDocumentItemType { get; }
		public abstract AccountingType AccountingType { get; }
		
		#region Функции

		public virtual string Title =>
			$"[{Nomenclature.Unit.MakeAmountShortStr(SentAmount)}] {Document.Title} - {Nomenclature.Name}";

		public virtual string Name => Nomenclature != null ? Nomenclature.Name : string.Empty;
		public virtual string InventoryNumber => string.Empty;

		public virtual bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual bool HasDiscrepancy => SentAmount != ReceivedAmount;

		public virtual void UpdateWriteOffOperation()
		{
			if(Document == null) {
				throw new InvalidOperationException(
					"Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(Document.Status == MovementDocumentStatus.Sended)
			{
				CreateOrUpdateWriteOffOperation();
				//Предполагается что если документ находиться в статусе отправлен, то время доставки обязательно установлено
				WriteOffOperation.OperationTime = Document.SendTime.Value;
				WriteOffOperation.Nomenclature = Nomenclature;
				WriteOffOperation.Amount = -SentAmount;
				return;
			}

			if(Document.IsDelivered)
			{
				CreateOrUpdateWriteOffOperation();
				//Предполагается что если документ доставлен, то время доставки обязательно установлено
				WriteOffOperation.OperationTime = Document.SendTime.Value;
				WriteOffOperation.Nomenclature = Nomenclature;
				WriteOffOperation.Amount = -ReceivedAmount;
			}
		}
		
		public virtual void UpdateIncomeOperation()
		{
			if(Document == null)
			{
				throw new InvalidOperationException(
					"Не правильно создана строка перемещения. Не указан документ в котором содержится текущая строка");
			}

			if(!Document.IsDelivered)
			{
				IncomeOperation = null;
				return;
			}

			CreateOrUpdateIncomeOperation();
			//Предполагается что если документ находиться в одном из принятых статусов, то время доставки обязательно установлено
			IncomeOperation.OperationTime = Document.ReceiveTime.Value;
			IncomeOperation.Nomenclature = Nomenclature;
			IncomeOperation.Amount = ReceivedAmount;

			UpdateWriteOffOperation();
		}

		public virtual bool TrySetIsUsed()
		{
			return false;
		}

		protected abstract void CreateOrUpdateIncomeOperation();
		protected abstract void CreateOrUpdateWriteOffOperation();

		#endregion
	}
}
