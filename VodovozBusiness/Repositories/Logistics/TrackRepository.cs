using System;
using Vodovoz.Domain.Logistic;
using QSOrmProject;

namespace Vodovoz.Repository.Logistics
{
	public static class TrackRepository
	{
		public static Track GetTrackForRouteList (IUnitOfWork uow, int routeListId)
		{
			Track trackAlias = null;

			return uow.Session.QueryOver<Track> (() => trackAlias)
				.Where (() => trackAlias.RouteList.Id == routeListId)
				.SingleOrDefault ();
		}
	}
}

