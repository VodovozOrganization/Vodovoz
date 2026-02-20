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
        Nominative = "документ расхождения разгрузки водителя",
        NominativePlural = "документы расхождения разгрузки водителя")]
    public class DriverDiscrepancyDocument : Document
    {
        #region Сохраняемые свойства

        private RouteList routeList;
        [Display(Name = "Маршрутный лист")]
        public virtual RouteList RouteList {
            get => routeList;
            set => SetField(ref routeList, value);
        }
        
        private IList<DriverDiscrepancyDocumentItem> items = new List<DriverDiscrepancyDocumentItem>();
        [Display(Name = "Строки документа расхождения разгрузки водителя")]
        public virtual IList<DriverDiscrepancyDocumentItem> Items {
            get => items;
            set => SetField(ref items, value);
        }

        private GenericObservableList<DriverDiscrepancyDocumentItem> observableItems;
        //FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
        public virtual GenericObservableList<DriverDiscrepancyDocumentItem> ObservableItems => 
            observableItems ?? (observableItems = new GenericObservableList<DriverDiscrepancyDocumentItem>(Items));

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
