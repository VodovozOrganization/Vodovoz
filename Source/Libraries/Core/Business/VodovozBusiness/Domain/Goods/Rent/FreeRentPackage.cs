using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Goods.Rent;
using Vodovoz.EntityRepositories.RentPackages;

namespace Vodovoz.Domain.Goods.Rent
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "пакеты бесплатной аренды",
		Nominative = "пакет бесплатной аренды")]
	[EntityPermission]
	public class FreeRentPackage : FreeRentPackageEntity, IValidatableObject
	{
		private const int _nameLimit = 45;
		private const int _onlineNameLimit = 45;
		private const int _minWaterCount = 1;
		private const int _maxWaterCount = 200;
		private EquipmentKind _equipmentKind;
		private Nomenclature _depositService;
		private IList<FreeRentPackageOnlineParameters> _onlineParameters = new List<FreeRentPackageOnlineParameters>();
		
		public FreeRentPackage()
		{
			Name = string.Empty;
		}

		#region Свойства

		/// <summary>
		/// Вид оборудования
		/// </summary>
		[Display(Name = "Вид оборудования")]
		public virtual new EquipmentKind EquipmentKind
		{
			get => _equipmentKind;
			set => SetField(ref _equipmentKind, value);
		}

		/// <summary>
		/// Услуга залога
		/// </summary>
		[Display(Name = "Услуга залога")]
		public virtual new Nomenclature DepositService
		{
			get => _depositService;
			set => SetField(ref _depositService, value);
		}

		/// <summary>
		/// Онлайн параметры
		/// </summary>
		[Display(Name = "Онлайн параметры")]
		public virtual new IList<FreeRentPackageOnlineParameters> OnlineParameters
		{
			get => _onlineParameters;
			set => SetField(ref _onlineParameters, value);
		}

		#endregion

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!(validationContext.GetService(
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

			if(MinWaterAmount < _minWaterCount || MinWaterAmount > _maxWaterCount)
			{
				yield return new ValidationResult(
					$"Минимальное количество воды в пакете аренды не может быть меньше {_minWaterCount} или больше {_maxWaterCount}");
			}
			
			if(EquipmentKind is null)
			{
				yield return new ValidationResult("Вид оборудования должен быть указан.");
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
	}
}
