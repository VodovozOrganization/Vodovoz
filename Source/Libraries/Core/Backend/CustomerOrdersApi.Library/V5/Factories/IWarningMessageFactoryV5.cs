using CustomerOrders.Contracts.InfoMessages;

namespace CustomerOrdersApi.Library.V5.Factories
{
	/// <summary>
	/// Контракт фабрики предупреждений
	/// </summary>
	public interface IWarningMessageFactoryV5
	{
		/// <summary>
		/// Создание сообщения о не найденном логистическом районе
		/// </summary>
		/// <returns></returns>
		WarningMessage CreateDistrictNotFoundMessage();
		/// <summary>
		/// Создание сообщения об изменившейся цене доставки
		/// </summary>
		/// <returns></returns>
		WarningMessage CreateDeliveryChangedMessage(string message);
		/// <summary>
		/// Создание сообщения о недоступном промо наборе(ах)
		/// </summary>
		/// <returns></returns>
		WarningMessage CreatePromoSetUnavailableMessage();
		/// <summary>
		/// Создание сообщения о недоступном промокоде(ах)
		/// </summary>
		/// <returns></returns>
		WarningMessage CreatePromoCodeUnavailableMessage();
		/// <summary>
		/// Создание сообщения об изменившейся цене товара
		/// </summary>
		/// <returns></returns>
		WarningMessage CreatePriceChangedMessage();
		/// <summary>
		/// Создание сообщения о нехватке товара на складе
		/// </summary>
		/// <returns></returns>
		WarningMessage CreateOutOfStockMessage();
		/// <summary>
		/// Создание сообщения о нехватке всех позиций товара на складе
		/// </summary>
		/// <returns></returns>
		WarningMessage CreateAllOutOfStockMessage();
	}
}
