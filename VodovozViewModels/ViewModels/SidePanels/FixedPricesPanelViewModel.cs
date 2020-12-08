using System;
using System.Collections.Generic;
using QS.Commands;
using QS.DomainModel.NotifyChange;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.SidePanels
{
    public class FixedPricesPanelViewModel : UoWWidgetViewModelBase
    {
        private readonly IFixedPricesDialogOpener fixedPricesDialogOpener;
        private Counterparty counterparty;
        private DeliveryPoint deliveryPoint;
        
        public FixedPricesPanelViewModel(IFixedPricesDialogOpener fixedPricesDialogOpener)
        {
            this.fixedPricesDialogOpener = fixedPricesDialogOpener ?? throw new ArgumentNullException(nameof(fixedPricesDialogOpener));
        }

        public string Title => deliveryPoint == null ? "Для самовывоза" : "Для точки доставки ";
        
        public IEnumerable<NomenclatureFixedPrice> FixedPrices {
            get {
                if(deliveryPoint != null) {
                    return deliveryPoint.NomenclatureFixedPrices;
                }
                if (counterparty != null) {
                    return counterparty.NomenclatureFixedPrices;
                }
                return new List<NomenclatureFixedPrice>();
            }
        }

        public void Refresh(Counterparty counterparty, DeliveryPoint deliveryPoint)
        {
            this.counterparty = counterparty;
            this.deliveryPoint = deliveryPoint;
            Refresh();
        }

        private void Refresh()
        {
            OnPropertyChanged(nameof(FixedPrices));
            OnPropertyChanged(nameof(Title));
            OpenFixedPricesDialogCommand.RaiseCanExecuteChanged();
        }

        private DelegateCommand openFixedPricesDialogCommand;
        public DelegateCommand OpenFixedPricesDialogCommand {
            get {
                if (openFixedPricesDialogCommand == null) {
                    openFixedPricesDialogCommand = new DelegateCommand(
                        OpenFixedPricesDialog, 
                        () => counterparty != null || deliveryPoint != null
                    );
                }
                return openFixedPricesDialogCommand;
            }
        }

        private void OpenFixedPricesDialog()
        {
            if (deliveryPoint != null) {
                fixedPricesDialogOpener.OpenFixedPricesForDeliveryPoint(deliveryPoint.Id);
                return;
            }
            if (counterparty != null) {
                fixedPricesDialogOpener.OpenFixedPricesForSelfDelivery(counterparty.Id);
                return;
            }
        }
    }
}