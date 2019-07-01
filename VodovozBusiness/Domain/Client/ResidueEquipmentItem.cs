using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
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
			set {
				if(SetField(ref equipmentDirection, value, () => EquipmentDirection) && equipmentDirection == ResidueEquipmentDirection.FromClient) {
					EquipmentDeposit = 0;
				}
			}
		}

		private int count;
		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set => SetField(ref count, value, () => Count);
		}

		private decimal equipmentDeposit;
		[Display(Name = "Залог за оборудование")]
		public virtual decimal EquipmentDeposit {
			get { return equipmentDeposit; }
			set { SetField(ref equipmentDeposit, value, () => EquipmentDeposit); }
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
			movementOperation.Amount = Count;
			if(EquipmentDirection == ResidueEquipmentDirection.ToClient) {
				movementOperation.IncomingCounterparty = Residue.Customer;
				movementOperation.WriteoffCounterparty = null;
			} else {
				movementOperation.WriteoffCounterparty = Residue.Customer;
				movementOperation.IncomingCounterparty = null;
			}
		}
	}
}
