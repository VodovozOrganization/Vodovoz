using NHibernate.Type;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallResult
	{
		[Display (Name = "Ничего")]
		Nothing,
		[Display (Name = "Заказ создан")]
		OrderCreated,
		[Display (Name = "Заказ подтвержден")]
		OrderAccepted
	}

	public class RoboatsCallResultStringType : EnumStringType<RoboatsCallResult> { }
}
