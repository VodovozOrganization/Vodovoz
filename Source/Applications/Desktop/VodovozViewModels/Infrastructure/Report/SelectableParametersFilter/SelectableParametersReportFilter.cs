using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using QS.DomainModel.UoW;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParametersReportFilter
	{
		private HashSet<string> parameterNames = new HashSet<string>();
		private readonly IUnitOfWork uow;

		public GenericObservableList<SelectableParameterSet> ParameterSets { get; } = new GenericObservableList<SelectableParameterSet>();

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
			parameterSet.PropertyChanged += ParameterSetOnPropertyChanged;
			ParameterSets.Add(parameterSet);

			return parameterSet;
		}

		private void ParameterSetOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(SelectableParameterSet.IsVisible))
			{
				if(!(sender is SelectableParameterSet parameterSet))
				{
					return;
				}
				
				if(parameterSet.IsVisible)
				{
					AddHidedParameterSet(parameterSet);
				}
				else
				{
					HideParameterSet(parameterSet);
				}
			}
		}

		private void AddHidedParameterSet(SelectableParameterSet parameterSet)
		{
			if(ParameterSets.Contains(parameterSet))
			{
				return;
			}
			ParameterSets.Add(parameterSet);
		}

		private void HideParameterSet(SelectableParameterSet parameterSet)
		{
			if(!ParameterSets.Contains(parameterSet))
			{
				return;
			}
			ParameterSets.Remove(parameterSet);
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
		
		public IDictionary<string, string> GetSelectedParametersTitlesFromParameterSet(string parameterSet)
		{
			var result = new Dictionary<string, string>();
			var paramSet = ParameterSets.SingleOrDefault(x => x.ParameterName == parameterSet);
			
			return paramSet != null ? paramSet.GetSelectedParametersTitles() : result;
		}
	}
}
