using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды",
		GenitivePlural = "пакетов бесплатной аренды")]
	[EntityPermission]
	public class FreeRentPackage : BusinessObjectBase<FreeRentPackage>, IDomainObject, IValidatableObject
	{
		private int _minWaterAmount;
		private string _name;
		private decimal _deposit;
		private EquipmentKind _equipmentKind;
		private Nomenclature _depositService;
		private bool _isArchive;

		public FreeRentPackage()
		{
			Name = string.Empty;
		}

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Минимальное количество")]
		[Range(1, 200, ErrorMessage = "Минимальное количество воды в пакете аренды не может быть равно нулю.")]
		public virtual int MinWaterAmount
		{
			get => _minWaterAmount;
			set => SetField(ref _minWaterAmount, value);
		}

		[Display(Name = "Название")]
		[Required(ErrorMessage = "Необходимо заполнить название пакета.")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Залог")]
		public virtual decimal Deposit
		{
			get => _deposit;
			set => SetField(ref _deposit, value);
		}

		[Display(Name = "Вид оборудования")]
		[Required(ErrorMessage = "Вид оборудования должен быть указан.")]
		public virtual EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		[Display(Name = "Услуга залога")]
		public virtual Nomenclature DepositService
		{
			get => _depositService;
			set => SetField(ref _depositService, value);
		}

		[Display(Name = "Архив")]
		public virtual bool IsArchive
		{
			get => _isArchive;
			set => SetField(ref _isArchive, value);
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
				yield return new ValidationResult(
					$"Условия для оборудования {EquipmentKind.Name} уже существуют.",
					new[] { this.GetPropertyName(o => o.EquipmentKind) });
			}
		}

		#endregion
	}
}
