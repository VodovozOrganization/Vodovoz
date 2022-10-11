using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrderPaymentSettings: IOrderPaymentSettings
    {
        private IParametersProvider _parametersProvider;

        public OrderPaymentSettings(IParametersProvider parametersProvider)
        {
            _parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int DefaultSelfDeliveryPaymentFromId => _parametersProvider.GetIntValue("default_selfdelivery_paymentFrom_id");
    }
}
