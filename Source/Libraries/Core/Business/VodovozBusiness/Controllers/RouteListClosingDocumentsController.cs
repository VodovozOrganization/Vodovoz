using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Controllers
{
	public class RouteListClosingDocumentsController : IRouteListClosingDocumentsController
    {
        public RouteListClosingDocumentsController(
			INomenclatureSettings nomenclatureSettings,
            IEmployeeRepository employeeRepository,
            IRouteListRepository routeListRepository)
        {
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
        }

		private readonly INomenclatureSettings _nomenclatureSettings;
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

            foreach(var document in documents) {
                uow.Delete(document);
            }

            var discrepancyDocument = uow.Session.QueryOver<DriverDiscrepancyDocument>().Where(x => x.RouteList.Id == routeList.Id).Take(1).SingleOrDefault();
            if(discrepancyDocument != null) {
                uow.Delete(discrepancyDocument);
            }
        }

        private void CreateOrUpdateDocuments(IUnitOfWork uow, RouteList routeList)
        {
            var standartReturnNomenclature = uow.GetById<Nomenclature>(_nomenclatureSettings.ReturnedBottleNomenclatureId);

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

				if(deliveryDocument.AuthorId == null)
				{
					deliveryDocument.AuthorId = employeeForCurrentUser.Id;
				}

				if(deliveryDocument.TimeStamp == default(DateTime)) {
                    deliveryDocument.TimeStamp = DateTime.Now;
                }

                deliveryDocument.LastEditorId = employeeForCurrentUser.Id;
                deliveryDocument.LastEditedTime = DateTime.Now;
                deliveryDocument.RouteListItem = address;

                foreach(DeliveryDocumentItem deliveryDocumentItem in deliveryDocument.ObservableItems) {
                    uow.Delete(deliveryDocumentItem);
                }
                deliveryDocument.ObservableItems.Clear();

                foreach(var orderItem in address.Order.OrderItems.Where(x => x.ActualCount.HasValue && x.ActualCount != 0)) {
                    Debug.Assert(orderItem.ActualCount != null, "orderItem.ActualCount != null");
                    
                    var newDeliveryDocumentItem = new DeliveryDocumentItem {
                        Document = deliveryDocument,
                        Amount = orderItem.ActualCount.Value,
                        Nomenclature = orderItem.Nomenclature,
                        Direction = DeliveryDirection.ToClient
                    };
                    newDeliveryDocumentItem.CreateOrUpdateOperations();
                    deliveryDocument.ObservableItems.Add(newDeliveryDocumentItem);
                }

                foreach(var orderEquipment in address.Order.OrderEquipments.Where(x => x.ActualCount.HasValue && x.ActualCount != 0)) {
                    Debug.Assert(orderEquipment.ActualCount != null, "orderEquipment.ActualCount != null");
                    
                    var newDeliveryDocumentItem = new DeliveryDocumentItem {
                        Document = deliveryDocument,
                        Amount = (decimal)orderEquipment.ActualCount,
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

        /// <summary>
        /// Высчитывает все недосданные и пересданные водителем номенклатуры и записывает их в <see cref="DriverDiscrepancyDocument"/>
        /// </summary>
        private void CreateOrUpdateDiscrepancyDocument(IUnitOfWork uow, RouteList routeList, Employee employeeForCurrentUser, IList<DeliveryDocument> deliveryDocuments)
        {
            var terminalNomenclatureId = _nomenclatureSettings.NomenclatureIdForTerminal;
            
            DriverDiscrepancyDocument discrepancyDocument = uow.Session.QueryOver<DriverDiscrepancyDocument>()
                .Where(x => x.RouteList.Id == routeList.Id).Take(1).SingleOrDefault() ?? new DriverDiscrepancyDocument();

            if(discrepancyDocument.AuthorId == null)
			{
				discrepancyDocument.AuthorId = employeeForCurrentUser.Id;
			}

			if(discrepancyDocument.TimeStamp == default(DateTime)) {
                discrepancyDocument.TimeStamp = DateTime.Now;
            }

            discrepancyDocument.LastEditorId = employeeForCurrentUser.Id;
            discrepancyDocument.LastEditedTime = DateTime.Now;
            discrepancyDocument.RouteList = routeList;

            foreach(DriverDiscrepancyDocumentItem item in discrepancyDocument.ObservableItems) {
                uow.Delete(item);
            }
            discrepancyDocument.ObservableItems.Clear();
            
            var loaded = routeListRepository.AllGoodsLoaded(uow, routeList)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);
            
            var transferedFromThisRL = routeListRepository.AllGoodsTransferredToAnotherDrivers(uow, routeList)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);
            
            var transferedToThisRL = routeListRepository.AllGoodsTransferredFromDrivers(uow, routeList)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);
            
            var delivered = routeListRepository.AllGoodsDelivered(deliveryDocuments)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);

            var receivedFromClient = routeListRepository.AllGoodsReceivedFromClient(deliveryDocuments)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);

            var unloaded = routeListRepository.GetReturnsToWarehouse(uow, routeList.Id)
                .ToDictionary(x => x.NomenclatureId, x => x.Amount);

            var nomenclatureIds = 
                loaded.Keys
                .Union(transferedFromThisRL.Keys)
                .Union(transferedToThisRL.Keys)
                .Union(delivered.Keys)
                .Union(receivedFromClient.Keys)
                .Union(unloaded.Keys)
                .Where(x => x != terminalNomenclatureId);
            
            foreach(var nomId in nomenclatureIds) {
                loaded.TryGetValue(nomId, out decimal loadedAmount);
                transferedFromThisRL.TryGetValue(nomId, out decimal transferedFromThisRLAmount);
                transferedToThisRL.TryGetValue(nomId, out decimal transferedToThisAmount);
                delivered.TryGetValue(nomId, out decimal deliveredAmount);
                receivedFromClient.TryGetValue(nomId, out decimal receivedFromClientAmount);
                unloaded.TryGetValue(nomId, out decimal unloadedAmount);

                var discrepancyValue = loadedAmount - transferedFromThisRLAmount + transferedToThisAmount - deliveredAmount + receivedFromClientAmount - unloadedAmount;
                if(discrepancyValue == 0)
                    continue;
                
                var newDiscrepanceItem = new DriverDiscrepancyDocumentItem {
                    Document = discrepancyDocument,
                    Nomenclature = uow.GetById<Nomenclature>(nomId),
                    DiscrepancyReason = discrepancyValue > 0 ? DiscrepancyReason.UnloadedDeficiently : DiscrepancyReason.UnloadedExcessively,
                    Amount = Math.Abs(discrepancyValue)
                };
                newDiscrepanceItem.CreateOrUpdateOperations();
                discrepancyDocument.ObservableItems.Add(newDiscrepanceItem);
            }
            
            if(discrepancyDocument.ObservableItems.Any()) {
                uow.Save(discrepancyDocument);
            }
        }
    }
}
