using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using QS.Commands;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.TempAdapters;

namespace Vodovoz.ViewModels.ViewModels.SidePanels
{
    public class FixedPricesPanelViewModel : UoWWidgetViewModelBase, IDisposable
    {
        private readonly IFixedPricesDialogOpener fixedPricesDialogOpener;
        private Domain.Client.Counterparty counterparty;
        private DeliveryPoint deliveryPoint;
        private readonly IPermissionResult _deliveryPointPermissionResult;
        private readonly IPermissionResult _counterpartyPermissionResult;
        private DelegateCommand _openFixedPricesDialogCommand;

        public FixedPricesPanelViewModel(IFixedPricesDialogOpener fixedPricesDialogOpener, ICommonServices commonServices)
        {
            this.fixedPricesDialogOpener = fixedPricesDialogOpener ?? throw new ArgumentNullException(nameof(fixedPricesDialogOpener));
            _deliveryPointPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(DeliveryPoint));
            _counterpartyPermissionResult = commonServices.CurrentPermissionService.ValidateEntityPermission(typeof(Domain.Client.Counterparty));
        }

        public string Title => deliveryPoint == null ? "Для самовывоза" : "Для точки доставки ";
        
        public IEnumerable<NomenclatureFixedPrice> FixedPrices {
            get {
                if(deliveryPoint != null) {
                    return deliveryPoint.NomenclatureFixedPrices.OrderBy(p => p.Nomenclature.Name, OrderByDirection.Ascending).ThenBy(p => p.MinCount, OrderByDirection.Ascending).ToList();
                }
                if (counterparty != null) {
                    return counterparty.NomenclatureFixedPrices.OrderBy(p => p.Nomenclature.Name, OrderByDirection.Ascending).ThenBy(p => p.MinCount, OrderByDirection.Ascending).ToList();
                }
                return new List<NomenclatureFixedPrice>();
            }
        }

        public void Refresh(Domain.Client.Counterparty counterparty, DeliveryPoint deliveryPoint)
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

        public DelegateCommand OpenFixedPricesDialogCommand
        {
	        get
	        {
		        return _openFixedPricesDialogCommand ?? (_openFixedPricesDialogCommand = new DelegateCommand(
			        OpenFixedPricesDialog,
			        () => (deliveryPoint != null && _deliveryPointPermissionResult.CanUpdate)
				        || (deliveryPoint == null && counterparty != null && _counterpartyPermissionResult.CanUpdate)
		        ));
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

		public void Dispose()
		{
			fixedPricesDialogOpener?.Dispose();
		}
    }
}
