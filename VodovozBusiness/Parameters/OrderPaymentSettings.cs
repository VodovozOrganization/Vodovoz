using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrderPaymentSettings: IOrderPaymentSettings
    {
        private IParametersProvider parametersProvider;

        public OrderPaymentSettings()
        {
            parametersProvider = SingletonParametersProvider.Instance;
        }

        public int DefaultSelfDeliveryPaymentFromId{
            get => parametersProvider.GetIntValue("default_selfdelivery_paymentFrom_id");        
        }

		public int PaymentFromTerminalId{
			get => parametersProvider.GetIntValue("paymentfrom_terminal_id");
		}
	}
}
