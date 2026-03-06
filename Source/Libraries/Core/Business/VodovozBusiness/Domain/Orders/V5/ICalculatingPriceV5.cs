using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders.V5
{
	public interface ICalculatingPriceV5
	{
		/// <summary>
		/// Id сущности
		/// </summary>
		int Id { get; }
		/// <summary>
		/// Количество
		/// </summary>
		decimal Count { get; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; }
		/// <summary>
		/// Скидки
		/// </summary>
		IEnumerable<IDiscountData> Discounts { get; }
		/// <summary>
		/// Номенклатура <see cref="Vodovoz.Domain.Goods.Nomenclature"/>
		/// </summary>
		Nomenclature Nomenclature { get; }
		/// <summary>
		/// Промо набор <see cref="Vodovoz.Domain.Orders.PromotionalSet"/>
		/// </summary>
		PromotionalSet PromoSet { get; }
	}

	public interface IDiscountData
	{
		decimal Discount { get; }
		bool IsDiscountInMoney { get; }
		DiscountReason DiscountReason { get; }
	}

	public class ProductDiscountData : IDiscountData
	{
		public decimal Discount { get; set; }
		public bool IsDiscountInMoney { get; set; }
		public DiscountReason DiscountReason { get; set; }
	}
}
