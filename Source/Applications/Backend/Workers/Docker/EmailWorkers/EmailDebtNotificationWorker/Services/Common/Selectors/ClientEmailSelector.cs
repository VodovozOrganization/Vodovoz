using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Domain.Client;

namespace EmailDebtNotificationWorker.Services.Common.Selectors
{
	public class ClientEmailSelector : IClientEmailSelector
	{
		private readonly ILogger<ClientEmailSelector> _logger;

		public ClientEmailSelector(ILogger<ClientEmailSelector> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger))	; 
		}

		public string? SelectEmailForDebtNotification(Counterparty client, EmailPurpose? purpose = null)
		{
			if(client.Emails is null || !client.Emails.Any())
			{
				_logger.LogWarning("Клиент {ClientId} не имеет email адресов", client.Id);
				return null;
			}

			if(purpose.HasValue)
			{
				var emailForPurpose = client.Emails
					.LastOrDefault(x => x.EmailType?.EmailPurpose == purpose)
					?.Address;

				if(!string.IsNullOrWhiteSpace(emailForPurpose))
				{
					return emailForPurpose;
				}
			}

			var billEmail = client.Emails
				.LastOrDefault(x => x.EmailType?.EmailPurpose is EmailPurpose.ForBills)
				?.Address;

			if(!string.IsNullOrWhiteSpace(billEmail))
			{
				return billEmail;
			}

			var defaultEmail = client.Emails
				.LastOrDefault()
				?.Address;

			if(!string.IsNullOrWhiteSpace(defaultEmail))
			{
				return defaultEmail;
			}

			return null;
		}
	}
}
