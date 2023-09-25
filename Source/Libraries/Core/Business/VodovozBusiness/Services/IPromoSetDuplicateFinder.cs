using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace Vodovoz.Services
{
	public interface IPromoSetDuplicateFinder
	{
		bool CheckDuplicatePromoSets(IUnitOfWork uow,  DeliveryPoint deliveryPoint, IEnumerable<Phone> phones);
	}
}
