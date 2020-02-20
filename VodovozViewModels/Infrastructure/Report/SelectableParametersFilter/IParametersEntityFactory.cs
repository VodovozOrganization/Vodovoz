using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersEntityFactory<TEntity> : IParametersFactory
		where TEntity : class, IDomainObject
	{
		IList<SelectableParameter> GetParameters(Expression<Func<TEntity, object>> filterExpression, SelectableParameterSet filterSource);
	}
}