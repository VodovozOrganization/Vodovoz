using QS.DomainModel.UoW;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
    public class TrackRepository : ITrackRepository
    {
        public Track GetTrackByRouteListId(IUnitOfWork unitOfWork, int routeListId)
        {
            return unitOfWork.Session.Query<Track>().Where(t => t.RouteList.Id == routeListId).SingleOrDefault();
        }
    }
}
