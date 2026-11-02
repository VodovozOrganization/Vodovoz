using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Тип применения скидки
	/// </summary>
	public enum UseDiscountType
	{
		/// <summary>
		/// Суммируется
		/// </summary>
		[Display(Name = "Суммируется")]
		AddUp,
		/// <summary>
		/// Не применяется
		/// </summary>
		[Display(Name = "Не применяется")]
		NotApplicable
	}
}
