using NHibernate.Type;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallFailType
	{
		[Display (Name = "Нет")]
		None,
		[Display(Name = "Служба отключена")]
		ServiceDisabled,
		[Display(Name = "Неизвестный запрос")]
		UnknownRequest,
		[Display (Name = "Найдено более 1 клиента")]
		ClientDuplicate,
		[Display (Name = "Не найден клиент")]
		ClientNotFound,
		[Display (Name = "Не найдено имя клиента")]
		ClientNameNotFound,
		[Display(Name = "Контрагент отключен")]
		ClientExcluded,
		[Display (Name = "Не найдено отчество клиента")]
		ClientPatronymicNotFound,
		[Display (Name = "Не найдены точки доставки")]
		DeliveryPointsNotFound,
		[Display (Name = "Улица не найдена")]
		StreetNotFound,
		[Display (Name = "Дом не найден")]
		HouseNotFound,
		[Display (Name = "Корпус не найден")]
		CorpusNotFound,
		[Display (Name = "Квартира не найдена")]
		ApartmentNotFound,
		[Display (Name = "Интервалы доставки не найдены")]
		DeliveryIntervalsNotFound,
		[Display (Name = "Заказ не найден")]
		OrderNotFound,
		[Display (Name = "Не найдена доступная вода")]
		AvailableWatersNotFound,
		[Display (Name = "Не найдена вода в заказе")]
		WaterInOrderNotFound,
		[Display (Name = "Тип воды не поддерживается")]
		WaterNotSupported,
		[Display (Name = "Не найдено количество тары на возврат")]
		BottlesReturnNotFound,		
		[Display (Name = "Не найден интервал заказа")]
		OrderIntervalNotFound,
		[Display(Name = "Неизвестный тип запроса")]
		UnknownRequestType,
		[Display(Name = "Некорректный код адреса")]
		IncorrectAddressId,
		[Display(Name = "Не указан код адреса")]
		AddressIdNotSpecified,
		[Display(Name = "Не указана вода")]
		WaterNotSpecified,
		[Display(Name = "Возникло исключение")]
		Exception,
		[Display(Name = "Рассчиталась отрицательная стоимость заказа")]
		NegativeOrderSum,
		[Display(Name = "Некорректный код заказа")]
		IncorrectOrderId,
		[Display(Name = "Некорректная дата заказа")]
		IncorrectOrderDate,
		[Display(Name = "Некорректный интервал заказа")]
		IncorrectOrderInterval,
		[Display (Name = "Неизвестное значение оплаты по терминалу")]
		UnknownIsTerminalValue,
		[Display (Name = "Неверное значение сдачи для заказа по наличке")]
		IncorrectTrifleForCashOrder,
		[Display (Name = "Таймаут открытого звонка")]
		TimeOut,
		[Display (Name = "Заказ содержит позиции, которые продаются от разных организаций")]
		OrderHasGoodsSoldFromSeveralOrganizations
	}

	public class RoboatsCallFailTypeStringType : EnumStringType<RoboatsCallFailType> { }
}
