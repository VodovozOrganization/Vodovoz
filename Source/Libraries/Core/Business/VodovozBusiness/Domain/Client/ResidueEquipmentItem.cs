using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "строки оборудования документов ввода остатков",
		Nominative = "строка оборудования документа ввода остатков")]
	public class ResidueEquipmentDepositItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		private Residue residue;
		[Display(Name = "Документ ввода остатков")]
		public virtual Residue Residue {
			get => residue;
			set => SetField(ref residue, value, () => Residue);
		}

		private Nomenclature nomenclature;
		[Display(Name = "Номенклатура оборудования")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value, () => Nomenclature);
		}

		private ResidueEquipmentDirection equipmentDirection;
		[Display(Name = "Направление передачи оборудования")]
		public virtual ResidueEquipmentDirection EquipmentDirection {
			get => equipmentDirection;
			set => SetField(ref equipmentDirection, value, () => EquipmentDirection);
		}

		private int equipmentCount;
		[Display(Name = "Количество оборудования")]
		public virtual int EquipmentCount {
			get => equipmentCount;
			set => SetField(ref equipmentCount, value, () => EquipmentCount);
		}

		private int depositCount;
		[Display(Name = "Количество залогов")]
		public virtual int DepositCount {
			get => depositCount;
			set => SetField(ref depositCount, value, () => DepositCount);
		}

		private decimal equipmentDeposit;
		[Display(Name = "Залог за оборудование")]
		public virtual decimal EquipmentDeposit {
			get { return equipmentDeposit; }
			set { SetField(ref equipmentDeposit, value, () => EquipmentDeposit); }
		}

		private PaymentType paymentType;
		[Display(Name = "Форма оплаты")]
		public virtual PaymentType PaymentType {
			get { return paymentType; }
			set { SetField(ref paymentType, value, () => PaymentType); }
		}

		private CounterpartyMovementOperation movementOperation;
		[Display(Name = "Операция передвижения оборудования")]
		public virtual CounterpartyMovementOperation MovementOperation {
			get => movementOperation;
			set => SetField(ref movementOperation, value, () => MovementOperation);
		}

		public virtual void UpdateOperation()
		{
			if(MovementOperation == null) {
				MovementOperation = new CounterpartyMovementOperation();
			}
			movementOperation.Nomenclature = Nomenclature;
			movementOperation.Amount = EquipmentCount;
			if(Residue.DeliveryPoint == null) {
				if(EquipmentDirection == ResidueEquipmentDirection.ToClient) {
					movementOperation.IncomingCounterparty = Residue.Customer;
					movementOperation.WriteoffCounterparty = null;
					movementOperation.WriteoffDeliveryPoint = null;
					movementOperation.IncomingDeliveryPoint = null;
				} else {
					movementOperation.WriteoffCounterparty = Residue.Customer;
					movementOperation.IncomingCounterparty = null;
					movementOperation.WriteoffDeliveryPoint = null;
					movementOperation.IncomingDeliveryPoint = null;
				}
			} else {
				if(EquipmentDirection == ResidueEquipmentDirection.ToClient) {
					movementOperation.IncomingDeliveryPoint = Residue.DeliveryPoint;
					movementOperation.IncomingCounterparty = Residue.Customer;
					movementOperation.WriteoffDeliveryPoint = null;
					movementOperation.WriteoffCounterparty = null;
				} else {
					movementOperation.WriteoffDeliveryPoint = Residue.DeliveryPoint;
					movementOperation.WriteoffCounterparty = Residue.Customer;
					movementOperation.IncomingDeliveryPoint = null;
					movementOperation.IncomingCounterparty = null;
				}
			}

		}
	}
}
