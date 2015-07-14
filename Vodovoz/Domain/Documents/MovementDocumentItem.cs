using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения")]
	public class MovementDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual MovementDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature);
				if (MoveGoodsOperation.Nomenclature != nomenclature)
					MoveGoodsOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment);
				if (MoveGoodsOperation.Equipment != equipment)
					MoveGoodsOperation.Equipment = equipment;
			}
		}

		decimal amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount);
				if (MoveGoodsOperation.Amount != amount)
					MoveGoodsOperation.Amount = amount;
			}
		}

		decimal amountOnSource = 10000000; //FIXME пока не реализуем способ загружать количество на складе на конкретный день

		[Display (Name = "Имеется на складе")]
		public virtual decimal AmountOnSource {
			get { return amountOnSource; }
			set {
				SetField (ref amountOnSource, value, () => AmountOnSource);
			}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null ? Equipment.Serial : "-"; } 
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature != null && !Nomenclature.Serial; }
		}

		GoodsMovementOperation moveGoodsOperation = new GoodsMovementOperation ();

		public GoodsMovementOperation MoveGoodsOperation {
			get { return moveGoodsOperation; }
			set { SetField (ref moveGoodsOperation, value, () => MoveGoodsOperation); }
		}

	}
}

