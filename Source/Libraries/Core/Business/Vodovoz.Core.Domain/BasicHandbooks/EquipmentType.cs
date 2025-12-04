using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.BasicHandbooks
{
	/// <summary>
	/// Тип оборудования
	/// </summary>
	public enum EquipmentType
	{
		[Display(Name = "Кулер")]
		Cooler,
		[Display(Name = "Помпа")]
		Pump,
		[Display(Name = "Прочее")]
		Other
	}
}
