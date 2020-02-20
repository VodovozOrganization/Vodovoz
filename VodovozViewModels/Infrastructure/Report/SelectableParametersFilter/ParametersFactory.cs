using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Transform;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class ParametersEnumFactory<TEnum> : IParametersEnumFactory<TEnum>
	{
		public IList<SelectableParameter> GetParameters()
		{
			var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var enumValue in values) {
				SelectableParameter parameter = new SelectableEnumParameter<TEnum>(enumValue);
				result.Add(parameter);
			}
			return result;
		}
	}

	public class ParametersFactory<TEntity> : IParametersEntityFactory<TEntity>
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork uow;
		private readonly Expression<Func<TEntity, object>> titleExpr;
		private readonly Expression<Func<TEntity, bool>>[] additionalFilters;

		public ParametersFactory(IUnitOfWork uow, Expression<Func<TEntity, object>> titleExpr, params Expression<Func<TEntity, bool>>[] additionalFilters)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.titleExpr = titleExpr ?? throw new ArgumentNullException(nameof(titleExpr));
			this.additionalFilters = additionalFilters;
		}

		public IList<SelectableParameter> GetParameters()
		{
			var query = uow.Session.QueryOver<TEntity>();
			if(additionalFilters != null && additionalFilters.Any()) {
				foreach(var additionalFilter in additionalFilters) {
					query.Where(additionalFilter);
				}
			}
			SelectableEntityParameter<TEntity> resultAlias = null;
			query.SelectList(list => list
				.Select(Projections.Id()).WithAlias(() => resultAlias.EntityId)
				.Select(titleExpr).WithAlias(() => resultAlias.EntityTitle)
			)
			.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<TEntity>>());
			return query.List<SelectableParameter>();
		}

		public IList<SelectableParameter> GetParameters(System.Linq.Expressions.Expression<Func<TEntity, object>> filterExpression, SelectableParameterSet filterSource)
		{
			if(filterSource == null) {
				throw new ArgumentNullException(nameof(filterSource));
			}
			var selectedIds = filterSource.GetSelectedValues();

			var query = uow.Session.QueryOver<TEntity>();
			if(additionalFilters != null && additionalFilters.Any()) {
				foreach(var additionalFilter in additionalFilters) {
					query.Where(additionalFilter);
				}
			}
			if(selectedIds.Any()) {
				query.Where(Restrictions.In(Projections.Property(filterExpression), selectedIds));
			}
			SelectableEntityParameter<TEntity> resultAlias = null;
			query.SelectList(list => list
				.Select(Projections.Id()).WithAlias(() => resultAlias.EntityId)
				.Select(titleExpr).WithAlias(() => resultAlias.Title)
			)
			.TransformUsing(Transformers.AliasToBean<SelectableEntityParameter<TEntity>>());
			return query.List<SelectableParameter>();			
		}
	}
}
