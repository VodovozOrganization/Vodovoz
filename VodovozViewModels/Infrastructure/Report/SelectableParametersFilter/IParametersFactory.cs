using System;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory
	{
		IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations);
	}
}
