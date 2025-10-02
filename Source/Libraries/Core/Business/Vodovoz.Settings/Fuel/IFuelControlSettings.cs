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
		string FuelProductTypeId { get; }

		/// <summary>
		/// Id единицы измерения при выдаче лимита литрами
		/// </summary>
		string LiterUnitId { get; }

		/// <summary>
		/// Id валюты Рубль при выдаче лимита деньгами
		/// </summary>
		string RubleCurrencyId { get; }

		/// <summary>
		/// Максимальный суточный лимит на топливо для авто типа Легковая (Ларгус), л.
		/// </summary>
		int LargusMaxDailyFuelLimit { get; }

		/// <summary>
		/// Максимальный суточный лимит на топливо для авто типа Фура, л.
		/// </summary>
		int TruckMaxDailyFuelLimit { get; }

		/// <summary>
		/// Максимальный суточный лимит на топливо для авто типа Грузовой, л.
		/// </summary>
		int GAZelleMaxDailyFuelLimit { get; }

		/// <summary>
		/// Максимальный суточный лимит на топливо для авто типа Погрузчик, л.
		/// </summary>
		int LoaderMaxDailyFuelLimit { get; }
		
		/// <summary>
		/// Максимальный суточный лимит на топливо для авто типа Фургон (Transit Mini), л.
		/// </summary>
		int MinivanMaxDailyFuelLimit { get; }

		/// <summary>
		/// Максимальный число транзакций в топливном лимите для авто типа Легковая (Ларгус)
		/// </summary>
		int LargusFuelLimitMaxTransactionsCount { get; }

		/// <summary>
		/// Максимальный число транзакций в топливном лимите для авто типа Грузовной
		/// </summary>
		int GAZelleFuelLimitMaxTransactionsCount { get; }

		/// <summary>
		/// Максимальный число транзакций в топливном лимите для авто типа Фура
		/// </summary>
		int TruckFuelLimitMaxTransactionsCount { get; }

		/// <summary>
		/// Максимальный число транзакций в топливном лимите для авто типа Погрузчик
		/// </summary>
		int LoaderFuelLimitMaxTransactionsCount { get; }
		
		/// <summary>
		/// Максимальный число транзакций в топливном лимите для авто типа Фургон (Transit Mini)
		/// </summary>
		int MinivanFuelLimitMaxTransactionsCount { get; }

		/// <summary>
		/// Дата последнего обновления стоимости топлива
		/// </summary>
		DateTime FuelPricesLastUpdateDate { get; }

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

		/// <summary>
		/// Установка значения максимального суточного лимита на топливо для авто типа Фургон
		/// </summary>
		void SetLargusMaxDailyFuelLimit(int value);

		/// <summary>
		/// Установка значения максимального суточного лимита на топливо для авто типа Фура
		/// </summary>
		void SetTruckMaxDailyFuelLimit(int value);

		/// <summary>
		/// Установка значения максимального суточного лимита на топливо для авто типа Грузовой
		/// </summary>
		void SetGAZelleMaxDailyFuelLimit(int value);

		/// <summary>
		/// Установка значения максимального суточного лимита на топливо для авто типа Погрузчик
		/// </summary>
		void SetLoaderMaxDailyFuelLimit(int value);
		
		/// <summary>
		/// Установка значения максимального суточного лимита на топливо для авто типа Фургон (Transit Mini)
		/// </summary>
		void SetMinivanMaxDailyFuelLimit(int value);

		/// <summary>
		/// Установка значения даты последнего обновления стоимости топлива
		/// </summary>
		/// <param name="value"></param>
		void SetFuelPricesLastUpdateDate(string value);
	}
}
