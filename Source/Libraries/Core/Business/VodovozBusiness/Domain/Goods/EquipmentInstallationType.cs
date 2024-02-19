using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum EquipmentInstallationType
	{
		[Display(Name = "Напольный")]
		Floor,
		[Display(Name = "Настольный")]
		Desktop,
		[Display(Name = "Встраиваемый")]
		Embedded
	}
}
