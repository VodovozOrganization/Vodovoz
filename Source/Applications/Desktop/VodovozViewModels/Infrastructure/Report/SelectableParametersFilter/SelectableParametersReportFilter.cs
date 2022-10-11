using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParametersReportFilter
	{
		private HashSet<string> parameterNames = new HashSet<string>();
		private readonly IUnitOfWork uow;

		public List<SelectableParameterSet> ParameterSets { get; } = new List<SelectableParameterSet>();

		public SelectableParametersReportFilter(IUnitOfWork uow)
		{
			this.uow = uow ?? throw new ArgumentNullException(nameof(uow));
		}

		public SelectableParameterSet CreateParameterSet(string name, string parameterName, IParametersFactory parametersFactory,  string includeSuffix = "_include", string excludeSuffix = "_exclude")
		{
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			if(parameterNames.Contains(parameterName)) {
				throw new InvalidOperationException($"Параметр с именем {parameterName} уже был добавлен.");
			}

			if(parametersFactory == null) {
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			parameterNames.Add(parameterName);

			SelectableParameterSet parameterSet = new SelectableParameterSet(name, parametersFactory, parameterName, includeSuffix, excludeSuffix);

			ParameterSets.Add(parameterSet);

			return parameterSet;
		}

		public IDictionary<string, object> GetParameters()
		{
			Dictionary<string, object> result = new Dictionary<string, object>();

			foreach(var parameterSet in ParameterSets) {
				foreach(var parameter in parameterSet.GetParameters()) {
					result.Add(parameter.Key, parameter.Value);
				}
			}

			return result;
		}
	}
}
