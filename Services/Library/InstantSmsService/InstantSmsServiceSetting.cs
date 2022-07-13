using System.ServiceModel;

namespace InstantSmsService
{
	public class InstantSmsServiceSetting
	{
		private static InstantSmsServiceSetting settingInstance;

		public static bool SendingAllowed => settingInstance != null;
		private static ChannelFactory<IInstantSmsService> factory;

		public static IInstantSmsService GetInstantSmsService()
		{
			if(!SendingAllowed) {
				return null;
			}

			factory = new ChannelFactory<IInstantSmsService>(
				new BasicHttpBinding { SendTimeout = new System.TimeSpan(0, 0, 10) },
				$"http://{settingInstance.serviceUrl}/InstantSmsService");
			return factory.CreateChannel();
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

		public static void Close()
		{
			factory?.Close();
		}
	}
}
