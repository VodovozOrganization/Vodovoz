using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class ParametersFactory: IParametersFactory
	{
		private readonly IUnitOfWork uow;
		private readonly Func<IEnumerable<Func<ICriterion>>, IList<SelectableParameter>> sourceFunc;

		public ParametersFactory(IUnitOfWork uow, Func<IEnumerable<Func<ICriterion>>, IList<SelectableParameter>> sourceFunc)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
			this.sourceFunc = sourceFunc ?? throw new ArgumentNullException(nameof(sourceFunc));
		}

		public bool IsRecursiveFactory { get; }

		public IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations)
		{
			if(filterRelations == null) {
				throw new ArgumentNullException(nameof(filterRelations));
			}

			return sourceFunc(filterRelations);
		}
	}
}
