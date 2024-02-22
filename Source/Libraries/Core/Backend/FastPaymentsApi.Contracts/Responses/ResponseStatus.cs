using System.ComponentModel.DataAnnotations;

namespace FastPaymentsApi.Contracts.Responses
{
	public enum ResponseStatus
	{
		[Display(Name = "Успешное выполнение запроса")]
		Success,
		[Display(Name = "Поле shop_id пусто")]
		ShopIdIsEmpty,
		[Display(Name = "Поле shop_passwd пусто")]
		ShopPasswdIsEmpty,
		[Display(Name = "Неверное значение в поле shop_id и/или shop_passwd")]
		ShopIdOrShopPasswdInvalidValue,
		[Display(Name = "Внутренняя ошибка системы")]
		InternalSystemError,
		[Display(Name = "Поле ticket пусто")]
		TicketIsEmpty,
		[Display(Name = "Недопустимый IP-адрес. Отправка host2host запросов с данного адреса невозможна")]
		InvalidIPAddress,
		[Display(Name = "Некорректный XML-запрос")]
		InvalidXMLRequest,
		[Display(Name = "Пустой XML-запрос.")]
		XMLRequestIsEmpty,
		[Display(Name = "Неподдерживаемая кодировка запроса")]
		UnsupportedRequestEncoding,
		[Display(Name = "Некорректный формат суммы")]
		InvalidAmountFormat,
		[Display(Name = "Неизвестная ошибка")]
		UnknownError
	}
}
