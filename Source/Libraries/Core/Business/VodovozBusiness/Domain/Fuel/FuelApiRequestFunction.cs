using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public enum FuelApiRequestFunction
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "Авторизация")]
		Login,
		[Display(Name = "Список карт")]
		FuelCardsData,
		[Display(Name = "Информация по карте")]
		FuelCardDetails,
		[Display(Name = "Список лимитов карты")]
		FuelCardsLimitsData,
		[Display(Name = "Удаление лимитов карты")]
		FuelCardsLimitsDelete,
		[Display(Name = "Создание лимита карты")]
		FuelCardsLimitCreate,
		[Display(Name = "Список транзакций")]
		TransactionsData
	}
}
