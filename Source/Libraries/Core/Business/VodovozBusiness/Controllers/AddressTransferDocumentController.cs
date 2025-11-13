using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Controllers
{
	public class AddressTransferController : IAddressTransferController
	{
		private readonly IEmployeeRepository employeeRepository;
		private readonly IRouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;

		public AddressTransferController(IEmployeeRepository employeeRepository, IRouteListAddressKeepingDocumentController routeListAddressKeepingDocumentController)
		{
			_routeListAddressKeepingDocumentController = routeListAddressKeepingDocumentController ?? throw new ArgumentNullException(nameof(routeListAddressKeepingDocumentController));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public void UpdateDocuments(RouteListItem from, RouteListItem to, IUnitOfWork uow, AddressTransferType addressTransferType)
		{
			CreateOrUpdateAddressTransferDocuments(@from, to, uow, addressTransferType);
		}

		private void CreateOrUpdateAddressTransferDocuments(RouteListItem from, RouteListItem to, IUnitOfWork uow, AddressTransferType addressTransferType)
		{
			var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
				.Where(x => x.RouteListFrom.Id == from.RouteList.Id)
				.And(x => x.RouteListTo.Id == to.RouteList.Id)
				.SingleOrDefault() ?? new AddressTransferDocument();

			var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);

			if(transferDocument.AuthorId == null)
			{
				transferDocument.AuthorId = employeeForCurrentUser.Id;
			}
			if(transferDocument.TimeStamp == default(DateTime))
			{
				transferDocument.TimeStamp = DateTime.Now;
			}

			transferDocument.LastEditorId = employeeForCurrentUser.Id;
			transferDocument.LastEditedTime = DateTime.Now;
			transferDocument.RouteListFrom = from.RouteList;
			transferDocument.RouteListTo = to.RouteList;

			var newAddressTransferItem = new AddressTransferDocumentItem
			{
				Document = transferDocument,
				OldAddress = from,
				NewAddress = to,
				AddressTransferType = addressTransferType
			};

			transferDocument.ObservableAddressTransferDocumentItems.Add(newAddressTransferItem);

			CreateDeliveryFreeBalanceTransferItems(uow, newAddressTransferItem);

			if(newAddressTransferItem.AddressTransferType == AddressTransferType.FromHandToHand)
			{
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
					DriverTo = newAddressTransferItem.NewAddress.RouteList.Driver
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
					DriverTo = newAddressTransferItem.NewAddress.RouteList.Driver
				};

				newDriverNomenclatureTransferItem.CreateOrUpdateOperations();
				newAddressTransferItem.DriverNomenclatureTransferDocumentItems.Add(newDriverNomenclatureTransferItem);
			}
		}

		private void CreateDeliveryFreeBalanceTransferItems(IUnitOfWork uow, AddressTransferDocumentItem addressTransferItem)
		{
			_routeListAddressKeepingDocumentController.CreateDeliveryFreeBalanceTransferItems(uow, addressTransferItem);
		}
	}
}
