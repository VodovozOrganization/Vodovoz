using System.ComponentModel.DataAnnotations;

namespace FastPaymentsApi.Contracts.Responses
{
	/// <summary>
	/// Статус ответа банка
	/// </summary>
	public enum ResponseStatus
	{
		/// <summary>
		/// Успешно
		/// </summary>
		[Display(Name = "Успешное выполнение запроса")]
		Success,
		/// <summary>
		/// Поле shop_id пусто
		/// </summary>
		[Display(Name = "Поле shop_id пусто")]
		ShopIdIsEmpty,
		/// <summary>
		/// Поле shop_passwd пусто
		/// </summary>
		[Display(Name = "Поле shop_passwd пусто")]
		ShopPasswdIsEmpty,
		/// <summary>
		/// Неверное значение в поле shop_id и/или shop_passwd
		/// </summary>
		[Display(Name = "Неверное значение в поле shop_id и/или shop_passwd")]
		ShopIdOrShopPasswdInvalidValue,
		/// <summary>
		/// Внутренняя ошибка системы
		/// </summary>
		[Display(Name = "Внутренняя ошибка системы")]
		InternalSystemError,
		/// <summary>
		/// Поле ticket пусто
		/// </summary>
		[Display(Name = "Поле ticket пусто")]
		TicketIsEmpty,
		/// <summary>
		/// Недопустимый IP-адрес. Отправка host2host запросов с данного адреса невозможна
		/// </summary>
		[Display(Name = "Недопустимый IP-адрес. Отправка host2host запросов с данного адреса невозможна")]
		InvalidIPAddress,
		/// <summary>
		/// Некорректный XML-запрос
		/// </summary>
		[Display(Name = "Некорректный XML-запрос")]
		InvalidXMLRequest,
		/// <summary>
		/// Пустой XML-запрос
		/// </summary>
		[Display(Name = "Пустой XML-запрос.")]
		XMLRequestIsEmpty,
		/// <summary>
		/// Неподдерживаемая кодировка запроса
		/// </summary>
		[Display(Name = "Неподдерживаемая кодировка запроса")]
		UnsupportedRequestEncoding,
		/// <summary>
		/// Некорректный формат суммы
		/// </summary>
		[Display(Name = "Некорректный формат суммы")]
		InvalidAmountFormat,
		/// <summary>
		/// Неизвестная ошибка
		/// </summary>
		[Display(Name = "Неизвестная ошибка")]
		UnknownError
	}
}
