using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum EquipmentWorkloadType
	{
		[Display(Name = "Верхний")]
		Top,
		[Display(Name = "Нижний")]
		Lower
	}
}
