using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Goods.Rent;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Domain.Orders
{
	/// <summary>
	/// Более детальный контракт товара
	/// </summary>
	public interface IProduct : IAddItem
	{
		/// <summary>
		/// Id сущности
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal GetDiscount { get; }
		/// <summary>
		/// Скидка в деньгах
		/// </summary>
		bool IsDiscountInMoney { get; }
		/// <summary>
		/// Фактическая сумма
		/// </summary>
		decimal ActualSum { get; }
		/// <summary>
		/// Текущее количество
		/// </summary>
		decimal CurrentCount { get; }
		/// <summary>
		/// 
		/// </summary>
		OrderItemRentSubType OrderItemRentSubType { get; }
		/// <summary>
		/// 
		/// </summary>
		void RecalculatePrice();
		bool IsMasterNomenclature { get; }
		PaidRentPackage PaidRentPackage { get; }
		FreeRentPackage FreeRentPackage { get; }
		SaleRentType RentType { get; }
		bool IsUserPrice { get; }
		void SetPrice(decimal price);
	}
	
	/// <summary>
	/// Более детальный контракт товара
	/// </summary>
	public interface IAddItem : IGoods
	{
		/// <summary>
		/// Скопирован из недовоза
		/// </summary>
		bool IsCopiedFromUndelivery { get; }
		/// <summary>
		/// Альтернативная цена
		/// </summary>
		bool IsAlternativePrice { get; set; }
	}
}
