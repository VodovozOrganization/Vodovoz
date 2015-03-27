using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Vodovoz
{
	[OrmSubject ("Оборудование для платной аренды")]
	public class PaidRentEquipment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual PaidRentPackage PaidRentPackage { get; set; }

		public virtual Equipment Equipment { get; set; }

		Decimal price;

		public virtual Decimal Price {
			get {
				return price;
			}
			set {
				SetField (ref price, value, () => Price);
			}
		}

		public virtual bool IsNew { get; set; }

		public virtual string PackageName { get { return PaidRentPackage != null ? PaidRentPackage.Name : ""; } }

		public virtual string EquipmentName { get { return Equipment != null ? Equipment.NomenclatureName : ""; } }

		public virtual string EquipmentSerial { get { return Equipment != null ? Equipment.Serial : ""; } }

		public virtual string PriceString { get { return String.Format ("{0} р.", Price); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (PaidRentPackage == null)
				yield return new ValidationResult ("Не выбран пакет платной аренды.", new[] { "PaidRentPackage" });

			if (Equipment == null)
				yield return new ValidationResult ("Не выбрано оборудование.", new[] { "Equipment" });
		}

		#endregion

		public PaidRentEquipment ()
		{
		}
	}
}

