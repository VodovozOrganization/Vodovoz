using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        Nominative = "строка документа переноса адресов",
        NominativePlural = "строки документа переноса адресов")]
    [HistoryTrace]
	public class AddressTransferDocumentItem : PropertyChangedBase, IDomainObject
    {
	    private IList<DeliveryFreeBalanceTransferItem> _deliveryFreeBalanceTransferItems = new List<DeliveryFreeBalanceTransferItem>();
	    private AddressTransferType? _addressTransferType;
		public virtual int Id { get; set; }

	    private AddressTransferDocument document;
        [Display(Name = "Документ переноса адресов")]
        public virtual AddressTransferDocument Document {
            get => document;
            set => SetField(ref document, value);
        }

        private RouteListItem oldAddress;
        [Display(Name = "Старый адрес")]
        public virtual RouteListItem OldAddress {
            get => oldAddress;
            set => SetField(ref oldAddress, value);
        }
        
        private RouteListItem newAddress;
        [Display(Name = "Новый адрес")]
        public virtual RouteListItem NewAddress {
            get => newAddress;
            set => SetField(ref newAddress, value);
        }

        [Display(Name = "Тип переноcа адреса")]
        public virtual AddressTransferType? AddressTransferType
		{
	        get => _addressTransferType;
	        set => SetField(ref _addressTransferType, value);
        }

		private IList<DriverNomenclatureTransferItem> driverNomenclatureTransferDocumentItems = new List<DriverNomenclatureTransferItem>();
        [Display(Name = "Переносы номенклатур между водителями")]
        public virtual IList<DriverNomenclatureTransferItem> DriverNomenclatureTransferDocumentItems {
            get => driverNomenclatureTransferDocumentItems;
            set => SetField(ref driverNomenclatureTransferDocumentItems, value);
        }

        [Display(Name = "Изменение баланса свободных остатков МЛ")]
        public virtual IList<DeliveryFreeBalanceTransferItem> DeliveryFreeBalanceTransferItems
        {
	        get => _deliveryFreeBalanceTransferItems;
	        set => SetField(ref _deliveryFreeBalanceTransferItems, value);
        }

        private GenericObservableList<DriverNomenclatureTransferItem> observableDriverNomenclatureTransferDocumentItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<DriverNomenclatureTransferItem> ObservableDriverNomenclatureTransferDocumentItems => 
            observableDriverNomenclatureTransferDocumentItems 
            ?? (observableDriverNomenclatureTransferDocumentItems = new GenericObservableList<DriverNomenclatureTransferItem>(DriverNomenclatureTransferDocumentItems));
        
    }
}