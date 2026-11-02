using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface ISecondOrderDiscount
	{
		/// <summary>
		/// Второй заказ
		/// </summary>
		bool IsSecondOrder { get; set; }
		/// <summary>
		/// Позиции на продажу
		/// </summary>
		IEnumerable<IApplyDiscountReasonItem> SaleItems { get; }
	}
}
