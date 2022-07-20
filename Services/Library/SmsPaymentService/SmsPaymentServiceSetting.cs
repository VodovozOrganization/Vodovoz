using System.ServiceModel;

namespace SmsPaymentService
{
    public class SmsPaymentServiceSetting
    {
        private static SmsPaymentServiceSetting settingInstance;

        public static bool SendingAllowed => settingInstance != null;

        public static ISmsPaymentService GetSmsPaymentService()
        {
            if (!SendingAllowed)
            {
                return null;
            }
            return new ChannelFactory<ISmsPaymentService>(
                new BasicHttpBinding {SendTimeout = new System.TimeSpan(0,0,10)},
                $"http://{settingInstance.serviceUrl}/SmsPaymentService"
            ).CreateChannel();
        }

        public static void Init(string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
            {
                return;
            }
            settingInstance = new SmsPaymentServiceSetting(serviceUrl);
        }

        private string serviceUrl;

        private SmsPaymentServiceSetting(string serviceUrl)
        {
            this.serviceUrl = serviceUrl;
        }
    }
}
