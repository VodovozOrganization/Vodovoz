using QSOrmProject;

namespace Vodovoz
{
	public class IncomingInvoiceItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Nomenclature nomenclature;

		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Equipment equipment;

		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		int amount;

		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}

		decimal price;

		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}
	}
}

