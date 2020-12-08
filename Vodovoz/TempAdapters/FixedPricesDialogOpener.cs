using QS.Tdi;

namespace Vodovoz.TempAdapters
{
    public class FixedPricesDialogOpener : IFixedPricesDialogOpener
    {
        public void OpenFixedPricesForSelfDelivery(int counterpartyId)
        {
            CounterpartyDlg counterpartyDlg = new CounterpartyDlg(counterpartyId);
            counterpartyDlg.OpenFixedPrices();
            TDIMain.MainNotebook.AddTab(counterpartyDlg);
        }

        public void OpenFixedPricesForDeliveryPoint(int deliveryPointId)
        {
            DeliveryPointDlg deliveryPointDlg = new DeliveryPointDlg(deliveryPointId);
            deliveryPointDlg.OpenFixedPrices();
            TDIMain.MainNotebook.AddTab(deliveryPointDlg);
        }
    }
}