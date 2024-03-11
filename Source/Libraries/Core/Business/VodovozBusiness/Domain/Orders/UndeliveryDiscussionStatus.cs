using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	public enum UndeliveryDiscussionStatus
	{
		[Display(Name = "В работе")]
		InProcess,
		[Display(Name = "На проверке")]
		Checking,
		[Display(Name = "Закрыт")]
		Closed
	}
}
