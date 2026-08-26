using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	public interface IDiscountReasons
	{
		/// <summary>
		/// Основания скидок <see cref="Vodovoz.Domain.Orders.DiscountReason"/>
		/// </summary>
		IEnumerable<DiscountReason> DiscountReasons { get; }
	}
}
