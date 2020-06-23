using System;
using System.Threading.Tasks;

namespace SmsSendInterface
{
	/// <summary>
	/// Осуществляет отправку смс сообщений
	/// </summary>
	public interface ISmsSender
	{
		/// <summary>
		/// Синхронная отправка смс сообщения
		/// </summary>
		/// <returns>Результат отправки</returns>
		/// <param name="message">Смс сообщение</param>
		ISmsSendResult SendSms(ISmsMessage message);

		/// <summary>
		/// Асинхронная отправка смс сообщения
		/// </summary>
		/// <returns>Задача возвращающая результат отправки</returns>
		/// <param name="message">Смс сообщение</param>
		Task<ISmsSendResult> SendSmsAsync(ISmsMessage message);

		BalanceResponse GetBalanceResponse { get; }
	}
}
