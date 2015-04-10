using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz
{
	[OrmSubject ("Оборудование для бесплатной аренды")]
	public class FreeRentEquipment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		FreeRentPackage freeRentPackage;

		public virtual FreeRentPackage FreeRentPackage {
			get { return freeRentPackage; }
			set { SetField (ref freeRentPackage, value, () => FreeRentPackage); }
		}

		Equipment equipment;

		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Decimal deposit;

		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		int waterAmount;

		public virtual int WaterAmount {
			get { return waterAmount; }
			set { SetField (ref waterAmount, value, () => WaterAmount); }
		}

		bool isNew;

		public virtual bool IsNew {
			get { return isNew; }
			set { SetField (ref isNew, value, () => IsNew); }
		}

		public virtual string PackageName { get { return FreeRentPackage != null ? FreeRentPackage.Name : ""; } }

		public virtual string EquipmentName { get { return Equipment != null ? Equipment.NomenclatureName : ""; } }

		public virtual string EquipmentSerial { get { return Equipment != null ? Equipment.Serial : ""; } }

		public virtual string DepositString { get { return CurrencyWorks.GetShortCurrencyString (Deposit); } }

		public virtual string WaterAmountString { get { return String.Format ("{0} бутылей", WaterAmount); } }

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (FreeRentPackage == null)
				yield return new ValidationResult ("Не выбран пакет бесплатной аренды.", new[] { "FreeRentPackage" });
			
			if (Equipment == null)
				yield return new ValidationResult ("Не выбрано оборудование.", new[] { "Equipment" });
		}

		#endregion

		public FreeRentEquipment ()
		{
		}
	}
}

