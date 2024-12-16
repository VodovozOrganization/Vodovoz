using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	/// <summary>
	/// Тип охлаждения
	/// </summary>
	public enum CoolingType
	{
		/// <summary>
		/// Компрессорное
		/// </summary>
		[Display(Name = "Компрессорное")]
		Compressor,
		/// <summary>
		/// Электронное
		/// </summary>
		[Display(Name = "Электронное")]
		Electronic
	}
}
