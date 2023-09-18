using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum EquipmentWorkloadType
	{
		[Display(Name = "Верхняя")]
		Top,
		[Display(Name = "Нижняя")]
		Lower
	}
}
