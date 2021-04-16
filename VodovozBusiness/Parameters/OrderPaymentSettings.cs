using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrderPaymentSettings: IOrderPaymentSettings
    {
        private ParametersProvider parametersProvider;

        public OrderPaymentSettings()
        {
            parametersProvider = ParametersProvider.Instance;
        }

        public int DefaultSelfDeliveryPaymentFromId{
            get => parametersProvider.GetIntValue("default_selfdelivery_paymentFrom_id");        
        }
    }
}
