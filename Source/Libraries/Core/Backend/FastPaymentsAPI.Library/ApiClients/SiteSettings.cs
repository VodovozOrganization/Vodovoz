using Microsoft.Extensions.Configuration;
using System;
using Vodovoz.Settings.FastPayments;

namespace FastPaymentsAPI.Library.ApiClients
{
	public class SiteSettings : ISiteSettings
	{
		private readonly IConfiguration _configuration;
		private readonly IConfigurationSection _section;

		private readonly string _baseUrl;
		private readonly string _notifyOfFastPaymentStatusChangedUri;

		public SiteSettings(IConfiguration configuration)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_section = _configuration.GetSection("VodovozSiteNotificationService");
			_baseUrl = _section.GetValue<string>("BaseUrl");
			_notifyOfFastPaymentStatusChangedUri = _section.GetValue<string>("NotifyOfFastPaymentStatusChangedURI");
		}

		public string BaseUrl => _baseUrl;

		public string NotifyOfFastPaymentStatusChangedUri => _notifyOfFastPaymentStatusChangedUri;
	}
}
