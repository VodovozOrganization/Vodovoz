using Mailjet.Api.Abstractions;
using QS.DomainModel.UoW;
using RabbitMQ.MailSending;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.StoredEmails;

namespace EmailDebtNotificationWorker.Services.Common.Factories
{
	public interface IEmailMessageFactory
	{
		/// <summary>
		/// Создать сообщение для отправки
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="storedEmail">Электронное письмо для отправки</param>
		/// <param name="client">Клиент</param>
		/// <param name="organizationFullName">Полное название организации</param>
		/// <param name="organizationEmailForMailing">Почта организации для отправки</param>
		/// <param name="attachments">Вложения в письмо</param>
		/// <param name="emailAddress">Почта клиента для отправки</param>
		/// <param name="emailSubject">Тема письма</param>
		/// <param name="messageText">Тело письма</param>
		/// <returns></returns>
		SendEmailMessage CreateSendEmailMessage(
			IUnitOfWork uow,
			StoredEmail storedEmail,
			Counterparty client,
			string organizationFullName,
			string organizationEmailForMailing,
			IEnumerable<EmailAttachment> attachments,
			string emailAddress,
			string emailSubject,
			string messageText);

		/// <summary>
		/// Создать электронное письмо для отправки
		/// </summary>
		/// <param name="subject">Тема письма</param>
		/// <param name="email">Электронная почта</param>
		/// <param name="description">Описание письма</param>
		/// <returns>Электронное письмо для отправки</returns>
		StoredEmail CreateStoredEmail(
			string subject,
			string email, 
			string? description = null);
	}
}
