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
		/// Формирование отправок сделок по отсрочке сети: по одной на каждый заказ выборки
		/// (заказы окна уведомления), по которому отправки ещё не было ни в одном состоянии
		/// </summary>
		/// <param name="orders">Заказы окна уведомления (результат IOrderRepository.GetChainStoreDelayOrdersAsync)</param>
		/// <param name="previouslySentOrderIds">Id заказов, по которым уже есть отправка (в любом состоянии)</param>
		/// <param name="creationDate">Дата и время сбора</param>
		/// <returns>Отправки со статусом "Требуется создание сделки"</returns>
		IEnumerable<ChainStoreDelayBitrixDeal> CreateDeals(
			IEnumerable<ChainStoreDelayOrderNode> orders,
			ISet<int> previouslySentOrderIds,
			DateTime creationDate);
	}
}
