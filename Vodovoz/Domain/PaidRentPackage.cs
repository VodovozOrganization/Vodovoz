using System;
using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using QSProjectsLib;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия платной аренды",
		Nominative = "условие платной аренды",
		Accusative = "условие платной аренды"
	)]
	public class PaidRentPackage: PropertyChangedBase, IDomainObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Необходимо заполнить название пакета платной аренды.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal priceDaily;

		[Display (Name = "Стоимость дня")]
		public virtual decimal PriceDaily {
			get { return priceDaily; }
			set { SetField (ref priceDaily, value, () => PriceDaily); }
		}

		Nomenclature rentServiceDaily;

		[Display (Name = "Услуга КПА")]
		public virtual Nomenclature RentServiceDaily {
			get { return rentServiceDaily; }
			set { SetField (ref rentServiceDaily, value, () => RentServiceDaily); }
		}

		decimal priceMonthly;

		[Display (Name = "Стоимость месяца")]
		public virtual decimal PriceMonthly {
			get { return priceMonthly; }
			set { SetField (ref priceMonthly, value, () => PriceMonthly); }
		}

		Nomenclature rentServiceMonthly;

		[Display (Name = "Услуга ДПА")]
		public virtual Nomenclature RentServiceMonthly {
			get { return rentServiceMonthly; }
			set { SetField (ref rentServiceMonthly, value, () => RentServiceMonthly); }
		}

		EquipmentType equipmentType;

		[Display (Name = "Тип оборудования")]
		public virtual EquipmentType EquipmentType {
			get { return equipmentType; }
			set { SetField (ref equipmentType, value, () => EquipmentType); }
		}

		decimal deposit;

		[Display (Name = "Залог")]
		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		Nomenclature depositService;

		[Display (Name = "Услуга залога")]
		public virtual Nomenclature DepositService {
			get { return depositService; }
			set { SetField (ref depositService, value, () => DepositService); }
		}

		#endregion

		public PaidRentPackage ()
		{
			Name = String.Empty;
		}
	}
}
