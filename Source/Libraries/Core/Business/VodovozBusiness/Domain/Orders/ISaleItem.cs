using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public interface ISaleItem : IGetFixedPrice, IPrice
	{
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; set; }
		/// <summary>
		/// Пользовательская цена
		/// </summary>
		bool IsUserPrice { get; set; }
		/// <summary>
		/// Альтернативная цена
		/// </summary>
		bool IsAlternativePrice { get; set; }
	}
}
