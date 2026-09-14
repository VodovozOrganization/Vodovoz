using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Sms
{
	/// <summary>
	/// Тип смс уведомления
	/// </summary>
	public enum SmsNotificationType
	{
		/// <summary>
		/// При новом контрагенте
		/// </summary>
		[Display(Name = "При новом контрагенте")]
		NewClient,

		/// <summary>
		/// При низком балансе
		/// </summary>
		[Display(Name = "При низком балансе")]
		LowBalance,

		/// <summary>
		/// При недовозе в переносе 'автоперенос н/согл'
		/// </summary>
		[Display(Name = "При недовозе в переносе 'автоперенос н/согл' ")]
		UndeliveryNotApproved,

		/// <summary>
		/// Курьер в пути к клиенту (заказ выбран следующим)
		/// </summary>
		[Display(Name = "Курьер в пути к клиенту")]
		CourierOnTheWay
	}
}
