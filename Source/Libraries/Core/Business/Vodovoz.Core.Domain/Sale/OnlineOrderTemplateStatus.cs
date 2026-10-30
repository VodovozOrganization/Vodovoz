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
		/// На обработке у менеджера
		/// </summary>
		[Display(Name = "На обработке у менеджера")]
		ManualProcessing,

		/// <summary>
		/// Активен
		/// </summary>
		[Display(Name = "Активен")]
		Active,

		/// <summary>
		/// Остановлен
		/// </summary>
		[Display(Name = "Остановлен")]
		Paused,

		/// <summary>
		/// Проблема с оплатой
		/// </summary>
		[Display(Name = "Проблема с оплатой")]
		PaymentProblem,

		/// <summary>
		/// Архивирован
		/// </summary>
		[Display(Name = "Архивирован")]
		Archived
	}
}
