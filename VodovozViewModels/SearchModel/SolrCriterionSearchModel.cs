using System;
using NHibernate.Criterion;
using QS.Project.Journal.Search.Criterion;
using System.Linq;
using SolrSearch;
using System.Collections.Generic;
using Gamma.Utilities;
using System.Linq.Expressions;
using QS.DomainModel.Entity;

namespace Vodovoz.SearchModel
{
	public class SolrCriterionSearchModel : CriterionSearchModelBase
	{
		private readonly SolrOrmSearchProvider solrOrmSearchProvider;

		private Dictionary<Type, List<string>> solrSearchProperties = new Dictionary<Type, List<string>>();

		public SolrCriterionSearchModel(SolrOrmSearchProvider solrOrmSearchProvider)
		{
			this.solrOrmSearchProvider = solrOrmSearchProvider ?? throw new ArgumentNullException(nameof(solrOrmSearchProvider));
		}

		public override void Update()
		{
			base.Update();
		}

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

		public IEnumerable<SolrSearchResult> RunSolrSearch()
		{
			Dictionary<Type, IEnumerable<string>> properties = solrSearchProperties.ToDictionary(k => k.Key, v => v.Value.AsEnumerable());
			return solrOrmSearchProvider.Query(properties, SearchValues);
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

			foreach(var item in AliasParameters.Select(x => x.Expression)) {
				var expr = (item as LambdaExpression).Body as UnaryExpression;
				var propertyExpression = expr.Operand as MemberExpression;
				var aliasExpression = propertyExpression.Expression as MemberExpression;

				string idName = propertyExpression.Member.Name;
				Type aliasType = aliasExpression.Type;
				//nameof(domainObjectAlias.Id)
			}
		}

		protected override ICriterion GetSearchCriterion()
		{
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
