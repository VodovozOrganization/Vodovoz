using System.Collections.Generic;
using Vodovoz.Domain.Employees;

namespace DriverAPI.Services
{
	/// <summary>
	/// Сервис отправки WakeUp-PUSH-сообщений
	/// </summary>
	public interface IWakeUpDriverClientService
	{
		/// <summary>
		/// Список клиентов для рассылки
		/// </summary>
		IReadOnlyDictionary<int, string> Clients { get; }

		/// <summary>
		/// Подписка на WakeUp-PUSH-сообщения
		/// </summary>
		/// <param name="driver">Сотрудник</param>
		/// <param name="token">Токен</param>
		void Subscribe(Employee driver, string token);

		/// <summary>
		/// Отписка от WakeUp-PUSH-сообщений
		/// </summary>
		/// <param name="driver">Сотрудник</param>
		void UnSubscribe(Employee driver);
		void UnSubscribe(string recipientToken);
	}
}
