using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Goods
{
	public enum LockerRefrigeratorType
	{
		[Display(Name = "Шкафчик")]
		Locker,
		[Display(Name = "Холодильник")]
		Refrigerator
	}
}
