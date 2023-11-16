using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Payments
{
	public enum AllocationStatus
	{
		[Display(Name = "Распределено")]
		Accepted,
		[Display(Name = "Распределение отменено")]
		Cancelled
	}
}
