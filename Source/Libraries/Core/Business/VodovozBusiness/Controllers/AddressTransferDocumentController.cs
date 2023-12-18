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

		public AddressTransferController(IEmployeeRepository employeeRepository)
		{
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public void UpdateDocuments(RouteListItem from, RouteListItem to, IUnitOfWork uow, bool isRevert = false)
		{

			CreateOrUpdateAddressTransferDocuments(@from, to, uow, isRevert);
		}

		private void CreateOrUpdateAddressTransferDocuments(RouteListItem from, RouteListItem to, IUnitOfWork uow, bool isRevert = false)
		{
			var transferDocument = uow.Session.QueryOver<AddressTransferDocument>()
				.Where(x => x.RouteListFrom.Id == from.RouteList.Id)
				.And(x => x.RouteListTo.Id == to.RouteList.Id)
				.SingleOrDefault() ?? new AddressTransferDocument();

			var employeeForCurrentUser = employeeRepository.GetEmployeeForCurrentUser(uow);

			if(transferDocument.Author == null)
			{
				transferDocument.Author = employeeForCurrentUser;
			}
			if(transferDocument.TimeStamp == default(DateTime))
			{
				transferDocument.TimeStamp = DateTime.Now;
			}

			transferDocument.LastEditor = employeeForCurrentUser;
			transferDocument.LastEditedTime = DateTime.Now;
			transferDocument.RouteListFrom = from.RouteList;
			transferDocument.RouteListTo = to.RouteList;

			var newAddressTransferItem = new AddressTransferDocumentItem
			{
				Document = transferDocument,
				OldAddress = from,
				NewAddress = to,
				AddressTransferType = isRevert ? from.AddressTransferType : from.TransferedTo?.AddressTransferType
			};

			transferDocument.ObservableAddressTransferDocumentItems.Add(newAddressTransferItem);

			CreateDeliveryFreeBalanceTransferItems(newAddressTransferItem);

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

		private void CreateDeliveryFreeBalanceTransferItems(AddressTransferDocumentItem addressTransferItem)
		{
			foreach(var orderItem in addressTransferItem.OldAddress.Order.GetAllGoodsToDeliver())
			{
				var newDeliveryFreeBalanceTransferItem = new DeliveryFreeBalanceTransferItem
				{
					AddressTransferDocumentItem = addressTransferItem,
					RouteListFrom = addressTransferItem.OldAddress.RouteList,
					RouteListTo = addressTransferItem.NewAddress.RouteList,
					Nomenclature = orderItem.Nomenclature,
					Amount = orderItem.Amount
				};

				newDeliveryFreeBalanceTransferItem.CreateOrUpdateOperations();
				addressTransferItem.DeliveryFreeBalanceTransferItems.Add(newDeliveryFreeBalanceTransferItem);
			}
		}
	}
}
