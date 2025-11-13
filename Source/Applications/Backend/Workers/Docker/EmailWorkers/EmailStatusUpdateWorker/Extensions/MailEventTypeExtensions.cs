using Mailjet.Api.Abstractions.Events;
using System;
using Vodovoz.Domain.StoredEmails;

namespace EmailStatusUpdateWorker.Extensions
{
	public static class MailEventTypeExtensions
	{
		public static StoredEmailStates MapToStoredEmailStates(this MailEventType mailEventType) => mailEventType switch
		{
			MailEventType.sent => StoredEmailStates.Delivered,
			MailEventType.open => StoredEmailStates.Opened,
			MailEventType.spam => StoredEmailStates.MarkedAsSpam,
			MailEventType.bounce => StoredEmailStates.SendingError,
			MailEventType.blocked => StoredEmailStates.Undelivered,
			_ => throw new ArgumentOutOfRangeException(nameof(mailEventType), $"Тип события {mailEventType} не поддерживается"),
		};
	}
}
