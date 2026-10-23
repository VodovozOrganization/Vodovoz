using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Состояние отправки сделки по отсрочке сети в Битрикс24
	/// </summary>
	public enum ChainStoreDelayBitrixDealStatus
	{
		/// <summary>
		/// Отправка сохранена, сделка не создана
		/// </summary>
		[Display(Name = "Требуется создание сделки")]
		DealCreateRequired,

		/// <summary>
		/// Сделка создана, ожидается оплата заказа
		/// </summary>
		[Display(Name = "Сделка создана")]
		DealCreated,

		/// <summary>
		/// Заказ оплачен, сделка переведена в стадию закрытия
		/// </summary>
		[Display(Name = "Сделка закрыта при оплате")]
		DealClosedOnPayment,

		/// <summary>
		/// На момент оплаты заказа сделка уже находилась в завершённой стадии, не изменялась
		/// </summary>
		[Display(Name = "Сделка закрыта в Битрикс24")]
		DealClosedInBitrix,

		/// <summary>
		/// На момент оплаты заказа сделка не найдена в Битрикс24, не изменялась
		/// </summary>
		[Display(Name = "Сделка удалена в Битрикс24")]
		DealNotFound,

		/// <summary>
		/// Заказ оплачен до отправки сделки, сделка не создавалась
		/// </summary>
		[Display(Name = "Не отправлена: заказ оплачен до отправки")]
		NotSentOrderPaid
	}
}
