using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class GoodsMovementOperation: OperationBase
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

		Counterparty incomingCounterparty;

		public virtual Counterparty IncomingCounterparty {
			get { return incomingCounterparty; }
			set { SetField (ref incomingCounterparty, value, () => IncomingCounterparty); }
		}

		DeliveryPoint incomingDeliveryPoint;

		public virtual DeliveryPoint IncomingDeliveryPoint {
			get { return incomingDeliveryPoint; }
			set { SetField (ref incomingDeliveryPoint, value, () => IncomingDeliveryPoint); }
		}

		Counterparty writeoffCounterparty;

		public virtual Counterparty WriteoffCounterparty {
			get { return writeoffCounterparty; }
			set { SetField (ref writeoffCounterparty, value, () => WriteoffCounterparty); }
		}

		DeliveryPoint writeoffDeliveryPoint;

		public virtual DeliveryPoint WriteoffDeliveryPoint {
			get { return writeoffDeliveryPoint; }
			set { SetField (ref writeoffDeliveryPoint, value, () => WriteoffDeliveryPoint); }
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

		bool sale;

		public virtual bool Sale {
			get { return sale; }
			set { SetField (ref sale, value, () => Sale); }
		}
	}
}

