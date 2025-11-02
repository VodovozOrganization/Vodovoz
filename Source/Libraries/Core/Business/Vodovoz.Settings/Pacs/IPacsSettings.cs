using System;

namespace Vodovoz.Settings.Pacs
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

		/// <summary>
		/// Адрес сервиса Api для операторов
		/// </summary>
		string OperatorApiUrl { get; }

		/// <summary>
		/// Ключ оператора для подключения к системе
		/// </summary>
		string OperatorApiKey { get; }

		/// <summary>
		/// Адрес сервиса Api для администраторов
		/// </summary>
		string AdministratorApiUrl { get; }

		/// <summary>
		/// Ключ администратора для настройки системы
		/// </summary>
		string AdministratorApiKey { get; }
	}
}
