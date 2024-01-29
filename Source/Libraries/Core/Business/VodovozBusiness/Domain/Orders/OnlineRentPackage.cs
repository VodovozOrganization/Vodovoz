using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Orders
{
	public class OnlineRentPackage : PropertyChangedBase, IDomainObject
	{
		private int _rentPackageId;
		private FreeRentPackage _rentPackage;
		private decimal _count;
		private decimal _price;
		private OnlineOrder _onlineOrder;
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Id пакета аренды")]
		public virtual int RentPackageId
		{
			get => _rentPackageId;
			set => SetField(ref _rentPackageId, value);
		}
		
		[Display(Name = "Пакет аренды")]
		public virtual FreeRentPackage RentPackage
		{
			get => _rentPackage;
			set => SetField(ref _rentPackage, value);
		}
		
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}
		
		[Display(Name = "Количество")]
		public virtual decimal Count
		{
			get => _count;
			set => SetField(ref _count, value);
		}
		
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}
	}
}
