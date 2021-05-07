using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
    public class TrackRepository : ITrackRepository
    {
        private readonly IUnitOfWork unitOfWork;

        public TrackRepository(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public Track GetTrackByRouteListId(int routeListId)
        {
            return unitOfWork.Session.Query<Track>().Where(t => t.RouteList.Id == routeListId).SingleOrDefault();
        }

        public void Save(Track track)
        {
            unitOfWork.Save(track);
        }
    }
}
