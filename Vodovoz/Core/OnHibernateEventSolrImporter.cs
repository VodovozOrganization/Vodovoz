using System;
using QS.DomainModel.NotifyChange;
using System.Collections.Generic;
using SolrImportService;
using NLog;
using SolrSearch.Mapping;
using System.Linq;

namespace Vodovoz.Core
{
	public class OnHibernateEventSolrImporter : IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IEntityChangeWatcher entityChangeWatcher;
		private readonly SolrImportServiceChannelFactory serviceChannelFactory;
		private readonly SolrOrmSourceMapping solrMapping;

		public OnHibernateEventSolrImporter(IEntityChangeWatcher entityChangeWatcher, SolrImportServiceChannelFactory serviceChannelFactory, SolrOrmSourceMapping solrMapping)
		{
			this.entityChangeWatcher = entityChangeWatcher ?? throw new ArgumentNullException(nameof(entityChangeWatcher));
			this.serviceChannelFactory = serviceChannelFactory ?? throw new ArgumentNullException(nameof(serviceChannelFactory));
			this.solrMapping = solrMapping ?? throw new ArgumentNullException(nameof(solrMapping));


		}

		public void Start()
		{
			IEnumerable<Type> types = solrMapping.GetRegisteredOrmTypes();
			entityChangeWatcher.BatchSubscribeOnEntity(HandleBatchEntityChangeHandler, types.ToArray());
		}

		public void Stop()
		{
			entityChangeWatcher.UnsubscribeAll(this);
		}

		private ISolrService solrService;

		private void CreateService()
		{
			try {
				solrService = serviceChannelFactory.GetSolrService();
			} catch(Exception ex) {
				solrService = null;
				logger.Error(ex);
			}
		}

		HashSet<Type> changedEntityTypes = new HashSet<Type>();

		private void HandleBatchEntityChangeHandler(EntityChangeEvent[] changeEvents)
		{
			foreach(var changeEvent in changeEvents) {
				if(changedEntityTypes.Contains(changeEvent.EntityClass)) {
					continue;
				}
				changedEntityTypes.Add(changeEvent.EntityClass);
			}
			RunImport();
		}

		private string GetSolrEntityName(Type entityType)
		{
			return solrMapping.GetSolrEntityType(entityType);
		}

		private void RunImport()
		{
			if(solrService == null) {
				CreateService();
			}

			if(solrService == null) {
				return;
			}

			foreach(var entityType in changedEntityTypes.ToList()) {
				string solrEntityName;

				try {
					solrEntityName = GetSolrEntityName(entityType);
				} catch(Exception ex) {
					logger.Error(ex);
					continue;
				}

				if(string.IsNullOrWhiteSpace(solrEntityName)) {
					continue;
				}

				try {
					solrService.RunDeltaImport(solrEntityName);
					logger.Info($"Запущен импорт для сущности {solrEntityName}");
					changedEntityTypes.Remove(entityType);
				} catch(Exception ex) {
					logger.Error(ex);
				}
			}
		}

		public void Dispose()
		{
			Stop();
		}
	}
}
