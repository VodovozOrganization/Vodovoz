using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain.Goods.Rent
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия платной аренды",
		Nominative = "условие платной аренды",
		Accusative = "условие платной аренды"
	)]
	[EntityPermission]
	public class PaidRentPackage : BusinessObjectBase<PaidRentPackage>, IDomainObject, IValidatableObject
	{
		private string _name;
		private decimal _priceDaily;
		private Nomenclature _rentServiceDaily;
		private decimal _priceMonthly;
		private Nomenclature _rentServiceMonthly;
		private EquipmentKind _equipmentKind;
		private decimal _deposit;
		private Nomenclature _depositService;

		public PaidRentPackage()
		{
			Name = String.Empty;
		}
		
		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		[Display(Name = "Стоимость дня")]
		public virtual decimal PriceDaily
		{
			get => _priceDaily;
			set => SetField(ref _priceDaily, value);
		}

		[Display(Name = "Услуга КПА")]
		public virtual Nomenclature RentServiceDaily
		{
			get => _rentServiceDaily;
			set => SetField(ref _rentServiceDaily, value);
		}

		[Display(Name = "Стоимость месяца")]
		public virtual decimal PriceMonthly
		{
			get => _priceMonthly;
			set => SetField(ref _priceMonthly, value);
		}

		[Display(Name = "Услуга ДПА")]
		public virtual Nomenclature RentServiceMonthly
		{
			get => _rentServiceMonthly;
			set => SetField(ref _rentServiceMonthly, value);
		}

		[Display(Name = "Вид оборудования")]
		public virtual EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		[Display(Name = "Залог")]
		public virtual decimal Deposit
		{
			get => _deposit;
			set => SetField(ref _deposit, value);
		}

		[Display(Name = "Услуга залога")]
		public virtual Nomenclature DepositService
		{
			get => _depositService;
			set => SetField(ref _depositService, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(
					typeof(IRentPackageRepository)) is IRentPackageRepository rentPackageRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий { nameof(rentPackageRepository) }");
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Необходимо заполнить название пакета платной аренды.");
			}
			
			if(EquipmentKind is null)
			{
				yield return new ValidationResult("Вид оборудования должен быть указан.");
			}

			var allready = rentPackageRepository.GetPaidRentPackage(UoW, EquipmentKind);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult($"Условия для оборудования {EquipmentKind.Name} уже существуют.");
			}
		}

		#endregion
	}
}
