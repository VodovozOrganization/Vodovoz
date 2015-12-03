using System;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Operations;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения товаров",
		Nominative = "передвижение товаров")]
	public class CounterpartyMovementOperation : OperationBase
	{
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

		bool forRent;

		public virtual bool ForRent {
			get { return forRent; }
			set { SetField (ref forRent, value, () => ForRent); }
		}
	}
}

