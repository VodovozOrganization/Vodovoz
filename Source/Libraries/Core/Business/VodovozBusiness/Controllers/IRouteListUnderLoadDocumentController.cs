using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
	public interface IRouteListUnderLoadDocumentController
	{
		void CreateOrUpdateCarUnderloadDocument(IUnitOfWork uow, RouteList routeList);
	}
}
