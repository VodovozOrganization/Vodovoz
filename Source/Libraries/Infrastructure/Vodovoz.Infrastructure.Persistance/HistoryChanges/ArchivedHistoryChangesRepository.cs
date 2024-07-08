using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using Vodovoz.Domain.HistoryChanges;
using Vodovoz.EntityRepositories.HistoryChanges;

namespace Vodovoz.Infrastructure.Persistance.HistoryChanges
{
	internal sealed class ArchivedHistoryChangesRepository : IArchivedHistoryChangesRepository
	{
		public ArchivedChangedEntity GetLastOldChangedEntity(IUnitOfWork uow)
		{
			var lastOldChangedEntityId = uow.Session.QueryOver<ArchivedChangedEntity>()
				.Select(Projections.Max<ArchivedChangedEntity>(oce => oce.Id))
				.SingleOrDefault<int>();

			return lastOldChangedEntityId == 0
				? null
				: uow.GetById<ArchivedChangedEntity>(lastOldChangedEntityId);
		}

		#region Архивация мониторинга

		public void ArchiveChangeSets(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";
			var _changeSetAlias = "hcs";

			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var hcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangeSet));
			var oldhcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangeSet));

			var changeSetColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeSet)).First();
			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();

			var query = $"INSERT IGNORE INTO {oldhcsPersister.TableName} "
				+ $"(SELECT DISTINCT {_changeSetAlias}.* "
				+ $"FROM {hcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {hcsPersister.TableName} AS {_changeSetAlias} ON {_changeSetAlias}.{hcsPersister.KeyColumnNames.First()} = {_changedEntityAlias}.{changeSetColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void ArchiveChangedEntities(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";

			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangedEntity));

			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {oldhcePersister.TableName} "
				+ $"(SELECT * "
				+ $"FROM {hcePersister.TableName} AS {_changedEntityAlias} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void ArchiveFieldChanges(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";
			var _fieldChangeAlias = "hc";

			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var hcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(FieldChange));
			var oldhcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedFieldChange));

			var changedEntityColumn = hcPersister.GetPropertyColumnNames(nameof(FieldChange.Entity)).First();
			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {oldhcPersister.TableName} "
				+ $"(SELECT DISTINCT {_fieldChangeAlias}.* "
				+ $"FROM {hcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {hcPersister.TableName} AS {_fieldChangeAlias} ON {_changedEntityAlias}.{hcePersister.KeyColumnNames.First()} = {_fieldChangeAlias}.{changedEntityColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		#region Для теста

		public void ArchiveChangeSetsTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";
			var _changeSetAlias = "hcs";

			var factory = uow.Session.SessionFactory;
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangedEntity));
			var hcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangeSet));
			var oldhcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangeSet));

			var changeSetColumn = oldhcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeSet)).First();
			var dateTimeColumn = oldhcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeTime)).First();

			var query = $"INSERT IGNORE INTO {hcsPersister.TableName} "
				+ $"(SELECT DISTINCT {_changeSetAlias}.* "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {oldhcsPersister.TableName} AS {_changeSetAlias} ON {_changeSetAlias}.{oldhcsPersister.KeyColumnNames.First()} = {_changedEntityAlias}.{changeSetColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void ArchiveChangedEntitiesTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";

			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangedEntity));

			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {hcePersister.TableName} "
				+ $"(SELECT * "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void ArchiveFieldChangesTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var _changedEntityAlias = "hce";
			var _fieldChangeAlias = "hc";

			var factory = uow.Session.SessionFactory;
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangedEntity));
			var hcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(FieldChange));
			var oldhcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedFieldChange));

			var changedEntityColumn = oldhcPersister.GetPropertyColumnNames(nameof(ArchivedFieldChange.Entity)).First();
			var dateTimeColumn = oldhcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {hcPersister.TableName} "
				+ $"(SELECT DISTINCT {_fieldChangeAlias}.* "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {oldhcPersister.TableName} AS {_fieldChangeAlias} ON {_changedEntityAlias}.{oldhcePersister.KeyColumnNames.First()} = {_fieldChangeAlias}.{changedEntityColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void DeleteHistoryChangesTest(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			var _changedEntityAlias = "hce";
			var _changeSetAlias = "hcs";
			var _fieldChangeAlias = "hc";

			var factory = uow.Session.SessionFactory;
			var oldHcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangedEntity));
			var oldHcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedChangeSet));
			var oldHcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ArchivedFieldChange));

			var changeSetColumn = oldHcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeSet)).First();
			var changeTimeColumn = oldHcePersister.GetPropertyColumnNames(nameof(ArchivedChangedEntity.ChangeTime)).First();
			var changedEntityColumn = oldHcPersister.GetPropertyColumnNames(nameof(ArchivedFieldChange.Entity)).First();

			var query = $"DELETE {_fieldChangeAlias}, {_changedEntityAlias}, {_changeSetAlias} "
				+ $"FROM {oldHcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {oldHcsPersister.TableName} AS {_changeSetAlias} ON {_changeSetAlias}.{oldHcsPersister.KeyColumnNames.First()} = {_changedEntityAlias}.{changeSetColumn} "
				+ $"LEFT JOIN {oldHcPersister.TableName} AS {_fieldChangeAlias} ON {_changedEntityAlias}.{oldHcePersister.KeyColumnNames.First()} = {_fieldChangeAlias}.{changedEntityColumn} "
				+ $"WHERE {_changedEntityAlias}.{changeTimeColumn} BETWEEN '{dateFrom:yyyy-MM-dd}' AND '{dateTo:yyyy-MM-dd HH:mm:ss}';";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		#endregion

		#endregion
	}
}
