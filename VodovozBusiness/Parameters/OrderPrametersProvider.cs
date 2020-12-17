using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
    public class OrderPrametersProvider : IOrderPrametersProvider
    {
        private readonly ParametersProvider parametersProvider;

        public OrderPrametersProvider(ParametersProvider parametersProvider)
        {
            this.parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
        }

        public int PaymentByCardFromMobileAppId => parametersProvider.GetIntValue("PaymentByCardFromMobileAppId");

        public int PaymentByCardFromSiteId => parametersProvider.GetIntValue("PaymentByCardFromSiteId");
        
        public int OldInternalOnlineStoreId => parametersProvider.GetIntValue("OldInternalOnlineStoreId");
    }
}