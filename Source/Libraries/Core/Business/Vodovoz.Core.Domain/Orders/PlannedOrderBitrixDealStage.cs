using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Orders
{
	/// <summary>
	/// Стадия работы со сделкой в Битрикс24 по планируемому заказу
	/// </summary>
	public enum PlannedOrderBitrixDealStage
	{
		/// <summary>
		/// Сделка в Битрикс24 не создана
		/// </summary>
		[Display(Name = "Сделка не создана")]
		DealNotCreated,

		/// <summary>
		/// Сделка в Битрикс24 создана, ожидается создание заказа клиентом
		/// </summary>
		[Display(Name = "Сделка создана")]
		DealCreated,

		/// <summary>
		/// По планируемому заказу найден созданный клиентом заказ,
		/// требуется обновление сделки в Битрикс24
		/// </summary>
		[Display(Name = "Требуется обновление сделки")]
		DealUpdateRequired,

		/// <summary>
		/// Сделка обновлена данными созданного заказа и переведена в завершающую стадию
		/// </summary>
		[Display(Name = "Сделка завершена")]
		DealCompleted,

		/// <summary>
		/// Сделка на момент обновления уже находилась в завершённой стадии в Битрикс24,
		/// данные созданного заказа в неё не отправлялись
		/// </summary>
		[Display(Name = "Сделка закрыта в Битрикс24")]
		DealClosedInBitrix,

		/// <summary>
		/// Сделка не найдена в Битрикс24 (удалена), отслеживание прекращено
		/// </summary>
		[Display(Name = "Сделка не найдена в Битрикс24")]
		DealNotFound
	}
}
