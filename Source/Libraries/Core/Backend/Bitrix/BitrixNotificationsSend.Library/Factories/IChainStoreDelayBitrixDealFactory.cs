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
		/// Формирование отправок сделок по отсрочке сети: по одной на клиента, у которого есть заказ-инициатор.
		/// Заказ-инициатор - заказ с расчётной датой платежа в будущем, не позже чем через указанное число дней,
		/// который ещё не входил в состав прошлых отправок
		/// </summary>
		/// <param name="orders">Заказы сетей и тендеров по правилам отчёта "Отсрочка сети"</param>
		/// <param name="previouslySentOrderIds">Id заказов, входивших в состав прошлых отправок</param>
		/// <param name="counterpartyIdsWithTodayDeals">Id клиентов, по которым сегодня уже есть отправка</param>
		/// <param name="today">Сегодняшняя дата по Москве</param>
		/// <param name="daysBeforePaymentDate">За сколько дней до расчётной даты платежа заказ попадает в выгрузку</param>
		/// <param name="creationDate">Дата и время сбора</param>
		/// <returns>Отправки со статусом "Требуется создание сделки" и составом групп</returns>
		IEnumerable<ChainStoreDelayBitrixDeal> CreateDeals(
			IEnumerable<ChainStoreDelayOrderNode> orders,
			ISet<int> previouslySentOrderIds,
			ISet<int> counterpartyIdsWithTodayDeals,
			DateTime today,
			int daysBeforePaymentDate,
			DateTime creationDate);
	}
}
