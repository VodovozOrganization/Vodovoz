using Gamma.Widgets;
using QSOrmProject;
using QSOrmProject.RepresentationModel;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.JournalFilters.Cash
{
    [OrmDefaultIsFiltered(true)]
    public partial class CashTransferDocumentsFilter : RepresentationFilterBase<CashTransferDocumentsFilter>
    {
        public CashTransferDocumentsFilter()
        {
            this.Build();

            yenumCashTransferDocumentStatus.ItemsEnum = typeof(CashTransferDocumentStatuses);

            yenumCashTransferDocumentStatus.EnumItemSelected += (sender, e) => OnRefiltered();
        }

        public CashTransferDocumentStatuses? CashTransferDocumentStatus
        {
            get { return yenumCashTransferDocumentStatus.SelectedItem as CashTransferDocumentStatuses?; }
            set { 
                yenumCashTransferDocumentStatus.SelectedItem = value; 
                yenumCashTransferDocumentStatus.Sensitive = false;
            }
        }
    }
}
