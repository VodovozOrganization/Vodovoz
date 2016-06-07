using System;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Service
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки заявки на обслуживание",
		Nominative = "строка заявки на обслуживание")]
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

		public virtual decimal Total { get { return Price * Count; } }

		public virtual string Title{
			get{
				return String.Format("{0} - {1}", Nomenclature.Name, CurrencyWorks.GetShortCurrencyString(Total));
			}
		}
	}
}

