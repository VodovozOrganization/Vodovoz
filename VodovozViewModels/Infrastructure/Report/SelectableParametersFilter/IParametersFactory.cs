using System;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory
	{
		bool IsRecursiveFactory { get; }
		IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations);
	}
}
