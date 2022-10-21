using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class ParametersEnumFactory<TEnum> : IParametersFactory
	{
		private readonly TEnum[] _excluded;

		public ParametersEnumFactory(TEnum[] excluded = null)
		{
			_excluded = excluded;
		}

		public bool IsRecursiveFactory { get; }

		public IList<SelectableParameter> GetParameters(IEnumerable<Func<ICriterion>> filterRelations)
		{
			var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Where(val => !_excluded?.Contains(val) ?? true);
			List<SelectableParameter> result = new List<SelectableParameter>();
			foreach(var enumValue in values) {
				SelectableParameter parameter = new SelectableEnumParameter<TEnum>(enumValue);
				result.Add(parameter);
			}
			return result;
		}
	}
}
