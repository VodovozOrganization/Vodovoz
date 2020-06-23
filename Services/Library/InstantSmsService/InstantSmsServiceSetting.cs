using System.ServiceModel;

namespace InstantSmsService
{
	public class InstantSmsServiceSetting
	{
		private static InstantSmsServiceSetting settingInstance;

		public static bool SendingAllowed => settingInstance != null;

		public static IInstantSmsService GetInstantSmsService()
		{
			if(!SendingAllowed) {
				return null;
			}
			return new ChannelFactory<IInstantSmsService>(new BasicHttpBinding { SendTimeout = new System.TimeSpan(0, 0, 10)}, string.Format("http://{0}/InstantSmsService", settingInstance.serviceUrl))
				.CreateChannel();
		}

		public static void Init(string serviceUrl)
		{
			if(string.IsNullOrWhiteSpace(serviceUrl)) {
				return;
			}
			settingInstance = new InstantSmsServiceSetting(serviceUrl);
		}

		private string serviceUrl;

		private InstantSmsServiceSetting(string serviceUrl)
		{
			this.serviceUrl = serviceUrl;
		}
	}
}
