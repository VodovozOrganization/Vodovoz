using System.Collections.Generic;
using SolrSearch.Mapping;
using System;
using SolrNet;
using SolrNet.Impl;
using System.Linq;

namespace SolrSearch
{
	public class SolrEntityCreator
	{
		private readonly SolrOrmSourceMapping solrOrmSourceMapping;
		private Dictionary<Type, SolrEntityFactoryBase> solrFactories = new Dictionary<Type, SolrEntityFactoryBase>();

		public SolrEntityCreator(SolrOrmSourceMapping solrOrmSourceMapping)
		{
			this.solrOrmSourceMapping = solrOrmSourceMapping ?? throw new System.ArgumentNullException(nameof(solrOrmSourceMapping));
		}
		/*
		public IEnumerable<SolrEntityBase> CreateEntities(IEnumerable<Dictionary<string, object>> entityContents)
		{
			List<SolrEntityBase> result = new List<SolrEntityBase>();
			foreach(var entityContent in entityContents) {
				Type solrEntityType = solrOrmSourceMapping.GetSolrEntityType((string)entityContent[solrOrmSourceMapping.EntityTypeFieldName]);
				if(!solrFactories.TryGetValue(solrEntityType, out SolrEntityFactoryBase solrEntityFactory)) {
					solrEntityFactory = solrOrmSourceMapping.GetSolrEntityFactory(solrEntityType);
					solrFactories.Add(solrEntityType, solrEntityFactory);
				}

				SolrEntityBase solrEntityBase = solrEntityFactory.CreateEntityBase(new EntityContentProvider(solrEntityType, entityContent, solrOrmSourceMapping));
				result.Add(solrEntityBase);
			}
			return result;
		}*/

		public IEnumerable<SolrSearchResult> CreateEntities(SolrQueryResults<Dictionary<string, object>> queryResult)
		{
			List<SolrSearchResult> result = new List<SolrSearchResult>();
			foreach(var entityContent in queryResult) {
				Type solrEntityType = solrOrmSourceMapping.GetSolrEntityType((string)entityContent[solrOrmSourceMapping.EntityTypeFieldName]);
				if(!solrFactories.TryGetValue(solrEntityType, out SolrEntityFactoryBase solrEntityFactory)) {
					solrEntityFactory = solrOrmSourceMapping.GetSolrEntityFactory(solrEntityType);
					solrFactories.Add(solrEntityType, solrEntityFactory);
				}
				var solrEntityId = (string)entityContent[solrOrmSourceMapping.KeyFieldName];
				var solrEntityTypeName = (string)entityContent[solrOrmSourceMapping.EntityTypeFieldName];

				IDictionary<string, string> hightlightedContentDictionary = new Dictionary<string, string>();
				if(queryResult.Highlights != null && queryResult.Highlights.TryGetValue(solrEntityId, out HighlightedSnippets hightlightContent)) {
					//hightlightedContentDictionary.Add(solrOrmSourceMapping.KeyFieldName, solrEntityId);
					//hightlightedContentDictionary.Add(solrOrmSourceMapping.EntityTypeFieldName, solrEntityTypeName);
					//IDictionary<string, object>  entityContent.ToDictionary(k => k.Key, v => v.Value);
					foreach(var hc in hightlightContent) {
						string content = hc.Value.FirstOrDefault();
						if(!string.IsNullOrWhiteSpace(content)) {
							hightlightedContentDictionary.Add(solrOrmSourceMapping.GetSolrPropertyByFieldName(solrEntityType, hc.Key), content);
						}
					}
					//hightlightedContentDictionary = hightlightContent.ToDictionary(k => k.Key, v => (object)v.Value.FirstOrDefault());
					//hightlightedEntity = solrEntityFactory.CreateEntityBase(new EntityContentProvider(solrEntityType, hightlightedContentDictionary, solrOrmSourceMapping));
				}

				SolrEntityBase entity = solrEntityFactory.CreateEntityBase(new EntityContentProvider(solrEntityType, entityContent, solrOrmSourceMapping));
				result.Add(new SolrSearchResult(entity, hightlightedContentDictionary));
			}
			return result;
		}
	}
}
