using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Единицы производительности
	/// </summary>
	public enum ProductivityUnits
	{
		/// <summary>
		/// литр/час
		/// </summary>
		[Display(ShortName = "л/ч", Name = "литр/час")]
		LiterPerHour
	}
}
