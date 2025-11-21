using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public abstract class FreeRentPackageOnlineParameters : PropertyChangedBase, IDomainObject
	{
		private GoodsOnlineAvailability? _packageOnlineAvailability;
		private FreeRentPackage _freeRentPackage;

		public virtual int Id { get; set; }

		/// <summary>
		/// Пакет бесплатной аренды
		/// </summary>
		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage
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
		/// Тип параметра онлайн номенклатуры
		/// </summary>
		public abstract GoodsOnlineParameterType Type { get; }
	}
}
