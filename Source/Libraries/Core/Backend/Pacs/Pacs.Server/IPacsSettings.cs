using System;

namespace Pacs.Server
{
	public interface IPacsSettings
	{
		/// <summary>
		/// Сколько времени клиент может не передавать данные,
		/// до того как будет считаться отключенным
		/// </summary>
		TimeSpan OperatorInactivityTimeout { get; }

		/// <summary>
		/// С какой периодичностью оператор уведомляет о подключении
		/// </summary>
		TimeSpan OperatorKeepAliveInterval { get; }
	}
}
