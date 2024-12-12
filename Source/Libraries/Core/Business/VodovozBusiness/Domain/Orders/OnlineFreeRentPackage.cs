using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods.Rent;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Строки бесплатной аренды онлайн заказа",
		Nominative = "Строка бесплатной аренды онлайн заказа",
		Prepositional = "Строке бесплатной аренды онлайн заказа",
		PrepositionalPlural = "Строках бесплатной аренды онлайн заказа"
	)]
	[HistoryTrace]
	public class OnlineFreeRentPackage : PropertyChangedBase, IDomainObject
	{
		private int? _freeRentPackageId;
		private FreeRentPackage _freeRentPackage;
		private int _count;
		private decimal _price;
		private OnlineOrder _onlineOrder;

		protected OnlineFreeRentPackage() { }
		
		public virtual int Id { get; set; }
		
		[Display(Name = "Id пакета аренды")]
		public virtual int? FreeRentPackageId
		{
			get => _freeRentPackageId;
			set => SetField(ref _freeRentPackageId, value);
		}
		
		[Display(Name = "Пакет аренды")]
		public virtual FreeRentPackage FreeRentPackage
		{
			get => _freeRentPackage;
			set => SetField(ref _freeRentPackage, value);
		}
		
		[Display(Name = "Цена")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}
		
		[Display(Name = "Количество")]
		public virtual int Count
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
		
		[Display(Name = "Цена аренды из ДВ")]
		public virtual decimal FreeRentPackagePriceFromProgram { get; set; }

		public virtual decimal Sum => Count * Price;

		public static OnlineFreeRentPackage Create(
			int? rentPackageId,
			int count,
			decimal price,
			FreeRentPackage rentPackage,
			OnlineOrder onlineOrder)
		{
			return new OnlineFreeRentPackage
			{
				FreeRentPackageId = rentPackageId,
				Count = count,
				Price = price,
				FreeRentPackage = rentPackage,
				OnlineOrder = onlineOrder
			};
		}
	}
}
