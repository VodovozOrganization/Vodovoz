using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public interface IParametersFactory
	{
		bool IsRecursiveFactory { get; }
		IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations);
	}
}
