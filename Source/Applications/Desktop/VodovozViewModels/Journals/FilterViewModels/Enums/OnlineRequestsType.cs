using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Enums
{
	public enum OnlineRequestsType
	{
		[Display(Name = "Онлайн заказы")]
		OnlineOrders,
		[Display(Name = "Заявки на звонок")]
		RequestsForCall
	}
}
