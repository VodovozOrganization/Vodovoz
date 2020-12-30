using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative (Gender = GrammaticalGender.Feminine,
        Nominative = "строка документа переноса адресов",
        NominativePlural = "строки документа переноса адресов")]
    public class AddressTransferDocumentItem : PropertyChangedBase, IDomainObject
    {
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

        private bool needToReload;
        [Display(Name = "Необходима загрузка")]
        public virtual bool NeedToReload {
            get => needToReload;
            set => SetField(ref needToReload, value);
        }
        
        private IList<DriverNomenclatureTransferItem> driverNomenclatureTransferDocumentItems = new List<DriverNomenclatureTransferItem>();
        [Display(Name = "Переносы номенклатур между водителями")]
        public virtual IList<DriverNomenclatureTransferItem> DriverNomenclatureTransferDocumentItems {
            get => driverNomenclatureTransferDocumentItems;
            set => SetField(ref driverNomenclatureTransferDocumentItems, value);
        }

        private GenericObservableList<DriverNomenclatureTransferItem> observableDriverNomenclatureTransferDocumentItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<DriverNomenclatureTransferItem> ObservableDriverNomenclatureTransferDocumentItems => 
            observableDriverNomenclatureTransferDocumentItems 
            ?? (observableDriverNomenclatureTransferDocumentItems = new GenericObservableList<DriverNomenclatureTransferItem>(DriverNomenclatureTransferDocumentItems));
        
    }
}