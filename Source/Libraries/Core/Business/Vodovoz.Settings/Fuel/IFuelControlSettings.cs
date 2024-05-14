using System;

namespace Vodovoz.Settings.Fuel
{
	public interface IFuelControlSettings
	{
		/// <summary>
		/// Адрес API сервера
		/// </summary>
		string ApiBaseAddress { get; }

		/// <summary>
		/// Время жизни сессии после авторизации
		/// </summary>
		TimeSpan ApiSessionLifetime { get; }

		/// <summary>
		/// Таймаут запроса к Api для получения данных
		/// </summary>
		TimeSpan ApiRequesTimeout { get; }

		/// <summary>
		/// Лимит возвращаемого колличества транзакций на один запрос
		/// </summary>
		int TransactionsPerQueryLimit { get; }

		/// <summary>
		/// Id контракта организации на которую оформлены топливные карты
		/// </summary>
		string OrganizationContractId { get; }

		/// <summary>
		/// Дата на которую были успешно получены данные по транзакциям топлива за день
		/// </summary>
		DateTime FuelTransactionsPerDayLastUpdateDate { get; }

		/// <summary>
		/// Дата по которую были успешно получены данные по транзакциям за месяц (последний день месяца)
		/// </summary>
		DateTime FuelTransactionsPerMonthLastUpdateDate { get; }

		/// <summary>
		/// Id типа продукта "Топливо" в сервисе Газпромнефть
		/// </summary>
		string FuelProductTypeId {  get; }

		/// <summary>
		/// Id единицы измерения при выдаче лимита литрами
		/// </summary>
		string LiterUnitId { get; }

		/// <summary>
		/// Id валюты Рубль при выдаче лимита деньгами
		/// </summary>
		string RubleCurrencyId { get; }

		/// <summary>
		/// Обновление даты на которую были успешно получены данные по транзакциям за день
		/// </summary>
		/// <param name="value"></param>
		void SetFuelTransactionsPerDayLastUpdateDate(string value);

		/// <summary>
		/// Обновление даты по которую были успешно получены данные по транзакциям за месяц (последний день месяца)
		/// </summary>
		/// <param name="value"></param>
		void SetFuelTransactionsPerMonthLastUpdateDate(string value);
	}
}
