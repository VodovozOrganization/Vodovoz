using NHibernate.Type;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallOperation
	{
		[Display(Name = "Создание обработчика")]
		OnCreateHandler,
		[Display (Name = "При обработке запроса клиента")]
		OnClientHandle,
		[Display (Name = "Проверка клиента")]
		ClientCheck,
		[Display (Name = "Получение имени клиента")]
		GetClientName,
		[Display (Name = "Получение отчества клиента")]
		GetClientPatronymic,
		[Display (Name = "При обработке запроса адреса")]
		OnAddressHandle,
		[Display (Name = "Получение точек доставок")]
		GetDeliveryPoints,
		[Display (Name = "Получение кода улицы")]
		GetStreetId,
		[Display (Name = "Получение номера дома")]
		GetHouseNumber,
		[Display (Name = "Получение номера корпуса")]
		GetCorpusNumber,
		[Display (Name = "Получение номера квартиры")]
		GetApartmentNumber,
		[Display (Name = "При обработке запроса интервалов доставок")]
		OnDeliveryIntervalsHandle,
		[Display (Name = "Получение интервалов доставок")]
		GetDeliveryIntervals,
		[Display (Name = "При обработке запроса последнего заказа")]
		GetLastOrderCheck,
		[Display (Name = "Получение кода последнего заказа")]
		GetLastOrderId,
		[Display (Name = "Получение информации о воде")]
		GetWaterInfo,
		[Display (Name = "Получение тары на возврат")]
		GetBottlesReturn,
		[Display (Name = "При обработке запроса типов воды")]
		OnWaterTypeHandle,
		[Display (Name = "При обработке запроса заказа")]
		OnOrderHandle,
		[Display (Name = "Расчет цены заказа")]
		CalculateOrderPrice,
		[Display (Name = "Создание заказа")]
		CreateOrder,
		[Display (Name = "Закрытие устаревших звонков")]
		ClosingStaleCalls
	}

	public class RoboatsCallOperationStringType : EnumStringType<RoboatsCallOperation> { }
}
