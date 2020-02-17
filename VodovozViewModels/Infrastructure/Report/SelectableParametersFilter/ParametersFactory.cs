using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class ParametersFactory<TEntity> : IParametersFactory<TEntity>
		where TEntity : class, IDomainObject
	{
		public ParametersFactory()
		{
		}

		public IList<SelectableParameter> GetParameters(IEnumerable<TEntity> entities)
		{
			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var entity in entities) {
				result.Add(CreateParameter(entity));
			}
			return result;
		}

		private SelectableParameter<TEntity> CreateParameter(TEntity entity)
		{
			if(entity == null) {
				throw new ArgumentNullException(nameof(entity));
			}

			SelectableParameter<TEntity> parameter = new SelectableParameter<TEntity>(entity);
			return parameter;
		}
	}
}
