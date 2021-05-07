using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
    public interface ITrackRepository
    {
        Track GetTrackByRouteListId(int routeListId);
        void Save(Track track);
    }
}