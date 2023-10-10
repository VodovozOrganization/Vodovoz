using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.Utilities;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Domain.Client
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки ПА соглашения",
		Nominative = "строка ПА соглашения")]
	public class PaidRentEquipment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		PaidRentPackage paidRentPackage;

		[Display (Name = "Пакет платной аренды")]
		public virtual PaidRentPackage PaidRentPackage {
			get => paidRentPackage;
			set => SetField (ref paidRentPackage, value);
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set => SetField (ref equipment, value);
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get => nomenclature;
			set => SetField(ref nomenclature, value);
		}

		int count;

		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set => SetField(ref count, value);
		}

		Decimal price;

		[Display (Name = "Цена")]
		public virtual Decimal Price {
			get => price;
			set => SetField (ref price, value);
		}

		Decimal deposit;

		[Display (Name = "Залог")]
		public virtual Decimal Deposit {
			get => deposit;
			set => SetField (ref deposit, value);
		}

		bool isNew;

		public virtual bool IsNew {
			get => isNew;
			set => SetField (ref isNew, value);
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

		public virtual string PackageName => PaidRentPackage != null ? PaidRentPackage.Name : "";

		public virtual string PriceString => CurrencyWorks.GetShortCurrencyString (Price);

		public virtual string Title => String.Format("Платная аренда {0}", Equipment?.NomenclatureName);
	}
}

