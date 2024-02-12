using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain.Goods.Rent
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "условия платной аренды",
		Nominative = "условие платной аренды",
		Accusative = "условие платной аренды"
	)]
	[EntityPermission]
	public class PaidRentPackage: BusinessObjectBase<PaidRentPackage>, IDomainObject, IValidatableObject
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

		EquipmentKind equipmentKind;

		[Display (Name = "Вид оборудования")]
		[Required(ErrorMessage = "Вид оборудования должен быть указан.")]
		public virtual EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		decimal deposit;

		[Display (Name = "Залог")]
		public virtual decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		Nomenclature depositService;

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
				throw new ArgumentNullException($"Не найден репозиторий { nameof(rentPackageRepository) }");
			}
			
			var allready = rentPackageRepository.GetPaidRentPackage(UoW, EquipmentKind);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult(
					$"Условия для оборудования {EquipmentKind.Name} уже существуют.",
					new[] { this.GetPropertyName(o => o.EquipmentKind) });
			}
		}

		#endregion

		public PaidRentPackage ()
		{
			Name = String.Empty;
		}
	}
}
