using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Core.Domain.Goods.Rent
{
	public class FreeRentPackageOnlineParametersEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private GoodsOnlineAvailability? _packageOnlineAvailability;
		private FreeRentPackageEntity _freeRentPackage;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value, () => Id);
		}

		/// <summary>
		/// Пакет бесплатной аренды
		/// </summary>
		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackageEntity FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
		}

		/// <summary>
		/// Онлайн доступность
		/// </summary>
		[Display(Name = "Онлайн доступность")]
		public virtual GoodsOnlineAvailability? PackageOnlineAvailability
		{
			get => _packageOnlineAvailability;
			set => SetField(ref _packageOnlineAvailability, value);
		}

		/// <summary>
		/// Тип параметра для онлайн-товаров
		/// </summary>
		public virtual GoodsOnlineParameterType Type { get; }
	}
}
