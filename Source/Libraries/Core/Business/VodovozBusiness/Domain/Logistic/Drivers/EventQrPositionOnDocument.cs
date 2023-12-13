using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Logistic.Drivers
{
	public enum EventQrPositionOnDocument
	{
		[Display(Name = "Слева")]
		Left,
		[Display(Name = "Сверху")]
		Top,
		[Display(Name = "Справа")]
		Right,
		[Display(Name = "Снизу")]
		Bottom
	}
}
