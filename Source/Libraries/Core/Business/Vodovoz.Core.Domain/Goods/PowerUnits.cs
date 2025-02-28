using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Единицы мощности
	/// </summary>
	public enum PowerUnits
	{
		/// <summary>
		/// Ватт
		/// </summary>
		[Display(ShortName = "Вт", Name = "Ватт")]
		Watt
	}
}
