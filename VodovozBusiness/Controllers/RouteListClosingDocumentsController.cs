using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
    public class RouteListClosingDocumentsController : IRouteListClosingDocumentsController
    {
        public RouteListClosingDocumentsController(IStandartNomenclatures standartNomenclaturesParameters, IEmployeeRepository employeeRepository, IRouteListRepository routeListRepository)
        {
            this.standartNomenclaturesParameters = standartNomenclaturesParameters ?? throw new ArgumentNullException(nameof(standartNomenclaturesParameters));
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
        }

        private readonly IStandartNomenclatures standartNomenclaturesParameters;
        private readonly IEmployeeRepository employeeRepository;
        private readonly IRouteListRepository routeListRepository;

        public void UpdateDocuments(RouteList routeList, IUnitOfWork uow)
        {
            switch(routeList.Status) {
                case RouteListStatus.OnClosing:
                    DeleteDocuments(routeList, uow);
                    break;
                case RouteListStatus.Closed:
                    CreateOrUpdateDocuments(uow, routeList);
                    break;
                default:
                    return;
            }
        }

        private void DeleteDocuments(RouteList routeList, IUnitOfWork uow)
        {
            var documents = uow.Session.QueryOver<DeliveryDocument>()
                .WhereRestrictionOn(x => x.RouteListItem.Id)
                .IsIn(routeList.Addresses.Select(x => x.Id).ToArray())
                .List();

            var discrepancyDocument = uow.Session.QueryOver<DriverDiscrepancyDocument>().
                Where(x => x.RouteList.Id == routeList.Id).Take(1).SingleOrDefault();

            foreach(var document in documents) {
                uow.Delete(document);
            }
            if(discrepancyDocument != null) {
                uow.Delete(discrepancyDocument);
            }
        }

        private void CreateOrUpdateDocuments(IUnitOfWork uow, RouteList routeList)
        {
            var standartReturnNomenclature = uow.GetById<Nomenclature>(standartNomenclaturesParameters.GetReturnedBottleNomenclatureId);

            var deliveryDocuments = uow.Session.QueryOver<DeliveryDocument>()
                .WhereRestrictionOn(x => x.RouteListItem.Id)
                .IsIn(routeList.Addresses.Select(x => x.Id).ToArray())
                .List();

            var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);
            
            CreateOrUpdateDeliveryDocuments(uow, routeList, employeeForCurrentUser, standartReturnNomenclature, ref deliveryDocuments);
            CreateOrUpdateDiscrepancyDocument(uow, routeList, employeeForCurrentUser, deliveryDocuments);
        }

        private void CreateOrUpdateDeliveryDocuments(IUnitOfWork uow,
            RouteList routeList,
            Employee employeeForCurrentUser,
            Nomenclature standartReturnNomenclature,
            ref IList<DeliveryDocument> deliveryDocuments)
        {
            foreach(var address in routeList.Addresses.Where(x => x.Status == RouteListItemStatus.Completed)) {
                var deliveryDocument = deliveryDocuments.FirstOrDefault(x => x.RouteListItem.Id == address.Id);
                if(deliveryDocument == null) {
                    deliveryDocument = new DeliveryDocument();
                    deliveryDocuments.Add(deliveryDocument);
                }

                if(deliveryDocument.Author == null)
                    deliveryDocument.Author = employeeForCurrentUser;

                if(deliveryDocument.TimeStamp == default(DateTime)) {
                    deliveryDocument.TimeStamp = DateTime.Now;
                }

                deliveryDocument.LastEditor = employeeForCurrentUser;
                deliveryDocument.LastEditedTime = DateTime.Now;
                deliveryDocument.RouteListItem = address;

                foreach(DeliveryDocumentItem deliveryDocumentItem in deliveryDocument.ObservableItems) {
                    uow.Delete(deliveryDocumentItem);
                }
                deliveryDocument.ObservableItems.Clear();

                foreach(var orderItem in address.Order.OrderItems) {
                    var newDeliveryDocumentItem = new DeliveryDocumentItem {
                        Document = deliveryDocument,
                        Amount = orderItem.ActualCount ?? 0,
                        Nomenclature = orderItem.Nomenclature,
                        Direction = DeliveryDirection.ToClient
                    };
                    newDeliveryDocumentItem.CreateOrUpdateOperations();
                    deliveryDocument.ObservableItems.Add(newDeliveryDocumentItem);
                }

                foreach(var orderEquipment in address.Order.OrderEquipments) {
                    var newDeliveryDocumentItem = new DeliveryDocumentItem {
                        Document = deliveryDocument,
                        Amount = orderEquipment.ActualCount.HasValue ? (decimal)orderEquipment.ActualCount : 0,
                        Nomenclature = orderEquipment.Nomenclature,
                        Direction = orderEquipment.Direction == Direction.Deliver
                            ? DeliveryDirection.ToClient
                            : DeliveryDirection.FromClient
                    };
                    newDeliveryDocumentItem.CreateOrUpdateOperations();
                    deliveryDocument.ObservableItems.Add(newDeliveryDocumentItem);
                }

                if(address.BottlesReturned != 0) {
                    var newDeliveryDocumentItem = new DeliveryDocumentItem {
                        Document = deliveryDocument,
                        Amount = address.BottlesReturned,
                        Nomenclature = standartReturnNomenclature,
                        Direction = DeliveryDirection.FromClient
                    };
                    newDeliveryDocumentItem.CreateOrUpdateOperations();
                    deliveryDocument.ObservableItems.Add(newDeliveryDocumentItem);
                }

                uow.Save(deliveryDocument);
            }
        }

        private void CreateOrUpdateDiscrepancyDocument(IUnitOfWork uow, RouteList routeList, Employee employeeForCurrentUser, IList<DeliveryDocument> deliveryDocuments)
        {
            DriverDiscrepancyDocument discrepancyDocument = uow.Session.QueryOver<DriverDiscrepancyDocument>()
                .Where(x => x.RouteList.Id == routeList.Id).Take(1).SingleOrDefault() ?? new DriverDiscrepancyDocument();

            if(discrepancyDocument.Author == null)
                discrepancyDocument.Author = employeeForCurrentUser;

            if(discrepancyDocument.TimeStamp == default(DateTime)) {
                discrepancyDocument.TimeStamp = DateTime.Now;
            }

            discrepancyDocument.LastEditor = employeeForCurrentUser;
            discrepancyDocument.LastEditedTime = DateTime.Now;
            discrepancyDocument.RouteList = routeList;

            foreach(DriverDiscrepancyDocumentItem item in discrepancyDocument.ObservableItems) {
                uow.Delete(item);
            }
            discrepancyDocument.ObservableItems.Clear();

            var unloadedNomenclatureNodes =
                routeListRepository.GetReturnsToWarehouse(uow, routeList.Id, Enum.GetValues(typeof(NomenclatureCategory)).Cast<NomenclatureCategory>().ToArray());
            
            foreach(var unloadedNode in unloadedNomenclatureNodes) {
                var deliveryDocItems = deliveryDocuments
                    .SelectMany(x => x.ObservableItems.Where(i => i.Nomenclature.Id == unloadedNode.NomenclatureId)).ToList();
                var deliveredAmount = deliveryDocItems.Sum(g => g.Amount);

                if(deliveredAmount != unloadedNode.Amount) {
                    var newDiscrepanceItem = new DriverDiscrepancyDocumentItem {
                        Document = discrepancyDocument,
                        Nomenclature = deliveryDocItems.FirstOrDefault()?.Nomenclature ?? uow.GetById<Nomenclature>(unloadedNode.NomenclatureId),
                        DiscrepancyReason = deliveredAmount > unloadedNode.Amount ? DiscrepancyReason.UnloadedDeficiently : DiscrepancyReason.UnloadedExcessively,
                        Amount = Math.Abs(deliveredAmount - unloadedNode.Amount)
                    };
                    newDiscrepanceItem.CreateOrUpdateOperations();
                    discrepancyDocument.ObservableItems.Add(newDiscrepanceItem);
                }
            }
            if(discrepancyDocument.ObservableItems.Any()) {
                uow.Save(discrepancyDocument);
            }
        }
    }
}