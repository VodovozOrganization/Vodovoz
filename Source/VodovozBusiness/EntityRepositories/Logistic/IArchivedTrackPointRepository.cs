using QS.DomainModel.UoW;
using System;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface IArchivedTrackPointRepository
	{
		DateTime GetMaxOldTrackPointDate(IUnitOfWork uow);
		void ArchiveTrackPoints(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
	}
}
