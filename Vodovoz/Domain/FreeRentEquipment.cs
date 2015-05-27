using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QSProjectsLib;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Строки БА соглашения", ObjectName = "строка БА соглашения")]
	public class FreeRentEquipment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		FreeRentPackage freeRentPackage;

		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage {
			get { return freeRentPackage; }
			set { SetField (ref freeRentPackage, value, () => FreeRentPackage); }
		}

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Decimal deposit;

		[Display(Name = "Залог")]
		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		int waterAmount;
		[Display(Name = "Кол. бутылей")]
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

		public virtual string WaterAmountString { get { return String.Format ("{0} " + RusNumber.Case (WaterAmount, "бутыль", "бутыли", "бутылей"), WaterAmount); } }

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

