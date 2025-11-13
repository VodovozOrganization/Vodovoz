using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Тип помпы
	/// </summary>
	public enum PumpType
	{
		/// <summary>
		/// Механическая
		/// </summary>
		[Display(Name = "Механическая")]
		Mechanical,
		/// <summary>
		/// Электронная
		/// </summary>
		[Display(Name = "Электронная")]
		Electronic
	}
}
