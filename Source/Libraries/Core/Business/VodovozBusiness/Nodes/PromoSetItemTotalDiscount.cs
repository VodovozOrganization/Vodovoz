using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Класс для хранения данных об итоговой скидке в позиции промонабора
	/// </summary>
	public class PromoSetItemTotalDiscount
	{
		private PromoSetItemTotalDiscount(
			decimal totalDiscountMoney,
			IDiscountValue discountValue,
			IEnumerable<DiscountReasonBase> discountReasons,
			PersonalDiscount personalDiscount = null)
		{
			TotalDiscountMoney = totalDiscountMoney;
			DiscountValue = discountValue;
			DiscountReasons = discountReasons;
			PersonalDiscount =  personalDiscount;
		}
		
		/// <summary>
		/// Общая скидка в деньгах
		/// </summary>
		public decimal TotalDiscountMoney { get; }
		/// <summary>
		/// Общие данные по скидке <see cref="IDiscountValue"/>
		/// </summary>
		public IDiscountValue DiscountValue { get; }
		/// <summary>
		/// Список оснований скидок
		/// </summary>
		public IEnumerable<DiscountReasonBase>  DiscountReasons { get; }
		/// <summary>
		/// Персональная скидка
		/// </summary>
		public PersonalDiscount PersonalDiscount { get; }

		public static PromoSetItemTotalDiscount Create(
			decimal totalDiscountMoney,
			IDiscountValue discountValue,
			IEnumerable<DiscountReasonBase> discountReasons,
			PersonalDiscount personalDiscount = null) =>
			new PromoSetItemTotalDiscount(totalDiscountMoney, discountValue, discountReasons, personalDiscount);
	}
}
