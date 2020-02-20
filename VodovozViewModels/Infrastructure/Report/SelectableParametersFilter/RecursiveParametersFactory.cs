using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using NHibernate.Criterion;
using System.Linq.Expressions;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class RecursiveParametersFactory<TEntity> : IParametersEntityFactory<TEntity>
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork uow;
		private readonly Func<TEntity, string> titleFunc;
		private readonly Func<TEntity, IList<TEntity>> childsSelector;
		private readonly Expression<Func<TEntity, bool>>[] additionalFilters;

		public RecursiveParametersFactory(IUnitOfWork uow, Func<TEntity, string> titleFunc, Func<TEntity, IList<TEntity>> childsSelector, params Expression<Func<TEntity, bool>>[] additionalFilters)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.titleFunc = titleFunc ?? throw new ArgumentNullException(nameof(titleFunc));
			this.childsSelector = childsSelector ?? throw new ArgumentNullException(nameof(childsSelector));
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
			var entities = query.List();
			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var entity in entities) {
				if(entity == null) {
					continue;
				}
				SelectableParameter parameter = CreateParameter(entity);
				result.Add(parameter);
			}
			return result;

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
			var entities = query
				.Where(Restrictions.In(Projections.Property(filterExpression), selectedIds))
				.List();

			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var entity in entities) {
				if(entity == null) {
					continue;
				}
				SelectableParameter parameter = CreateParameter(entity);
				result.Add(parameter);
			}
			return result;
		}

		private SelectableEntityParameter<TEntity> CreateParameter(TEntity entity)
		{
			SelectableEntityParameter<TEntity> parameter = new SelectableEntityParameter<TEntity>(entity.Id, titleFunc(entity));
			var childs = childsSelector(entity);
			if(childs == null || !childs.Any()) {
				return parameter;
			}

			parameter.SetChilds(GetParameters(childs));
			foreach(SelectableParameter child in parameter.Children) {
				child.Parent = parameter;
			}

			return parameter;
		}

		private IList<SelectableParameter> GetParameters(IEnumerable<TEntity> entities)
		{
			if(entities == null) {
				throw new ArgumentNullException(nameof(entities));
			}

			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var entity in entities) {
				if(entity == null) {
					continue;
				}
				SelectableParameter parameter = CreateParameter(entity);
				result.Add(parameter);
			}
			return result;
		}
	}
}
