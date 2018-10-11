using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using QSProjectsLib;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	[OrmSubject (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки ПА соглашения",
		Nominative = "строка ПА соглашения")]
	public class PaidRentEquipment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		PaidRentPackage paidRentPackage;

		[Display (Name = "Пакет платной аренды")]
		public virtual PaidRentPackage PaidRentPackage {
			get { return paidRentPackage; }
			set { SetField (ref paidRentPackage, value, () => PaidRentPackage); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set { SetField(ref nomenclature, value, () => Nomenclature); }
		}

		int count;

		[Display(Name = "Количество")]
		public virtual int Count {
			get { return count; }
			set { SetField(ref count, value, () => Count); }
		}

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get { return price; }
			set { SetField (ref price, value, () => Price); }
		}

		Decimal deposit;

		[Display (Name = "Залог")]
		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		bool isNew;

		public virtual bool IsNew {
			get { return isNew; }
			set { SetField (ref isNew, value, () => IsNew); }
		}

		/// <summary>
		/// Выводит имя из оборудования если посерийный учет, иначе из номенклатуры 
		/// </summary>
		/// <value>The name of the equipment.</value>
		public virtual string EquipmentName {
			get {
				if(Equipment != null) {
					return Equipment.NomenclatureName;
				} else if(Nomenclature != null) {
					return Nomenclature.Name;
				} else {
					return String.Empty;
				}
			}
		}

		public virtual string PackageName { get { return PaidRentPackage != null ? PaidRentPackage.Name : ""; } }

		public virtual string PriceString { get { return CurrencyWorks.GetShortCurrencyString (Price); } }

		public virtual string Title {get { return String.Format("Платная аренда {0}", Equipment?.NomenclatureName); }}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
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

