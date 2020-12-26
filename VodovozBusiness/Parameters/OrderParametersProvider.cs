using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrderParametersProvider : IOrderParametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public OrderParametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int PaymentByCardFromMobileAppId => parametersProvider.GetIntValue("PaymentByCardFromMobileAppId");
        public int PaymentByCardFromOnlineStoreId => parametersProvider.GetIntValue("PaymentByCardFromOnlineStoreId");
        public int PaymentByCardFromSiteId => parametersProvider.GetIntValue("PaymentByCardFromSiteId");
        public int PaymentByCardFromSmsId => parametersProvider.GetIntValue("sms_payment_by_card_from_id");
        public int OldInternalOnlineStoreId => parametersProvider.GetIntValue("OldInternalOnlineStoreId");
    }
}