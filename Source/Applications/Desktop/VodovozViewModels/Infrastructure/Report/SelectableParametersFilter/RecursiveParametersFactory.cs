using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using NHibernate.Criterion;
using System.Linq.Expressions;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class RecursiveParametersFactory<TEntity> : IParametersFactory
		where TEntity : class, IDomainObject
	{
		private readonly IUnitOfWork uow;
		private readonly Func<TEntity, string> titleFunc;
		private readonly Func<TEntity, IList<TEntity>> childsSelector;
		private readonly bool _useFullEntity;
		private readonly Func<IEnumerable<Func<ICriterion>>, IList<TEntity>> sourceFunc;
		private readonly Expression<Func<TEntity, bool>>[] additionalFilters;

		public RecursiveParametersFactory(IUnitOfWork uow, Func<IEnumerable<Func<ICriterion>>, IList<TEntity>> sourceFunc,
			Func<TEntity, string> titleFunc, Func<TEntity, IList<TEntity>> childsSelector, bool useFullEntity = false)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.sourceFunc = sourceFunc ?? throw new ArgumentNullException(nameof(sourceFunc));
			this.titleFunc = titleFunc ?? throw new ArgumentNullException(nameof(titleFunc));
			this.childsSelector = childsSelector ?? throw new ArgumentNullException(nameof(childsSelector));
			_useFullEntity = useFullEntity;
		}

		public bool IsRecursiveFactory => true;

		public IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations)
		{
			var entities = sourceFunc(filterRelations);
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
			SelectableEntityParameter<TEntity> parameter = _useFullEntity
				? new SelectableEntityParameter<TEntity>(entity, titleFunc(entity)) 
				: new SelectableEntityParameter<TEntity>(entity.Id, titleFunc(entity));

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
