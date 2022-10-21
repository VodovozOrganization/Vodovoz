using QS.Tdi;

namespace Vodovoz.TempAdapters
{
    public interface IFixedPricesDialogOpener
    {
        void OpenFixedPricesForSelfDelivery(int counterpartyId);
        void OpenFixedPricesForDeliveryPoint(int deliveryPointId);
    }
}