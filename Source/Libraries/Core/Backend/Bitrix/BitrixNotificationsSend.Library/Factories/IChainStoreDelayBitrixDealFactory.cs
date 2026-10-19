using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Orders;
using VodovozBusiness.EntityRepositories.Nodes;

namespace BitrixNotificationsSend.Library.Factories
{
	/// <summary>
	/// Фабрика отправок сделок по отсрочке сети в Битрикс24
	/// </summary>
	public interface IChainStoreDelayBitrixDealFactory
	{
		/// <summary>
		/// Формирование отправок сделок по отсрочке сети: по одной на каждый заказ окна оплаты,
		/// по которому отправки ещё не было ни в одном состоянии
		/// </summary>
		/// <param name="orders">Заказы сетей и тендеров по правилам отчёта "Отсрочка сети"</param>
		/// <param name="previouslySentOrderIds">Id заказов, по которым уже есть отправка (в любом состоянии)</param>
		/// <param name="today">Сегодняшняя дата по Москве</param>
		/// <param name="daysBeforePaymentDate">За сколько дней до расчётной даты платежа заказ попадает в выгрузку</param>
		/// <param name="creationDate">Дата и время сбора</param>
		/// <returns>Отправки со статусом "Требуется создание сделки"</returns>
		IEnumerable<ChainStoreDelayBitrixDeal> CreateDeals(
			IEnumerable<ChainStoreDelayOrderNode> orders,
			ISet<int> previouslySentOrderIds,
			DateTime today,
			int daysBeforePaymentDate,
			DateTime creationDate);
	}
}
