using System;
using NHibernate.Criterion;
using QS.Project.Journal.Search.Criterion;
using System.Linq;
using SolrSearch;
using System.Collections.Generic;
using Gamma.Utilities;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using System.Collections.ObjectModel;
using System.Net;
using NLog;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace Vodovoz.SearchModel
{
	public class SolrCriterionSearchModel : CriterionSearchModel
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly SolrOrmSearchProvider solrOrmSearchProvider;

		private Dictionary<Type, List<string>> solrSearchProperties = new Dictionary<Type, List<string>>();

		private bool? solrUnavailable;
		/// <summary>
		/// Сервер Solr недоступен
		/// </summary>
		public virtual bool SolrUnavailable {
			get {
				if(!solrUnavailable.HasValue) {
					UpdateSolrServiceAvailability();
				}
				return solrUnavailable ?? false;
			}

			private set => SetField(ref solrUnavailable, value);
		}

		public void UpdateSolrServiceAvailability()
		{
			try {
				var result = solrOrmSearchProvider.CustomQuery(new SolrQuery("*:*"), new QueryOptions { Rows = 1 });
				solrUnavailable = !result.Any();
			} catch(SolrNet.Exceptions.SolrConnectionException ex) {
				logger.Error(ex);
				solrUnavailable = true;
			}
		}


		private bool solrDisable;
		/// <summary>
		/// Отключает/включает поиск с использованием Solr
		/// </summary>
		public virtual bool SolrDisable {
			get => solrDisable;
			set => SetField(ref solrDisable, value, () => SolrDisable);
		}

		public SolrCriterionSearchModel(SolrOrmSearchProvider solrOrmSearchProvider)
		{
			this.solrOrmSearchProvider = solrOrmSearchProvider ?? throw new ArgumentNullException(nameof(solrOrmSearchProvider));
		}

		public override void Update()
		{
			base.Update();
		}

		public ObservableCollection<Type> SearchEntityTypes = new ObservableCollection<Type>();

		/*
		public IEnumerable<Type> GetSearchTypes()
		{
			return solrSearchProperties.Select(x => x.Key);
		}
		*/

		public void AddSolrSearchBy<TEntity>(Expression<Func<TEntity, object>> propertySelector)
			where TEntity : class, IDomainObject
		{
			Type entityType = typeof(TEntity);

			if(!solrSearchProperties.ContainsKey(entityType)) {
				solrSearchProperties.Add(entityType, new List<string>());
			}
			string propertyName = PropertyUtil.GetName(propertySelector);
			if(solrSearchProperties[entityType].Contains(propertyName)) {
				throw new InvalidOperationException("Такое свойство уже было добавлено в поиск");
			}
			solrSearchProperties[entityType].Add(propertyName);
			SearchEntityTypes.Add(entityType);
		}

		private IEnumerable<SolrSearchResult> selectedResults;
		public IEnumerable<SolrSearchResult> SelectedResults {
			get => selectedResults;
			set {
				if(SetField(ref selectedResults, value, () => SelectedResults)) {
					Update();
				}
			}
		}

		public SolrSearchResults RunSolrSearch(IEnumerable<Type> forTypes = null)
		{
			if(SearchValues.All(x => string.IsNullOrWhiteSpace(x))) {
				return new SolrSearchResults();
			}

			Dictionary<Type, IEnumerable<string>> properties;
			if(forTypes == null) {
				properties = solrSearchProperties.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
			} else {
				properties = solrSearchProperties.Where(x => forTypes.Contains(x.Key)).ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
			}

			SolrSearchResults results = null;
			try {
				results = solrOrmSearchProvider.Query(properties, SearchValues);
				SolrUnavailable = false;
			} catch(SolrNet.Exceptions.SolrConnectionException ex) {
				logger.Error(ex);
				UpdateSolrServiceAvailability();
			}
			return results;
		}

		private IEnumerable<SearchAliasParameter> GetIdParameters()
		{
			IDomainObject domainObjectAlias = null;
			string identifierName = nameof(domainObjectAlias.Id);

			var result = AliasParameters.Where(x => x.Expression is LambdaExpression)
				.Where(x => (x.Expression as LambdaExpression).Body is UnaryExpression)
				.Where(x => ((x.Expression as LambdaExpression).Body as UnaryExpression).Operand is MemberExpression)
				.Where(x => (((x.Expression as LambdaExpression).Body as UnaryExpression).Operand as MemberExpression).Member.Name == identifierName);

			return result;
			/*
			foreach(var item in AliasParameters.Select(x => x.Expression)) {
				var expr = (item as LambdaExpression).Body as UnaryExpression;
				var propertyExpression = expr.Operand as MemberExpression;
				var aliasExpression = propertyExpression.Expression as MemberExpression;

				string idName = propertyExpression.Member.Name;
				Type aliasType = aliasExpression.Type;
				//nameof(domainObjectAlias.Id)
			}*/
		}

		protected override ICriterion GetSearchCriterion()
		{
			if(SolrDisable || SolrUnavailable) {
				return base.GetSearchCriterion();
			}

			IEnumerable<SearchAliasParameter> idParameters = GetIdParameters();

			Disjunction resultRestriction = new Disjunction();
			if(SelectedResults == null) {
				return resultRestriction;
			}
			foreach(var sr in SelectedResults) {
				if(!(sr.Entity is IDomainObject)) {
					continue;
				}
				Type ormType = solrOrmSearchProvider.GetOrmEntityType(sr.Entity.GetType());

				IEnumerable<SearchAliasParameter> currentParameters = idParameters.Where(x => x.AliasType == ormType);
				foreach(var parameter in currentParameters) {
					ICriterion entityRestriction = Restrictions.Eq(parameter.PropertyProjection, (sr.Entity as IDomainObject).Id);

					resultRestriction.Add(entityRestriction);
				}
			}

			return resultRestriction;
		}
	}
}
