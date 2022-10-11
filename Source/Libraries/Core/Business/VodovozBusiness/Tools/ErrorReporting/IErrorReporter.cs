using System;
using QS.ErrorReporting;
using QS.Project.Domain;

namespace Vodovoz.Tools
{
	public interface IErrorReporter
	{
		/// <summary>
		/// Название используемой базы данных
		/// </summary>
		string DatabaseName { get; }

		/// <summary>
		/// Название программного продукта
		/// </summary>
		string ProductName { get; }

		/// <summary>
		/// Версия программного продукта
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Редакция программного продукта
		/// </summary>
		string Edition { get; }

		/// <summary>
		/// Включение автоматической отправки отчетов
		/// </summary>
		bool AutomaticallySendEnabled { get; }

		/// <summary>
		/// Отправка отчета в автоматическом режиме.
		/// Отправляет только если свойство <see cref="AutomaticallySendEnabled"/> установлено в true;
		/// Использовать следует в коде, при обработке ошибок, если есть необходимость зарегистрировать обработанную ошибку.
		/// </summary>
		bool AutomaticSendErrorReport(string description, string email, UserBase user, params Exception[] exceptions);

		/// <summary>
		/// Отправка отчета в автоматическом режиме.
		/// Отправляет только если свойство <see cref="AutomaticallySendEnabled"/> установлено в true;
		/// Использовать следует в коде, при обработке ошибок, если есть необходимость зарегистрировать обработанную ошибку.
		/// </summary>
		bool AutomaticSendErrorReport(string description, UserBase user, params Exception[] exceptions);

		/// <summary>
		/// Отправка отчета в автоматическом режиме.
		/// Отправляет только если свойство <see cref="AutomaticallySendEnabled"/> установлено в true;
		/// Использовать следует в коде, при обработке ошибок, если есть необходимость зарегистрировать обработанную ошибку.
		/// </summary>
		bool AutomaticSendErrorReport(string description, params Exception[] exceptions);

		/// <summary>
		/// Отправка отчета в ручном режиме.
		/// При вызове обязательно отправляет отчет.
		/// Использовать следует в диалогах, информирующих пользователя об ошибке. Вызывать если пользователь сам нажал отправку.
		/// </summary>
		bool ManuallySendErrorReport(string description, string email, UserBase user, params Exception[] exceptions);
	}
}
