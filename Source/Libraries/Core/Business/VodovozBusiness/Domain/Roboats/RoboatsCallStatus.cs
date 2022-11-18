using NHibernate.Type;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallStatus
	{
		[Display (Name = "В процессе")]
		InProgress,
		[Display (Name = "Прерван")]
		Aborted,
		[Display (Name = "Проблема")]
		Fail,
		[Display (Name = "Выполнен")]
		Success
	}

	public class RoboatsCallStatusStringType : EnumStringType<RoboatsCallStatus> { }
}
