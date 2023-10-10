using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain.Goods.Rent
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды")]
	[EntityPermission]
	public class FreeRentPackage : BusinessObjectBase<FreeRentPackage>, IDomainObject, IValidatableObject
	{
		private const int _nameLimit = 45;
		private const int _onlineNameLimit = 45;
		private int _minWaterAmount;
		private string _name;
		private string _onlineName;
		private decimal _deposit;
		private EquipmentKind _equipmentKind;
		private Nomenclature _depositService;
		private IList<FreeRentPackageOnlineParameters> _onlineParameters = new List<FreeRentPackageOnlineParameters>();

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
		
		[Display(Name = "Название в ИПЗ")]
		public virtual string OnlineName
		{
			get => _onlineName;
			set => SetField(ref _onlineName, value);
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
		
		[Display(Name = "Онлайн параметры")]
		public virtual IList<FreeRentPackageOnlineParameters> OnlineParameters
		{
			get => _onlineParameters;
			set => SetField(ref _onlineParameters, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.ServiceContainer.GetService(
					typeof(IRentPackageRepository)) is IRentPackageRepository rentPackageRepository))
			{
				throw new ArgumentNullException($"Не найден репозиторий {nameof(rentPackageRepository)}");
			}

			if(string.IsNullOrWhiteSpace(Name))
			{
				yield return new ValidationResult("Нужно заполнить название пакета аренды");
			}
			else if(Name.Length > _nameLimit)
			{
				yield return new ValidationResult($"Длина названия пакета аренды превышена на {Name.Length - _nameLimit}");
			}

			if(!string.IsNullOrWhiteSpace(OnlineName) && OnlineName.Length > _onlineNameLimit)
			{
				yield return new ValidationResult(
					$"Длина названия пакета аренды в ИПЗ превышена на {OnlineName.Length - _onlineNameLimit}");
			}

			var allready = rentPackageRepository.GetFreeRentPackage(UoW, EquipmentKind);
			if(allready != null && allready.Id != Id)
			{
				yield return new ValidationResult(
					$"Условия для оборудования {EquipmentKind.Name} уже существуют.",
					new[] { nameof(EquipmentKind) });
			}
		}

		#endregion

		public FreeRentPackage()
		{
			Name = string.Empty;
		}
	}
}
