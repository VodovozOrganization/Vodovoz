using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.EntityRepositories.HistoryChanges
{
	public interface IArchiveHistoryChangesRepository
	{
		ArchiveChangedEntity GetLastOldChangedEntity(IUnitOfWork uow);
		void ArchiveChangeSets(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ArchiveChangedEntities(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ArchiveFieldChanges(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ArchiveChangeSetsTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ArchiveChangedEntitiesTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ArchiveFieldChangesTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void DeleteHistoryChangesTest(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
