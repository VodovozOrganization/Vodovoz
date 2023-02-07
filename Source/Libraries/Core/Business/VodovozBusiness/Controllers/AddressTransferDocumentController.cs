using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Controllers
{
    public class AddressTransferController
    {
        public AddressTransferController(IEmployeeRepository employeeRepository)
        {
            this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        }
        
        private readonly IEmployeeRepository employeeRepository;

        public void UpdateDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
            var transferredAddresses = from.Addresses
                .Where(x => to.Addresses.Contains(x.TransferedTo) && x.Status == RouteListItemStatus.Transfered)
                .ToList();

            if(!transferredAddresses.Any()) {
                DeleteAddressTransferDocuments(@from, to, uow);
            }
            else {
                CreateOrUpdateAddressTransferDocuments(@from, to, uow);
            }
        }

        private void DeleteAddressTransferDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
            var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
                .Where(x => x.RouteListFrom.Id == from.Id)
                .And(x => x.RouteListTo.Id == to.Id)
                .SingleOrDefault();

	        CleanRouteListFreeBalanceOperations(transferDocument);

            transferDocument.AddressTransferDocumentItems.Clear();

			uow.Delete(transferDocument);
        }

        private void CleanRouteListFreeBalanceOperations(AddressTransferDocument transferDocument)
        {
			var documentOperationsFrom = transferDocument.ObservableAddressTransferDocumentItems
				.SelectMany(x => x.DeliveryFreeBalanceTransferItems
					.Select(y => y.DeliveryFreeBalanceOperationTo));
			var documentOperationsTo = transferDocument.ObservableAddressTransferDocumentItems
				.SelectMany(x => x.DeliveryFreeBalanceTransferItems
					.Select(y => y.DeliveryFreeBalanceOperationFrom));
			var documentOperations = documentOperationsFrom.Concat(documentOperationsTo).ToArray();

			foreach(var operation in transferDocument.RouteListFrom.ObservableDeliveryFreeBalanceOperations.ToArray())
			{
				if(documentOperations.Contains(operation))
				{
					transferDocument.RouteListFrom?.ObservableDeliveryFreeBalanceOperations.Remove(operation);
				}
			}

			foreach(var operation in transferDocument.RouteListTo.ObservableDeliveryFreeBalanceOperations.ToArray())
			{
				if(documentOperations.Contains(operation))
				{
					transferDocument.RouteListTo?.ObservableDeliveryFreeBalanceOperations.Remove(operation);
				}
			}
		}

        private void CreateOrUpdateAddressTransferDocuments(RouteList from, RouteList to, IUnitOfWork uow)
        {
	        var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
                .Where(x => x.RouteListFrom.Id == from.Id)
                .And(x => x.RouteListTo.Id == to.Id)
                .SingleOrDefault() ?? new AddressTransferDocument();

			transferDocument.ObservableAddressTransferDocumentItems.Clear();

            var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);

            if(transferDocument.Author == null) {
                transferDocument.Author = employeeForCurrentUser;
            }
            if(transferDocument.TimeStamp == default(DateTime)) {
                transferDocument.TimeStamp = DateTime.Now;
            }

            transferDocument.LastEditor = employeeForCurrentUser;
            transferDocument.LastEditedTime = DateTime.Now;
            transferDocument.RouteListFrom = from;
            transferDocument.RouteListTo = to;

            var transferredAddresses = from.Addresses
                .Where(x => to.Addresses.Contains(x.TransferedTo) && x.Status == RouteListItemStatus.Transfered)
                .ToList();

            foreach (var address in transferredAddresses) {
                var newAddressTransferItem = new AddressTransferDocumentItem {
                    Document = transferDocument,
                    OldAddress = address,
                    NewAddress = address.TransferedTo,
                    AddressTransferType = address.TransferedTo.AddressTransferType
                };
                transferDocument.ObservableAddressTransferDocumentItems.Add(newAddressTransferItem);

				CreateDeliveryFreeBalanceTransferItems(newAddressTransferItem);
                
				if(newAddressTransferItem.AddressTransferType == AddressTransferType.NeedToReload) 
                {
                    continue;
                }

				CreateDriverNomenclatureTransferItems(newAddressTransferItem);
            }

            uow.Save(transferDocument);
        }

        private void CreateDriverNomenclatureTransferItems(AddressTransferDocumentItem newAddressTransferItem)
        {
			foreach(var orderItem in newAddressTransferItem.OldAddress.Order.OrderItems)
			{
				var newDriverNomenclatureTransferItem = new DriverNomenclatureTransferItem
				{
					DocumentItem = newAddressTransferItem,
					Amount = orderItem.Count,
					Nomenclature = orderItem.Nomenclature,
					DriverFrom = newAddressTransferItem.OldAddress.RouteList.Driver,
					DriverTo = newAddressTransferItem.OldAddress.TransferedTo.RouteList.Driver
				};

				newDriverNomenclatureTransferItem.CreateOrUpdateOperations();
				newAddressTransferItem.DriverNomenclatureTransferDocumentItems.Add(newDriverNomenclatureTransferItem);
			}

			foreach(var orderItem in newAddressTransferItem.OldAddress.Order.OrderEquipments.Where(x => x.Direction == Direction.Deliver))
			{
				var newDriverNomenclatureTransferItem = new DriverNomenclatureTransferItem
				{
					DocumentItem = newAddressTransferItem,
					Amount = orderItem.Count,
					Nomenclature = orderItem.Nomenclature,
					DriverFrom = newAddressTransferItem.OldAddress.RouteList.Driver,
					DriverTo = newAddressTransferItem.OldAddress.TransferedTo.RouteList.Driver
				};

				newDriverNomenclatureTransferItem.CreateOrUpdateOperations();
				newAddressTransferItem.DriverNomenclatureTransferDocumentItems.Add(newDriverNomenclatureTransferItem);
			}
		}

        private void CreateDeliveryFreeBalanceTransferItems(AddressTransferDocumentItem addressTransferItem)
		{
			foreach(var orderItem in addressTransferItem.OldAddress.Order.GetAllGoodsToDeliver())
			{
				var newDeliveryFreeBalanceTransferItem = new DeliveryFreeBalanceTransferItem
				{
					AddressTransferDocumentItem = addressTransferItem,
					RouteListFrom = addressTransferItem.OldAddress.RouteList,
					RouteListTo = addressTransferItem.OldAddress.TransferedTo.RouteList,
					Nomenclature = orderItem.Nomenclature,
					Amount = orderItem.Amount
				};

				newDeliveryFreeBalanceTransferItem.CreateOrUpdateOperations();
				addressTransferItem.DeliveryFreeBalanceTransferItems.Add(newDeliveryFreeBalanceTransferItem);
			}

			foreach(var orderItem in addressTransferItem.OldAddress.Order.OrderEquipments
						.Where(x => x.Direction == Direction.Deliver))
			{
				var newDeliveryFreeBalanceTransferItem = new DeliveryFreeBalanceTransferItem
				{
					AddressTransferDocumentItem = addressTransferItem,
					Amount = orderItem.Count,
					Nomenclature = orderItem.Nomenclature,
					RouteListFrom = addressTransferItem.OldAddress.RouteList,
					RouteListTo = addressTransferItem.OldAddress.TransferedTo.RouteList
				};

				newDeliveryFreeBalanceTransferItem.CreateOrUpdateOperations();
				addressTransferItem.DeliveryFreeBalanceTransferItems.Add(newDeliveryFreeBalanceTransferItem);
			}
		}
	}
}
