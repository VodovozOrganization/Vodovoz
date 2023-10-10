using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.Rent
{
	public abstract class FreeRentPackageOnlineParameters : PropertyChangedBase, IDomainObject
	{
		private GoodsOnlineAvailability? _packageOnlineAvailability;
		private FreeRentPackage _freeRentPackage;

		public virtual int Id { get; set; }

		[Display(Name = "Пакет бесплатной аренды")]
		public virtual FreeRentPackage FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
		}
		
		[Display(Name = "Онлайн доступность")]
		public virtual GoodsOnlineAvailability? PackageOnlineAvailability
		{
			get => _packageOnlineAvailability;
			set => SetField(ref _packageOnlineAvailability, value);
		}
		
		public abstract GoodsOnlineParameterType Type { get; }
	}
}
