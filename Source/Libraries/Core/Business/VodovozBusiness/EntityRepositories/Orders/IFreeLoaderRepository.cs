using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Nodes;

namespace VodovozBusiness.EntityRepositories.Orders
{
	public interface IFreeLoaderRepository
	{
		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersByAddress(
			IUnitOfWork uow,
			int orderId,
			DeliveryPoint deliveryPoint);

		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByCounterpartyPhones(
			IUnitOfWork uow,
			int orderId,
			IEnumerable<Phone> phones);

		IEnumerable<FreeLoaderInfoNode> GetPossibleFreeLoadersInfoByDeliveryPointPhones(
			IUnitOfWork uow,
			IEnumerable<int> excludeOrderIds,
			IEnumerable<Phone> phones);
	}
}
