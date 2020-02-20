using System;
using System.Collections.Generic;
using QS.DomainModel.Entity;
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

		public SelectableEntityParameterSet<TEntity> CreateEntityParameterSet<TEntity>(string name, IParametersEntityFactory<TEntity> parametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
			where TEntity : class, IDomainObject
		{
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			if(parametersFactory == null) {
				throw new ArgumentNullException(nameof(parametersFactory));
			}

			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			if(parameterNames.Contains(parameterName)) {
				throw new InvalidOperationException($"Параметр с именем {parameterName} уже был добавлен.");
			}
			parameterNames.Add(parameterName);

			SelectableEntityParameterSet<TEntity> parameterSet = new SelectableEntityParameterSet<TEntity>(name, parametersFactory, parameterName, includeSuffix, excludeSuffix);

			ParameterSets.Add(parameterSet);

			return parameterSet;
		}

		public SelectableParameterSet CreateEnumParameterSet<TEnum>(string name, IParametersEnumFactory<TEnum> enumParametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
		{
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			if(enumParametersFactory == null) {
				throw new ArgumentNullException(nameof(enumParametersFactory));
			}

			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			if(parameterNames.Contains(parameterName)) {
				throw new InvalidOperationException($"Параметр с именем {parameterName} уже был добавлен.");
			}

			SelectableParameterSet parameterSet = new SelectableParameterSet(name, enumParametersFactory, parameterName, includeSuffix, excludeSuffix);

			ParameterSets.Add(parameterSet);

			return parameterSet;
		}

		public IDictionary<string, object> GetParameters()
		{
			Dictionary<string, object> result = new Dictionary<string, object>();

			foreach(var parameterSet in ParameterSets) {
				var selectedValues = parameterSet.GetSelectedValues();
				foreach(var parameter in parameterSet.GetSelectedValues()) {
					result.Add(parameter.Key, parameter.Value);
				}
			}

			return result;
		}
	}
}
