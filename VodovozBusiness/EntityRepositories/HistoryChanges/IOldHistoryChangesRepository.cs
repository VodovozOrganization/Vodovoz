using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.EntityRepositories.HistoryChanges
{
	public interface IOldHistoryChangesRepository
	{
		OldChangedEntity GetLastOldChangedEntity(IUnitOfWork uow);
		void ChangeSetsArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ChangedEntitiesArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void FieldChangesArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ChangeSetsArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void ChangedEntitiesArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void FieldChangesArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo);
		void DeleteHistoryChangesTest(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo);
	}
}
