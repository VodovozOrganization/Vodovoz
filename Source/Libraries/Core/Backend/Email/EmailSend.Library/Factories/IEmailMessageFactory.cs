using Mailganer.Api.Client.Dto;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;

namespace EmailSend.Library.Factories
{
	/// <summary>
	/// Фабрика для создания EmailMessage
	/// </summary>
	public interface IEmailMessageFactory
	{
		/// <summary>
		/// Создает коллекцию EmailMessage из SendEmailMessage
		/// </summary>
		/// <param name="message">Письмо для отправки</param>
		/// <returns>Результат выполнения</returns>
		Result<IEnumerable<EmailMessage>> CreateEmailMessages(SendEmailMessage message);
	}
}
