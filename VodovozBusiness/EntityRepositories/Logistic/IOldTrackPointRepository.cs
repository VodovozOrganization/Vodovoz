using QS.DomainModel.UoW;
using System;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IOldTrackPointRepository
	{
		DateTime GetMaxOldTrackPointDate(IUnitOfWork uow);
		void TrackPointsArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
	}
}
