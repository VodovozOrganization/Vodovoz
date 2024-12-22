using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Goods
{
	/// <summary>
	/// Способ загрузки
	/// </summary>
	public enum EquipmentWorkloadType
	{
		/// <summary>
		/// Верхний
		/// </summary>
		[Display(Name = "Верхний")]
		Top,
		/// <summary>
		/// Нижний
		/// </summary>
		[Display(Name = "Нижний")]
		Lower
	}
}
