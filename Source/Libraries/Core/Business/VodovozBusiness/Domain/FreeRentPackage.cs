using System;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды")]
	[EntityPermission]
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

		EquipmentKind equipmentKind;

		[Display (Name = "Вид оборудования")]
		[Required(ErrorMessage = "Вид оборудования должен быть указан.")]
		public virtual EquipmentKind EquipmentKind {
			get { return equipmentKind; }
			set { SetField (ref equipmentKind, value, () => EquipmentKind); }
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
			if(!(validationContext.ServiceContainer.GetService(
				typeof(IRentPackageRepository)) is IRentPackageRepository rentPackageRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(rentPackageRepository)}");
			}
			
			var allready = rentPackageRepository.GetFreeRentPackage(UoW, EquipmentKind);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult (
					String.Format ("Условия для оборудования {0} уже существуют.", EquipmentKind.Name),
					new[] { this.GetPropertyName (o => o.EquipmentKind) });
			}
		}

		#endregion

		public FreeRentPackage ()
		{
			Name = String.Empty;
		}
	}
}
