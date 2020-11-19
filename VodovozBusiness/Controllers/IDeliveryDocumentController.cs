using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Controllers
{
    public interface IDeliveryDocumentController
    {
        void UpdateDocuments(RouteList routeList, IUnitOfWork uow);
    }
}