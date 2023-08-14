using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Client
{
	public enum OrderStatusForSendingUpd
	{
		[Display(Name = "Доставлен")]
		Delivered,
		[Display(Name = "В пути")]
		EnRoute
	}

}
