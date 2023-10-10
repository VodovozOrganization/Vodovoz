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
		NominativePlural = "строки БА соглашения",
		Nominative = "строка БА соглашения")]
	public class FreeRentEquipment : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		FreeRentPackage freeRentPackage;

		[Display (Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage {
			get => freeRentPackage;
			set => SetField (ref freeRentPackage, value);
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

		Decimal deposit;

		[Display (Name = "Залог")]
		public virtual Decimal Deposit {
			get => deposit;
			set => SetField (ref deposit, value);
		}

		int waterAmount;

		[Display (Name = "Кол бутылей")]
		public virtual int WaterAmount {
			get => waterAmount;
			set => SetField (ref waterAmount, value);
		}

		bool isNew;

		public virtual bool IsNew {
			get => isNew;
			set => SetField (ref isNew, value);
		}

		public virtual string PackageName => FreeRentPackage != null ? FreeRentPackage.Name : "";

		/// <summary>
		/// Выводит имя из оборудования если посерийный учет, иначе из номенклатуры 
		/// </summary>
		/// <value>The name of the equipment.</value>
		public virtual string EquipmentName { 
			get {
				if(Equipment != null) {
					return Equipment.NomenclatureName;
				} else if(Nomenclature != null){
					return Nomenclature.Name;
				}else {
					return String.Empty;
				}
			} 
		}

		public virtual string EquipmentSerial => Equipment != null && Equipment.Nomenclature.IsSerial ? Equipment.Serial : "";

		public virtual string DepositString => CurrencyWorks.GetShortCurrencyString (Deposit);

		public virtual string WaterAmountString => String.Format ("{0} " + NumberToTextRus.Case (WaterAmount, "бутыль", "бутыли", "бутылей"), WaterAmount);

		public virtual string Title => String.Format("Бесплатная аренда {0}", EquipmentName);
	}
}

