using System;
using System.Collections.Generic;
using NHibernate.Mapping;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParametersReportFilter
	{
		private List<SelectableParameterSet> parameterSets = new List<SelectableParameterSet>();

		public SelectableParametersReportFilter()
		{
		}

		public void CreateParameterSet<TEntity>(IEnumerable<TEntity> source, IParametersFactory<TEntity> parametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
			where TEntity : class, IDomainObject
		{
			if(source == null) {
				throw new ArgumentNullException(nameof(source));
			}

			if(parametersFactory == null) {
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			var parameters = parametersFactory.GetParameters(source);

			SelectableParameterSet parameterSet = new SelectableParameterSet(parameters, parameterName);

			parameterSets.Add(parameterSet);
		}
	}
}
