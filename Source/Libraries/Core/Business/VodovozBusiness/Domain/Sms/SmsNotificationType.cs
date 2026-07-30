using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sms
{
	public enum SmsNotificationType
	{
		[Display(Name = "При новом контрагенте")]
		NewClient,
		[Display(Name = "При низком балансе")]
		LowBalance,
		[Display(Name = "При недовозе в переносе 'автоперенос н/согл' ")]
		UndeliveryNotApproved,
		[Display(Name = "При переходе заказа в статус \"В пути\"")]
		OrderOnTheWay
	}
}
