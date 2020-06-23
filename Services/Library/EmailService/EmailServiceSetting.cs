using System.ServiceModel;

namespace EmailService
{
	public class EmailServiceSetting
	{
		private static EmailServiceSetting settingInstance;

		public static bool SendingAllowed => settingInstance != null;

		public static IEmailService GetEmailService()
		{
			if(!SendingAllowed) {
				return null;
			}
			return new ChannelFactory<IEmailService>(new BasicHttpBinding(), string.Format("http://{0}/EmailService", settingInstance.serviceUrl))
				.CreateChannel();
		}

		public static void Init(string serviceUrl)
		{
			if(string.IsNullOrWhiteSpace(serviceUrl)) {
				return;
			}
			settingInstance = new EmailServiceSetting(serviceUrl);
		}

		private string serviceUrl;

		private EmailServiceSetting(string serviceUrl)
		{
			this.serviceUrl = serviceUrl;
		}
	}
}
