using System;
namespace SmsSendInterface
{
	/// <summary>
	/// Статус отправки смс сообщения
	/// </summary>
	public enum SmsSentStatus
	{
		/// <summary>
		/// Сообщение принято сервером
		/// </summary>
		Accepted,

		/// <summary>
		/// Неверно заполнен номер телефона
		/// </summary>
		InvalidMobilePhone,

		/// <summary>
		/// Не заполнен текст сообщения
		/// </summary>
		TextIsEmpty,

		/// <summary>
		/// Неверный адрес(имя) отправителя
		/// </summary>
		SenderAddressInvalid,

		/// <summary>
		/// Недостаточно средств на счете для отправки сообщения1
		/// </summary>
		NotEnoughBalance,

		/// <summary>
		/// Неизвестная ошибка
		/// </summary>
		UnknownError
	}
}
