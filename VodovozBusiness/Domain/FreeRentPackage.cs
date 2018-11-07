using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды")]
	public class FreeRentPackage: BusinessObjectBase<FreeRentPackage>, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int minWaterAmount;

		[Display (Name = "Минимальное количество")]
		[Range (1, 200, ErrorMessage = "Минимальное количество воды в пакете аренды не может быть равно нулю.")]
		public virtual int MinWaterAmount {
			get { return minWaterAmount; }
			set { SetField (ref minWaterAmount, value, () => MinWaterAmount); }
		}

		string name;

		[Display (Name = "Название")]
		[Required (ErrorMessage = "Необходимо заполнить название пакета.")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		decimal deposit;

		[Display (Name = "Залог")]
		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		EquipmentType equipmentType;

		[Display (Name = "Тип оборудования")]
		[Required(ErrorMessage = "Тип оборудования должен быть указан.")]
		public virtual EquipmentType EquipmentType {
			get { return equipmentType; }
			set { SetField (ref equipmentType, value, () => EquipmentType); }
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
			var allready = Repository.RentPackageRepository.GetFreeRentPackage(UoW, EquipmentType);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult (
					String.Format ("Условия для оборудования {0} уже существуют.", EquipmentType.Name),
					new[] { this.GetPropertyName (o => o.EquipmentType) });
			}
		}

		#endregion

		public FreeRentPackage ()
		{
			Name = String.Empty;
		}
	}
}
