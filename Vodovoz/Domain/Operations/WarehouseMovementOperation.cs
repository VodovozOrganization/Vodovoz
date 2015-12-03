using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class WarehouseMovementOperation: OperationBase
	{
		//TODO ID Документа перемещения

		//TODO ID Приходной накладной

		Warehouse writeoffWarehouse;

		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set { SetField (ref writeoffWarehouse, value, () => WriteoffWarehouse); }
		}

		Warehouse incomingWarehouse;

		public virtual Warehouse IncomingWarehouse {
			get { return incomingWarehouse; }
			set { SetField (ref incomingWarehouse, value, () => IncomingWarehouse); }
		}

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		decimal amount;

		public virtual decimal Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}
	}
}

