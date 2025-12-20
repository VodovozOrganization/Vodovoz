using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentReturned: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		SelfDeliveryDocument document;

		public virtual SelfDeliveryDocument Document {
			get => document;
			set => SetField(ref document, value, () => Document);
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set {
				SetField(ref nomenclature, value, () => Nomenclature);

				if(GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != nomenclature)
					GoodsAccountingOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set {
				SetField(ref equipment, value, () => Equipment);
				
				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != equipment)
				{
					CounterpartyMovementOperation.Equipment = equipment;
				}
			}
		}

		decimal amount;

		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get => amount;
			set => SetField(ref amount, value, () => Amount);
		}

		int? actualCount;
		/// <summary>
		/// Количество оборудования, которое фактически привез/забрал клиент
		/// </summary>
		public virtual int? ActualCount
		{
			get => actualCount;
			set => SetField(ref actualCount, value);
		}

		WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;

		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation {
			get => _goodsAccountingOperation;
			set => SetField(ref _goodsAccountingOperation, value);
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get => counterpartyMovementOperation;
			set => SetField(ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation);
		}

		Direction? direction;

		[Display(Name = "Направление")]
		public virtual Direction? Direction
		{
			get => direction;
			set => SetField(ref direction, value, () => Direction);
		}

		DirectionReason directionReason;

		[Display(Name = "Причина забор-доставки")]
		public virtual DirectionReason DirectionReason
		{
			get => directionReason;
			set => SetField(ref directionReason, value, () => DirectionReason);
		}

		OwnTypes ownType;

		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType
		{
			get => ownType;
			set => SetField(ref ownType, value, () => OwnType);
		}

		#region Не сохраняемые

		decimal amountUnloaded;

		[Display (Name = "Уже отгружено")]
		public virtual decimal AmountUnloaded {
			get => amountUnloaded;
			set => SetField(ref amountUnloaded, value, () => AmountUnloaded);
		}

		public virtual string Title {
			get{
				return string.Format(
					"{0} - {1}", 
					GoodsAccountingOperation.Nomenclature.Name, 
					GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount)
				);
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, Counterparty counterparty, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
				{
					Warehouse = warehouse,
					Amount = Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
				};

			CounterpartyMovementOperation = new CounterpartyMovementOperation 
				{
					WriteoffCounterparty = counterparty,
					Amount = Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
				};
		}

		public virtual void UpdateOperation(Warehouse warehouse, Counterparty counterparty)
		{
			GoodsAccountingOperation.Warehouse = warehouse;
			GoodsAccountingOperation.Amount = Amount;

			CounterpartyMovementOperation.WriteoffCounterparty = counterparty;
			CounterpartyMovementOperation.Amount = Amount;
		}

		#endregion
	}
}

