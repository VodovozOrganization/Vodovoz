using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Fuel
{
	public class FuelApiRequest : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual DateTime RequestDateTime { get; set; }
		public virtual FuelApiRequestFunction RequestFunction { get; set; }
		public virtual FuelApiResponseResult ResponseResult { get; set; }
		public virtual string ErrorResponseMessage { get; set; }
	}

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

	public enum FuelApiResponseResult
	{
		[Display(Name = "Не указано")]
		None,
		[Display(Name = "Успех")]
		Success,
		[Display(Name = "Ошибка")]
		Error
	}
}
