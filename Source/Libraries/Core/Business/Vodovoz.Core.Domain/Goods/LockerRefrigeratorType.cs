using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Шкафчик/Холодильник
	/// </summary>
	public enum LockerRefrigeratorType
	{
		/// <summary>
		/// Шкафчик
		/// </summary>
		[Display(Name = "Шкафчик")]
		Locker,
		/// <summary>
		/// Холодильник
		/// </summary>
		[Display(Name = "Холодильник")]
		Refrigerator
	}
}
