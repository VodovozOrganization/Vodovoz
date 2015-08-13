using QSOrmProject;

namespace Vodovoz.Domain.Service
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "заявки на обслуживание",
		Nominative = "заявка на обслуживание")]
	public class ServiceClaimItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		ServiceClaim serviceClaim;

		public virtual ServiceClaim ServiceClaim {
			get { return serviceClaim; }
			set { SetField (ref serviceClaim, value, () => ServiceClaim); }
		}

		Nomenclature nomenclature;

		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		decimal price;

		public virtual decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		decimal count;

		public virtual decimal Count {
			get { return count; }
			set { SetField (ref count, value, () => Count); }
		}
	}
}

