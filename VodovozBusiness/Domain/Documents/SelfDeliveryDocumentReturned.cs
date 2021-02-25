using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;

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

				if(WarehouseMovementOperation != null && WarehouseMovementOperation.Nomenclature != nomenclature)
					WarehouseMovementOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set {
				SetField(ref equipment, value, () => Equipment);
				if(WarehouseMovementOperation != null && WarehouseMovementOperation.Equipment != equipment)
					WarehouseMovementOperation.Equipment = equipment;

				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != equipment)
					CounterpartyMovementOperation.Equipment = equipment;
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

		WarehouseMovementOperation warehouseMovementOperation;

		public virtual WarehouseMovementOperation WarehouseMovementOperation {
			get => warehouseMovementOperation;
			set => SetField(ref warehouseMovementOperation, value, () => WarehouseMovementOperation);
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
					WarehouseMovementOperation.Nomenclature.Name, 
					WarehouseMovementOperation.Nomenclature.Unit.MakeAmountShortStr(WarehouseMovementOperation.Amount)
				);
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, Counterparty counterparty, DateTime time)
		{
			WarehouseMovementOperation = new WarehouseMovementOperation
				{
					IncomingWarehouse = warehouse,
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
			WarehouseMovementOperation.IncomingWarehouse = warehouse;
			WarehouseMovementOperation.WriteoffWarehouse = null;
			WarehouseMovementOperation.Amount = Amount;

			CounterpartyMovementOperation.WriteoffCounterparty = counterparty;
			CounterpartyMovementOperation.Amount = Amount;
		}

		#endregion
	}
}

