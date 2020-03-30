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
		private string specialCharacters = "+-&|!(){}[]^\"~:/*?";

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

		public IEnumerable<Dictionary<string, object>> CustomQuery(ISolrQuery solrQuery, QueryOptions queryOptions)
		{
			if(solrQuery == null) {
				throw new ArgumentNullException(nameof(solrQuery));
			}

			if(queryOptions == null) {
				throw new ArgumentNullException(nameof(queryOptions));
			}

			ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();
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

		private IEnumerable<string> CleanSearchValues(string[] values)
		{
			List<string> result = new List<string>();
			foreach(var value in values) {
				string resultValue = value;
				foreach(var sc in specialCharacters) {
					//searchValue.Replace(sc.ToString(), $"\\{sc.ToString()}");
					resultValue = resultValue.Replace(sc.ToString(), " ");
				}
				if(!string.IsNullOrWhiteSpace(resultValue)) {
					result.Add(resultValue);
				}
			}
			return result;
		}

		private class EntityQueryInfo
		{
			public ISolrQuery EntityTypeQuery { get; set; }
			List<ISolrQuery> valueQueries = new List<ISolrQuery>();
			public IEnumerable<ISolrQuery> ValueQueries => valueQueries;

			public EntityQueryInfo(ISolrQuery entityTypeQuery)
			{
				EntityTypeQuery = entityTypeQuery;
			}

			public void AddValueQuery(ISolrQuery valueQuery)
			{
				valueQueries.Add(valueQuery);
			}
		}

		public SolrSearchResults Query(IDictionary<Type, IEnumerable<string>> entityProperties, string[] values, int limit = 50)
		{
			//TODO Вынести логику поиска как стратегию, для переопределния извне

			if(values == null || !values.Any() || values.All(x => string.IsNullOrWhiteSpace(x))) {
				return new SolrSearchResults();
			}

			if(entityProperties == null) {
				throw new ArgumentNullException(nameof(entityProperties));
			}

			if(!entityProperties.Values.SelectMany(x => x).Any()) {
				return new SolrSearchResults();
			}

			var cleanSearchValues = CleanSearchValues(values);
			var cleanSearchValuesCount = cleanSearchValues.Count();

			ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();

			HashSet<string> highlightFields = new HashSet<string>();

			List<ISolrQuery> queries = new List<ISolrQuery>();
			List<ISolrQuery> entityTypeQueries = new List<ISolrQuery>();
			List<ISolrQuery> searchValueQueries = new List<ISolrQuery>();
			ISolrQuery idQuery = null;
			if(cleanSearchValuesCount == 1) {
				idQuery = new SolrQuery($"Id:{cleanSearchValues.First()}");
			}


			List<EntityQueryInfo> entityQueries = new List<EntityQueryInfo>();
			foreach(var entity in entityProperties) {
				Type entityType = entity.Key;

				var entityTypeQuery = new SolrQuery($"{solrOrmSourceMapping.EntityTypeFieldName}:{solrOrmSourceMapping.GetSolrEntityType(entityType)}");
				EntityQueryInfo entityQueryInfo = new EntityQueryInfo(entityTypeQuery);

				foreach(var searchValue in cleanSearchValues) {
					List<ISolrQuery> searchValueQuery = new List<ISolrQuery>();
					foreach(var property in entity.Value) {
						if(property == "Id") {
							continue;
						}
						SolrFieldModel fieldInfo = solrOrmSourceMapping.GetSolrField(entityType, property);
						Type fieldType = solrOrmSourceMapping.GetSolrFieldType(entityType, property);

						if(!highlightFields.Contains(fieldInfo.FieldName)) {
							highlightFields.Add(fieldInfo.FieldName);
						}

						string queryValue;
						if(fieldType == typeof(string)) {
							queryValue = $"*{searchValue}*";
						} else {
							queryValue = $"{searchValue}";
						}
						AbstractSolrQuery fieldQuery = new SolrQuery($"{fieldInfo.FieldName}:{queryValue}");
						if(fieldInfo.Boost.HasValue) {
							fieldQuery = fieldQuery.Boost(fieldInfo.Boost.Value);
						}
						searchValueQuery.Add(fieldQuery);
					}
					entityQueryInfo.AddValueQuery(new SolrMultipleCriteriaQuery(searchValueQuery, "OR"));
					entityQueries.Add(entityQueryInfo);
				}
			}

			List<ISolrQuery> direct = new List<ISolrQuery>();
			List<ISolrQuery> inverse = new List<ISolrQuery>();

			foreach(var entityQueryInfo in entityQueries) {
				var valuesQueryDirect = new SolrMultipleCriteriaQuery(entityQueryInfo.ValueQueries, "AND");
				direct.Add(new SolrMultipleCriteriaQuery(new ISolrQuery[] { valuesQueryDirect, entityQueryInfo.EntityTypeQuery }, "AND"));

				var valuesQueryInverse = new SolrMultipleCriteriaQuery(entityQueryInfo.ValueQueries, "OR");
				inverse.Add(new SolrMultipleCriteriaQuery(new ISolrQuery[] { valuesQueryInverse, entityQueryInfo.EntityTypeQuery }, "AND"));
			}

			ISolrQuery directQuery = new SolrMultipleCriteriaQuery(direct, "OR");
			ISolrQuery inverseQuery = new SolrMultipleCriteriaQuery(inverse, "AND");
			ISolrQuery resultQuery = new SolrMultipleCriteriaQuery(new ISolrQuery[] { directQuery, inverseQuery}, "OR");

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			var queryResults = operations.Query(resultQuery, new QueryOptions {
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
			return new SolrSearchResults(queryResults.NumFound, queryResults.Count, results);
		}

		//public SolrSearchResults Query(IDictionary<Type, IEnumerable<string>> entityProperties, string[] values, int limit = 50)
		//{
		//	//TODO Вынести логику поиска как стратегию, для переопределния извне

		//	if(values == null || !values.Any() || values.All(x => string.IsNullOrWhiteSpace(x))) {
		//		return new SolrSearchResults();
		//	}

		//	if(entityProperties == null) {
		//		throw new ArgumentNullException(nameof(entityProperties));
		//	}

		//	if(!entityProperties.Values.SelectMany(x => x).Any()) {
		//		return new SolrSearchResults();
		//	}

		//	var cleanSearchValues = CleanSearchValues(values);
		//	var cleanSearchValuesCount = cleanSearchValues.Count();

		//	ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();

		//	HashSet<string> highlightFields = new HashSet<string>();

		//	List<ISolrQuery> queries = new List<ISolrQuery>();
		//	List<ISolrQuery> entityTypeQueries = new List<ISolrQuery>();
		//	List<ISolrQuery> searchValueQueries = new List<ISolrQuery>();
		//	ISolrQuery idQuery = null;

		//	if(cleanSearchValuesCount == 1) {
		//		idQuery = new SolrQuery($"Id:{cleanSearchValues.First()}");
		//	}

		//	HashSet<Type> addedEntities = new HashSet<Type>();

		//	foreach(var searchValue in cleanSearchValues) {

		//		List<ISolrQuery> fieldQueries = new List<ISolrQuery>();
		//		HashSet<string> addedFields = new HashSet<string>();

		//		foreach(var entity in entityProperties) {
		//			Type entityType = entity.Key;

		//			if(!addedEntities.Contains(entityType)) {
		//				entityTypeQueries.Add(new SolrQuery($"{solrOrmSourceMapping.EntityTypeFieldName}:{solrOrmSourceMapping.GetSolrEntityType(entityType)}"));
		//				addedEntities.Add(entityType);
		//			}

		//			foreach(var property in entity.Value) {
		//				if(property == "Id") {
		//					continue;
		//				}

		//				SolrFieldModel fieldInfo = solrOrmSourceMapping.GetSolrField(entityType, property);
		//				if(addedFields.Contains(fieldInfo.FieldName)) {
		//					continue;
		//				}

		//				Type fieldType = solrOrmSourceMapping.GetSolrFieldType(entityType, property);

		//				if(!highlightFields.Contains(fieldInfo.FieldName)) {
		//					highlightFields.Add(fieldInfo.FieldName);
		//				}

		//				string queryValue;
		//				if(fieldType == typeof(string)) {
		//					queryValue = $"*{searchValue}*";
		//				} else {
		//					queryValue = $"{searchValue}";
		//				}
		//				AbstractSolrQuery fieldQuery = new SolrQuery($"{fieldInfo.FieldName}:{queryValue}");
		//				if(fieldInfo.Boost.HasValue) {
		//					fieldQuery = fieldQuery.Boost(fieldInfo.Boost.Value);
		//				}

		//				fieldQueries.Add(fieldQuery);
		//				addedFields.Add(fieldInfo.FieldName);

		//				/*
		//				List<ISolrQuery> propertyQueries = new List<ISolrQuery>();
		//				foreach(var searchValue in cleanSearchValues) {
		//					string finalSearchValue = searchValue;
		//					if(fieldType != typeof(string) && StringToType(fieldType, finalSearchValue) == null) {
		//						continue;
		//					}

		//					string queryValue;
		//					if(fieldType == typeof(string)) {
		//						queryValue = $"*{finalSearchValue}*";
		//					} else {
		//						queryValue = $"{finalSearchValue}";
		//					}
		//					AbstractSolrQuery fieldQuery = new SolrQuery($"{fieldInfo.FieldName}:{queryValue}");
		//					if(fieldInfo.Boost.HasValue) {
		//						fieldQuery = fieldQuery.Boost(fieldInfo.Boost.Value);
		//					}
		//					propertyQueries.Add(fieldQuery);
		//				}*/

		//				//var propertyQuery = new SolrMultipleCriteriaQuery(propertyQueries, "AND");

		//				//entityQueries.Add(propertyQuery);
		//			}
		//		}

		//		searchValueQueries.Add(new SolrMultipleCriteriaQuery(fieldQueries, "OR"));
		//	}

		//	ISolrQuery entityQuery = new SolrMultipleCriteriaQuery(entityTypeQueries, "OR");
		//	foreach(var svq in searchValueQueries) {
		//		queries.Add(new SolrMultipleCriteriaQuery(new ISolrQuery[] { entityQuery, svq } , "AND"));
		//	}

		//	SolrMultipleCriteriaQuery query = new SolrMultipleCriteriaQuery(queries, "AND");
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
		//	return new SolrSearchResults(queryResults.NumFound, queryResults.Count, results);
		//}

		//public SolrSearchResults Query(IDictionary<Type, IEnumerable<string>> entityProperties, string[] values, int limit = 50)
		//{
		//	//TODO Вынести логику поиска как стратегию, для переопределния извне

		//	if(values == null || !values.Any() || values.All(x => string.IsNullOrWhiteSpace(x))) {
		//		return new SolrSearchResults();
		//	}

		//	if(entityProperties == null) {
		//		throw new ArgumentNullException(nameof(entityProperties));
		//	}

		//	if(!entityProperties.Values.SelectMany(x => x).Any()) {
		//		return new SolrSearchResults();
		//	}

		//	var cleanSearchValues = CleanSearchValues(values);

		//	ISolrOperations<Dictionary<string, object>> operations = Startup.Container.GetInstance<ISolrOperations<Dictionary<string, object>>>();

		//	HashSet<string> highlightFields = new HashSet<string>();

		//	List<ISolrQuery> queries = new List<ISolrQuery>();
		//	foreach(var entity in entityProperties) {
		//		Type entityType = entity.Key;
		//		List<ISolrQuery> entityQueries = new List<ISolrQuery>();

		//		foreach(var property in entity.Value) {
		//			SolrFieldModel fieldInfo = solrOrmSourceMapping.GetSolrField(entityType, property);
		//			Type fieldType = solrOrmSourceMapping.GetSolrFieldType(entityType, property);

		//			if(property == "Id" && cleanSearchValues.Count() > 1) {
		//				continue;
		//			}

		//			if(!highlightFields.Contains(fieldInfo.FieldName)) {
		//				highlightFields.Add(fieldInfo.FieldName);
		//			}

		//			List<ISolrQuery> propertyQueries = new List<ISolrQuery>();
		//			foreach(var searchValue in cleanSearchValues) {
		//				string finalSearchValue = searchValue;
		//				if(fieldType != typeof(string) && StringToType(fieldType, finalSearchValue) == null) {
		//					continue;
		//				}

		//				//foreach(var sc in specialCharacters) {
		//				//	//searchValue.Replace(sc.ToString(), $"\\{sc.ToString()}");
		//				//	finalSearchValue = finalSearchValue.Replace(sc.ToString(), "");
		//				//}
		//				//if(string.IsNullOrWhiteSpace(finalSearchValue)) {
		//				//	continue;
		//				//}


		//				/*
		//				List<ISolrQuery> wordsQueries = new List<ISolrQuery>();
		//				foreach(var word in searchValue.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
		//					wordsQueries.Add(new SolrQuery($"{field}:{word}"));
		//				}
		//				propertyQueries.Add(new SolrMultipleCriteriaQuery(wordsQueries, "AND"));*/
		//				string queryValue;
		//				if(fieldType == typeof(string)) {
		//					queryValue = $"*{finalSearchValue}*";
		//				} else {
		//					queryValue = $"{finalSearchValue}";
		//				}
		//				AbstractSolrQuery fieldQuery = new SolrQuery($"{fieldInfo.FieldName}:{queryValue}");
		//				if(fieldInfo.Boost.HasValue) {
		//					fieldQuery = fieldQuery.Boost(fieldInfo.Boost.Value);
		//				}
		//				propertyQueries.Add(fieldQuery);
		//			}

		//			//TODO Не использовать WildCard для числовых значений, для облегчения нагрузки
		//			//var propertyQueries = values.Select(v => new SolrQuery($"{field}:{v}"));

		//			var propertyQuery = new SolrMultipleCriteriaQuery(propertyQueries, "AND");


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
		//	return new SolrSearchResults(queryResults.NumFound, queryResults.Count, results);
		//}

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
