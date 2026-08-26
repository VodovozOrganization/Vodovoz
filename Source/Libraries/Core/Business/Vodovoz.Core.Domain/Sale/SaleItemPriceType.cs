using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Тип цены продаваемой позиции
	/// </summary>
	public enum SaleItemPriceType
	{
		/// <summary>
		/// Прайсовая
		/// </summary>
		[Display(Name = "Прайсовая")]
		General,
		/// <summary>
		/// Пользовательская
		/// </summary>
		[Display(Name = "Пользовательская")]
		User,
		/// <summary>
		/// Фикса
		/// </summary>
		[Display(Name = "Фикса")]
		Fixed,
		/// <summary>
		/// Альтернативная
		/// </summary>
		[Display(Name = "Альтернативная")]
		Alternative
	}
}
