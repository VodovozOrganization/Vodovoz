using System;
namespace SmsSendInterface
{
	/// <summary>
	/// Информация о смс сообщении
	/// </summary>
	public interface ISmsMessage
	{
		/// <summary>
		/// Номер мобильного телефона на который будет отправлено сообщение
		/// </summary>
		string MobilePhoneNumber { get; }

		/// <summary>
		/// Идентификатор сообщения на стороне клиента
		/// </summary>
		string LocalId{ get; }

		/// <summary>
		/// Отложенное время отправки
		/// </summary>
		DateTime ScheduleTime { get; }

		/// <summary>
		/// Текст смс сообщения
		/// </summary>
		string MessageText { get; }
	}
}
