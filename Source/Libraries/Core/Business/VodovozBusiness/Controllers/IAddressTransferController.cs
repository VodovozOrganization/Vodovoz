using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IAddressTransferController
	{
		void UpdateDocuments(RouteListItem from, RouteListItem to, IUnitOfWork uow);
	}
}