using System;
using Vodovoz.Settings.Common;

namespace Email.Infrastructure.Generators
{
	public class EmailLinkGenerator : IEmailLinkGenerator
	{
		private readonly IEmailSettings _emailSettings;

		public EmailLinkGenerator(IEmailSettings emailSettings)
		{
			_emailSettings = emailSettings ?? throw new ArgumentNullException(nameof(emailSettings));
		}

		public string GetUnsubscribeLink(Guid guid) => $"{_emailSettings.UnsubscribeUrl}/{guid}";
	}
}
