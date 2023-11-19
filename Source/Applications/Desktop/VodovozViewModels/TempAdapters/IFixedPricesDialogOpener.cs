using System;

namespace Vodovoz.TempAdapters
{
    public interface IFixedPricesDialogOpener : IDisposable
    {
        void OpenFixedPricesForSelfDelivery(int counterpartyId);
        void OpenFixedPricesForDeliveryPoint(int deliveryPointId);
    }
}
