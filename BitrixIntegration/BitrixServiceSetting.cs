using System.ServiceModel;

namespace BitrixIntegration
{
	public class BitrixServiceSetting
	{
		private static BitrixServiceSetting settingInstance;

		public static bool SendingAllowed => settingInstance != null;

		public static IBitrixService GetEmailService()
		{
			if(!SendingAllowed) {
				return null;
			}
			return new ChannelFactory<IBitrixService>(new BasicHttpBinding(), string.Format("http://{0}/EmailService", settingInstance.serviceUrl))
				.CreateChannel();
		}

		public static void Init(string serviceUrl)
		{
			if(string.IsNullOrWhiteSpace(serviceUrl)) {
				return;
			}
			settingInstance = new BitrixServiceSetting(serviceUrl);
		}

		private string serviceUrl;

		private BitrixServiceSetting(string serviceUrl)
		{
			this.serviceUrl = serviceUrl;
		}
	}
}
