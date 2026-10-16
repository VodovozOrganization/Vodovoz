using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Domain.Sale
{
	/// <summary>
	/// Интерфейс позиций на продажу
	/// </summary>
	public interface ISaleItems
	{
		/// <summary>
		/// Позиции на продажу
		/// </summary>
		IEnumerable<ISaleItem> SaleItems { get; }
	}
}
