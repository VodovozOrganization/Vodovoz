using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
    public interface ITrackRepository
    {
        Track GetTrackByRouteListId(IUnitOfWork unitOfWork, int routeListId);
    }
}