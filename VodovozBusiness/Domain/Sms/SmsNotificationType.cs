using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sms
{
	public enum SmsNotificationType
	{
		[Display(Name = "При новом контрагенте")]
		NewClient,
		[Display(Name = "При низком балансе")]
		LowBalance
	}
}
