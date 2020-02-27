using System;
using SolrSearch.Mapping;
using SolrNet;
using SolrNet.Commands.Parameters;
using System.Collections;
using System.Collections.Generic;
using SolrNet.Impl;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace SolrSearch
{
	public class SolrOrmSearchProvider
	{
		private readonly SolrOrmSourceMapping solrOrmSourceMapping;
		private readonly SolrEntityCreator solrEntityCreator;
		private string specialCharacters = "+-&|!(){}[]^\"~*?:/";

		public SolrOrmSearchProvider(SolrOrmSourceMapping solrOrmSourceMapping)
		{
			this.solrOrmSourceMapping = solrOrmSourceMapping ?? throw new ArgumentNullException(nameof(solrOrmSourceMapping));
			solrEntityCreator = new SolrEntityCreator(solrOrmSourceMapping);
		}

		public IEnumerable<TSolrEntity> CustomQuery<TSolrEntity>(ISolrQuery solrQuery, QueryOptions queryOptions)
			where TSolrEntity : SolrEntityBase
		{
			if(solrQuery == null) {
				throw new ArgumentNullException(nameof(solrQuery));
			}

			if(queryOptions == null) {
				throw new ArgumentNullException(nameof(queryOptions));
			}

			ISolrOperations<TSolrEntity> operations = Startup.Container.GetInstance<ISolrOperations<TSolrEntity>>();
			var results = operations.Query(solrQuery, queryOptions);
			return results;
		}

		//public IEnumerable<SolrSearchResult> Query(IDictionary<Type, IEnumerable<string>> entityProperties, string[] values, int limit = 30)
		//{
		//	if(values == null || !values.Any()) {
		//		return new SolrSearchResult[] { };
		//	}

		//	if(entityProperties == null) {
		//		throw new ArgumentNullException(nameof(entityProperties));
		//	}

		//	if(!entityProperties.Values.SelectMany(x => x).Any()) {
		//		return new SolrSearchResult[] { };
		//	}

		//	ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();

		//	HashSet<string> highlightFields = new HashSet<string>();

		//	List<ISolrQuery> queries = new List<ISolrQuery>();
		//	foreach(var entity in entityProperties) {
		//		Type entityType = entity.Key;
		//		List<ISolrQuery> entityQueries = new List<ISolrQuery>();

		//		foreach(var property in entity.Value) {
		//			string field = solrOrmSourceMapping.GetSolrField(entityType, property);
		//			Type fieldType = solrOrmSourceMapping.GetSolrFieldType(entityType, property);

		//			if(!highlightFields.Contains(field)) {
		//				highlightFields.Add(field);
		//			}

		//			List<SolrQuery> propertyQueries = new List<SolrQuery>();
		//			foreach(var searchValue in values) {
		//				if(fieldType != typeof(string) && StringToType(fieldType, searchValue) == null) {
		//					continue;
		//				}
		//				propertyQueries.Add(new SolrQuery($!highlightFields.Contains(field)) {\\n\\t\\t//\\t\\t\\t\\thighlightFields.Add(field);\\n\\t\\t//\\t\\t\\t}\\n\\n\\t\\t//\\t\\t\\tList<SolrQuery> propertyQueries = new List<SolrQuery>();\\n\\t\\t//\\t\\t\\tforeach(var searchValue in values) {\\n\\t\\t//\\t\\t\\t\\tif(fieldType != typeof(string) && StringToType(fieldType, searchValue) == null) {\\n\\t\\t//\\t\\t\\t\\t\\tcontinue;\\n\\t\\t//\\t\\t\\t\\t}\\n\\t\\t//\\t\\t\\t\\tpropertyQueries.Add(new SolrQuery($"{field}:*{searchValue}*"));
		//			}

		//			//TODO Не использовать WildCard для числовых значений, для облегчения нагрузки
		//			//var propertyQueries = values.Select(v => new SolrQuery($"{field}:{v}"));

		//			var propertyQuery = new SolrMultipleCriteriaQuery(propertyQueries, "OR");
		//			entityQueries.Add(propertyQuery);
		//		}

		//		var entityQuery = new SolrMultipleCriteriaQuery(entityQueries, "OR");

		//		queries.Add(new SolrMultipleCriteriaQuery(new ISolrQuery[] {
		//			new SolrQuery($"{solrOrmSourceMapping.EntityTypeFieldName}:{solrOrmSourceMapping.GetSolrEntityType(entityType)}"),
		//			entityQuery
		//		}, "AND"));
		//	}

		//	SolrMultipleCriteriaQuery query = new SolrMultipleCriteriaQuery(queries, "OR");
		//	Stopwatch stopwatch = new Stopwatch();
		//	stopwatch.Start();
		//	var queryResults = operations.Query(query, new QueryOptions {
		//		Rows = limit,
		//		Highlight = new HighlightingParameters {
		//			BeforeTerm = "<b>",
		//			AfterTerm = "</b>",
		//			Fields = highlightFields.ToArray()
		//		}
		//	});
		//	stopwatch.Stop();
		//	Console.WriteLine($"Запрос: {stopwatch.ElapsedMilliseconds} мс.");
		//	stopwatch.Reset();
		//	stopwatch.Start();
		//	var results = solrEntityCreator.CreateEntities(queryResults);
		//	stopwatch.Stop();
		//	Console.WriteLine($"Построение данных: {stopwatch.ElapsedMilliseconds} мс.");
		//	return results;
		//}

		public IEnumerable<SolrSearchResult> Query(IDictionary<Type, IEnumerable<string>> entityProperties, string[] values, int limit = 30)
		{
			if(values == null || !values.Any()) {
				return new SolrSearchResult[] { };
			}

			if(entityProperties == null) {
				throw new ArgumentNullException(nameof(entityProperties));
			}

			if(!entityProperties.Values.SelectMany(x => x).Any()) {
				return new SolrSearchResult[] { };
			}

			ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();

			HashSet<string> highlightFields = new HashSet<string>();

			List<ISolrQuery> queries = new List<ISolrQuery>();
			foreach(var entity in entityProperties) {
				Type entityType = entity.Key;
				List<ISolrQuery> entityQueries = new List<ISolrQuery>();

				foreach(var property in entity.Value) {
					string field = solrOrmSourceMapping.GetSolrField(entityType, property);
					Type fieldType = solrOrmSourceMapping.GetSolrFieldType(entityType, property);

					if(!highlightFields.Contains(field)) {
						highlightFields.Add(field);
					}

					List<ISolrQuery> propertyQueries = new List<ISolrQuery>();
					foreach(var searchValue in values) {
						if(fieldType != typeof(string) && StringToType(fieldType, searchValue) == null) {
							continue;
						}
						foreach(var sc in specialCharacters) {
							searchValue.Replace(sc.ToString(), $"\\{sc.ToString()}");
						}

						List<ISolrQuery> wordsQueries = new List<ISolrQuery>();
						foreach(var word in searchValue.Trim().Split(' ')) {
							wordsQueries.Add(new SolrQuery($"{field}:*{word}*"));
						}
						propertyQueries.Add(new SolrMultipleCriteriaQuery(wordsQueries, "AND"));
					}

					//TODO Не использовать WildCard для числовых значений, для облегчения нагрузки
					//var propertyQueries = values.Select(v => new SolrQuery($"{field}:{v}"));

					var propertyQuery = new SolrMultipleCriteriaQuery(propertyQueries, "OR");
					entityQueries.Add(propertyQuery);
				}

				var entityQuery = new SolrMultipleCriteriaQuery(entityQueries, "OR");

				queries.Add(new SolrMultipleCriteriaQuery(new ISolrQuery[] {
					new SolrQuery($"{solrOrmSourceMapping.EntityTypeFieldName}:{solrOrmSourceMapping.GetSolrEntityType(entityType)}"),
					entityQuery
				}, "AND"));
			}

			SolrMultipleCriteriaQuery query = new SolrMultipleCriteriaQuery(queries, "OR");
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var queryResults = operations.Query(query, new QueryOptions {
				Rows = limit,
				Highlight = new HighlightingParameters {
					BeforeTerm = "<b>",
					AfterTerm = "</b>",
					Fields = highlightFields.ToArray()
				}
			});
			stopwatch.Stop();
			Console.WriteLine($"Запрос: {stopwatch.ElapsedMilliseconds} мс.");
			stopwatch.Reset();
			stopwatch.Start();
			var results = solrEntityCreator.CreateEntities(queryResults);
			stopwatch.Stop();
			Console.WriteLine($"Построение данных: {stopwatch.ElapsedMilliseconds} мс.");
			return results;
		}

		public Type GetOrmEntityType(Type solrEntityType)
		{
			if(solrEntityType == null) {
				throw new ArgumentNullException(nameof(solrEntityType));
			}

			return solrOrmSourceMapping.GetOrmEntityType(solrEntityType);
		}

		private object StringToType(Type type, string value)
		{
			if(type == null) {
				throw new ArgumentNullException(nameof(type));
			}

			var underlyingType = Nullable.GetUnderlyingType(type);
			try {
				if(underlyingType == null)
					return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
				return String.IsNullOrEmpty(value)
				  ? null
				  : Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
			} catch(Exception) {
				return null;
			}
		}
	}
}
