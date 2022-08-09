using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Persister.Entity;
using QS.DomainModel.UoW;
using QS.HistoryLog.Domain;
using Vodovoz.Domain.HistoryChanges;

namespace Vodovoz.EntityRepositories.HistoryChanges
{
	public class OldHistoryChangesRepository : IOldHistoryChangesRepository
	{
		private const string _changedEntityAlias = "hce";
		private const string _changeSetAlias = "hcs";
		private const string _fieldChangeAlias = "hc";
		
		public OldChangedEntity GetLastOldChangedEntity(IUnitOfWork uow)
		{
			var lastOldChangedEntityId = uow.Session.QueryOver<OldChangedEntity>()
				.Select(Projections.Max<OldChangedEntity>(oce => oce.Id))
				.SingleOrDefault<int>();
			
			return lastOldChangedEntityId == 0
				? null
				: uow.GetById<OldChangedEntity>(lastOldChangedEntityId);
		}

		#region Архивация мониторинга

		public void ChangeSetsArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var hcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangeSet));
			var oldhcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangeSet));

			var changeSetColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeSet)).First();
			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {oldhcsPersister.TableName} "
				+ $"(SELECT DISTINCT {_changeSetAlias}.* "
				+ $"FROM {hcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {hcsPersister.TableName} AS {_changeSetAlias} ON {_changeSetAlias}.{hcsPersister.KeyColumnNames.First()} = {_changedEntityAlias}.{changeSetColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}
		
		public void ChangedEntitiesArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangedEntity));

			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(ChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {oldhcePersister.TableName} "
				+ $"(SELECT * "
				+ $"FROM {hcePersister.TableName} AS {_changedEntityAlias} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void FieldChangesArchiving(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var hcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(FieldChange));
			var oldhcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldFieldChange));

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

		public void ChangeSetsArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangedEntity));
			var hcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangeSet));
			var oldhcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangeSet));

			var changeSetColumn = oldhcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeSet)).First();
			var dateTimeColumn = oldhcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {hcsPersister.TableName} "
				+ $"(SELECT DISTINCT {_changeSetAlias}.* "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {oldhcsPersister.TableName} AS {_changeSetAlias} ON {_changeSetAlias}.{oldhcsPersister.KeyColumnNames.First()} = {_changedEntityAlias}.{changeSetColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void ChangedEntitiesArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var hcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(ChangedEntity));
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangedEntity));

			var dateTimeColumn = hcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {hcePersister.TableName} "
				+ $"(SELECT * "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void FieldChangesArchivingTest(IUnitOfWork uow, DateTime dateTimeFrom, DateTime dateTimeTo)
		{
			var factory = uow.Session.SessionFactory;
			var oldhcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangedEntity));
			var hcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(FieldChange));
			var oldhcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldFieldChange));

			var changedEntityColumn = oldhcPersister.GetPropertyColumnNames(nameof(OldFieldChange.Entity)).First();
			var dateTimeColumn = oldhcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeTime)).First();

			var query = $"INSERT INTO {hcPersister.TableName} "
				+ $"(SELECT DISTINCT {_fieldChangeAlias}.* "
				+ $"FROM {oldhcePersister.TableName} AS {_changedEntityAlias} "
				+ $"JOIN {oldhcPersister.TableName} AS {_fieldChangeAlias} ON {_changedEntityAlias}.{oldhcePersister.KeyColumnNames.First()} = {_fieldChangeAlias}.{changedEntityColumn} "
				+ $"WHERE {_changedEntityAlias}.{dateTimeColumn} BETWEEN '{dateTimeFrom:yyyy-MM-dd}' AND '{dateTimeTo:yyyy-MM-dd HH:mm:ss}');";
			uow.Session.CreateSQLQuery(query).SetTimeout(180).ExecuteUpdate();
		}

		public void DeleteHistoryChangesTest(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			var factory = uow.Session.SessionFactory;
			var oldHcePersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangedEntity));
			var oldHcsPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldChangeSet));
			var oldHcPersister = (AbstractEntityPersister)factory.GetClassMetadata(typeof(OldFieldChange));

			var changeSetColumn = oldHcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeSet)).First();
			var changeTimeColumn = oldHcePersister.GetPropertyColumnNames(nameof(OldChangedEntity.ChangeTime)).First();
			var changedEntityColumn = oldHcPersister.GetPropertyColumnNames(nameof(OldFieldChange.Entity)).First();

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
