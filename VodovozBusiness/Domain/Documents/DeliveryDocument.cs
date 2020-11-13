using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "документы доставки",
        Nominative = "документ доставки")]
    public class DeliveryDocument : Document
    {
        #region Сохраняемые свойства

        public override DateTime TimeStamp {
            get => base.TimeStamp;
            set {
                base.TimeStamp = value;
                if(!NHibernateUtil.IsInitialized(Items))
                    return;
                UpdateOperationsTime();
            }
        }

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

        #region Публичные функции

        public virtual void UpdateItems(RouteListItem address, Nomenclature standartReturnNomenclature)
        {
            ObservableItems.Clear();
            
            foreach (var orderItem in address.Order.OrderItems) {
                var newDeliveryDocumentItem = new DeliveryDocumentItem {
                    Document = this,
                    Amount = orderItem.ActualCount.HasValue ? (decimal)orderItem.ActualCount : 0,
                    Nomenclature = orderItem.Nomenclature,
                    Direction = DeliveryDirection.ToClient
                };
                newDeliveryDocumentItem.CreateOrUpdateOperations();
                ObservableItems.Add(newDeliveryDocumentItem);
            }
            
            foreach (var orderEquipment in address.Order.OrderEquipments) {
                var newDeliveryDocumentItem = new DeliveryDocumentItem {
                    Document = this,
                    Amount = orderEquipment.ActualCount.HasValue ? (decimal)orderEquipment.ActualCount : 0,
                    Nomenclature = orderEquipment.Nomenclature,
                    Direction = orderEquipment.Direction == Direction.Deliver 
                        ? DeliveryDirection.ToClient 
                        : DeliveryDirection.FromClient
                };
                newDeliveryDocumentItem.CreateOrUpdateOperations();
                ObservableItems.Add(newDeliveryDocumentItem);
            }

            if(address.BottlesReturned != 0) {
                var newDeliveryDocumentItem = new DeliveryDocumentItem {
                    Document = this,
                    Amount = address.BottlesReturned,
                    Nomenclature = standartReturnNomenclature,
                    Direction = DeliveryDirection.FromClient
                };
                newDeliveryDocumentItem.CreateOrUpdateOperations();
                ObservableItems.Add(newDeliveryDocumentItem);
            }
        }

        #endregion

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