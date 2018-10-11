using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain
{
	[OrmSubject (Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия платной аренды",
		Nominative = "условие платной аренды",
		Accusative = "условие платной аренды"
	)]
	public class PaidRentPackage: BusinessObjectBase<PaidRentPackage>, IDomainObject, IValidatableObject
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
		[Required(ErrorMessage = "Тип оборудования должен быть указан.")]
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

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var allready = Repository.RentPackageRepository.GetPaidRentPackage(UoW, EquipmentType);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult (
					String.Format ("Условия для оборудования {0} уже существуют.", EquipmentType.Name),
					new[] { this.GetPropertyName (o => o.EquipmentType) });
			}
		}

		#endregion

		public PaidRentPackage ()
		{
			Name = String.Empty;
		}
	}
}
