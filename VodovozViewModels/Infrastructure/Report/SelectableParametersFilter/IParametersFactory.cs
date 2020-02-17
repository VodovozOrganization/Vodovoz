using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory<TEntity>
		where TEntity : class, IDomainObject
	{
		IList<SelectableParameter> GetParameters(IEnumerable<TEntity> entities);
	}
}
