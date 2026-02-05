using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "документы доставки",
        Nominative = "документ доставки")]
    public class DeliveryDocument : Document
    {
        #region Сохраняемые свойства

        private RouteListItem routeListItem;
        public virtual RouteListItem RouteListItem {
            get => routeListItem;
            set => SetField(ref routeListItem, value, () => RouteListItem);
        }
        
        private IList<DeliveryDocumentItem> items = new List<DeliveryDocumentItem>();
        [Display(Name = "Строки документа доставки")]
        public virtual IList<DeliveryDocumentItem> Items {
            get => items;
            set => SetField(ref items, value, () => Items);
        }

        private GenericObservableList<DeliveryDocumentItem> observableItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<DeliveryDocumentItem> ObservableItems => 
            observableItems ?? (observableItems = new GenericObservableList<DeliveryDocumentItem>(Items));

		#endregion

		public override void SetTimeStamp(DateTime value)
		{
			base.TimeStamp = value;
			if(!NHibernateUtil.IsInitialized(Items))
			{
				return;
			}

			UpdateOperationsTime();
		}

		#region Приватные функии

		private void UpdateOperationsTime()
        {
            foreach(var item in Items) {
                if(item.EmployeeNomenclatureMovementOperation != null && item.EmployeeNomenclatureMovementOperation.OperationTime != TimeStamp)
                    item.EmployeeNomenclatureMovementOperation.OperationTime = TimeStamp;
            }
        }

        #endregion
    }
}
