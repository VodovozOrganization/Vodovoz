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
			IDiscountValue totalDiscountValue,
			IDiscountValue additionalDiscountValue,
			IEnumerable<DiscountReasonBase> discountReasons,
			PersonalDiscount personalDiscount = null)
		{
			TotalDiscountValue = totalDiscountValue;
			AdditionalDiscountValue = additionalDiscountValue;
			DiscountReasons = discountReasons;
			PersonalDiscount = personalDiscount;
		}

		/// <summary>
		/// Общие данные по скидке <see cref="IDiscountValue"/>
		/// </summary>
		public IDiscountValue TotalDiscountValue { get; }
		
		/// <summary>
		/// Данные по дополнительной скидке(промокод, применимая скидка и т.д.) <see cref="IDiscountValue"/>
		/// </summary>
		public IDiscountValue AdditionalDiscountValue { get; }

		/// <summary>
		/// Список оснований скидок
		/// </summary>
		public IEnumerable<DiscountReasonBase> DiscountReasons { get; }

		/// <summary>
		/// Персональная скидка
		/// </summary>
		public PersonalDiscount PersonalDiscount { get; }

		public static PromoSetItemTotalDiscount Create(
			IDiscountValue totalDiscountValue,
			IDiscountValue additionalDiscountValue,
			IEnumerable<DiscountReasonBase> discountReasons,
			PersonalDiscount personalDiscount = null) =>
			new PromoSetItemTotalDiscount(totalDiscountValue, additionalDiscountValue, discountReasons, personalDiscount);
	}
}
