using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;

namespace EmailDebtNotificationWorker.Services.Common.Generators
{
	public interface IEmailBodyGenerator
	{
		/// <summary>
		/// Создает и возращает тело письма с претензией
		/// </summary>
		/// <param name="client">Клиент</param>
		/// <param name="contract">Контракт</param>
		/// <param name="debt">Долг клиента</param>
		/// <param name="unsubscribeUrl">Ссылка для отписки</param>
		/// <returns>Тело письма с претензией</returns>
		string GenerateClaimEmailBody(
			Counterparty client,
			CounterpartyContract contract,
			decimal debt,
			string unsubscribeUrl);

		/// <summary>
		/// Создает и возвращает тело информационного письма с задолженностью
		/// </summary>
		/// <param name="client">Клиент</param>
		/// <param name="orders">Коллекция заказов</param>
		/// <param name="documentNumbersDict">Словарь с номерами документов</param>
		/// <param name="unsubscribeUrl">Ссылка для отписки</param>
		/// <returns>Тело информационного письма с задолженностью</returns>
		string GenerateDebtEmailBody(
			Counterparty client,
			IEnumerable<Order> orders,
			Dictionary<int, string> documentNumbersDict,
			string unsubscribeUrl);

		/// <summary>
		/// Создает и возвращает тело письма о закрытии поставок
		/// </summary>
		/// <param name="debt">Долг клиента</param>
		/// <param name="DaysBeforeClosingDeliveries">Количество до закрытия поставок</param>
		/// <param name="unsubscribeUrl">Ссылка для отписки</param>
		/// <returns>Тело письма о закрытии поставок</returns>
		string GenerateClosingDeliveriesEmailBody(decimal debt, int daysBeforeClosingDeliveries, string unsubscribeUrl);
	}
}
