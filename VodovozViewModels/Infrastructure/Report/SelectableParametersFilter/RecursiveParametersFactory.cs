using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class RecursiveParametersFactory<TEntity> : IParametersFactory<TEntity>
		where TEntity : class, IDomainObject
	{
		private readonly Func<TEntity, IList<TEntity>> childsSelector;

		public RecursiveParametersFactory(TEntity entity, Func<TEntity, IList<TEntity>> childsSelector)
		{
			this.childsSelector = childsSelector ?? throw new ArgumentNullException(nameof(childsSelector));
		}

		public IList<SelectableParameter> GetParameters(IEnumerable<TEntity> entities)
		{
			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var entity in entities) {
				SelectableParameter parameter = CreateParameter(entity);
				result.Add(parameter);
			}
			return result;
		}

		private SelectableParameter<TEntity> CreateParameter(TEntity entity)
		{
			if(entity == null) {
				throw new ArgumentNullException(nameof(entity));
			}

			SelectableParameter<TEntity> parameter = new SelectableParameter<TEntity>(entity);
			var childs = childsSelector(entity);
			if(childs == null || !childs.Any()) {
				return parameter;
			}

			parameter.Childs = GetParameters(childs);
			foreach(var child in parameter.Childs) {
				child.Parent = parameter;
			}

			return parameter;
		}
	}
}
