using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Sale
{
	/// <summary>
	/// Статус шаблона автозаказа
	/// </summary>
	public enum OnlineOrderTemplateStatus
	{
		/// <summary>
		/// Ожидание оплаты
		/// </summary>
		[Display(Name = "Ожидание оплаты")]
		AwaitingPayment,
		/// <summary>
		/// Проблема с оплатой
		/// </summary>
		[Display(Name = "Проблема с оплатой")]
		PaymentProblem,
		/// <summary>
		/// Активен
		/// </summary>
		[Display(Name = "Активен")]
		Active,
		/// <summary>
		/// Приостановлен
		/// </summary>
		[Display(Name = "Приостановлен")]
		Inactive,
		/// <summary>
		/// Архивирован
		/// </summary>
		[Display(Name = "Архивирован")]
		Archived
	}
}
