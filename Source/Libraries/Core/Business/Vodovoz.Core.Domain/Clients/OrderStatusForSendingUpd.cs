using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Clients
{
	public enum OrderStatusForSendingUpd
	{
		[Display(Name = "Доставлен")]
		Delivered,
		[Display(Name = "В пути")]
		EnRoute
	}

}
