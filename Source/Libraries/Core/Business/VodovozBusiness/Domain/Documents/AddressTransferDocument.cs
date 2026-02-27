using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using NHibernate;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative (Gender = GrammaticalGender.Masculine,
        Nominative = "документ переноса адресов",
        NominativePlural = "документы переноса адресов")]
    [HistoryTrace]
	public class AddressTransferDocument : Document
    {
        #region Сохраняемые свойства

        private RouteList routeListFrom;
        [Display(Name = "От МЛ")]
        public virtual RouteList RouteListFrom {
            get => routeListFrom;
            set => SetField(ref routeListFrom, value);
        }

        private RouteList routeListTo;
        [Display(Name = "К МЛ")]
        public virtual RouteList RouteListTo {
            get => routeListTo;
            set => SetField(ref routeListTo, value);
        }
        
        private IList<AddressTransferDocumentItem> addressTransferDocumentItems = new List<AddressTransferDocumentItem>();
        [Display(Name = "Строки документа переноса адресов")]
        public virtual IList<AddressTransferDocumentItem> AddressTransferDocumentItems {
            get => addressTransferDocumentItems;
            set => SetField(ref addressTransferDocumentItems, value);
        }

        private GenericObservableList<AddressTransferDocumentItem> observableAddressTransferDocumentItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<AddressTransferDocumentItem> ObservableAddressTransferDocumentItems => 
            observableAddressTransferDocumentItems 
            ?? (observableAddressTransferDocumentItems = new GenericObservableList<AddressTransferDocumentItem>(AddressTransferDocumentItems));
        
        #endregion

        #region Приватные функии

        private void UpdateOperationsTime()
        {
            foreach(var addressTransfer in AddressTransferDocumentItems) {
                if(!NHibernateUtil.IsInitialized(addressTransfer.DriverNomenclatureTransferDocumentItems))
                    continue;
                
                foreach (var driverNomenclatureTransfer in addressTransfer.DriverNomenclatureTransferDocumentItems) {
                    if(driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationFrom != null
                       && driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationFrom.OperationTime != TimeStamp) 
                    {
                        driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationFrom.OperationTime = TimeStamp;
                    }
                    if(driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationTo != null
                       && driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationTo.OperationTime != TimeStamp) 
                    {
                        driverNomenclatureTransfer.EmployeeNomenclatureMovementOperationTo.OperationTime = TimeStamp;
                    }
                }
            }
        }

		#endregion

		public override void SetTimeStamp(DateTime value)
		{
			base.TimeStamp = value;
			if(!NHibernateUtil.IsInitialized(AddressTransferDocumentItems))
			{
				return;
			}

			UpdateOperationsTime();
		}
	}
}
