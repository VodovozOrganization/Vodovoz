using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSet : PropertyChangedBase
	{
		private readonly IParametersFactory parametersFactory;
		private readonly string includeSuffix;
		private readonly string excludeSuffix;

		public string Name { get; set; }

		public virtual object[] EmptyValue { get; set; } = new object[] { "0" };

		private SelectableFilterType filterType;
		public virtual SelectableFilterType FilterType {
			get => filterType;
			set => SetField(ref filterType, value);
		}

		private GenericObservableList<SelectableParameter> parameters;
		public GenericObservableList<SelectableParameter> Parameters {
			get {
				if(parameters == null) {
					parameters = new GenericObservableList<SelectableParameter>(parametersFactory.GetParameters());
				}
				return parameters;
			}

			set {
				var oldParameters = parameters;
				if(oldParameters != null) {
					foreach(SelectableParameter oldParameter in oldParameters) {
						oldParameter.AnySelectedChanged -= Parameter_AnySelectedChanged;
					}
				}
				if(SetField(ref parameters, value) && parameters != null) {
					foreach(SelectableParameter parameter in Parameters) {
						parameter.AnySelectedChanged += Parameter_AnySelectedChanged;
					}
				}
			}
		}

		public string ParameterName { get; }

		public event EventHandler SelectionChanged;

		public SelectableParameterSet(string name, IParametersFactory parametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
		{
			if(string.IsNullOrWhiteSpace(name)) {
				throw new ArgumentNullException(nameof(name));
			}

			if(string.IsNullOrWhiteSpace(parameterName)) {
				throw new ArgumentNullException(nameof(parameterName));
			}

			this.includeSuffix = includeSuffix;
			this.excludeSuffix = excludeSuffix;

			Name = name;
			this.parametersFactory = parametersFactory ?? throw new ArgumentNullException(nameof(parametersFactory));
			ParameterName = parameterName;
		}

		void Parameter_AnySelectedChanged(object sender, EventArgs e)
		{
			RaiseSelectionChanged();
		}

		private bool suppressSelectionChangedEvent;

		private void RaiseSelectionChanged()
		{
			if(suppressSelectionChangedEvent) {
				return;
			}
			SelectionChanged?.Invoke(this, EventArgs.Empty);
		}

		public void SelectAll()
		{
			suppressSelectionChangedEvent = true;
			foreach(SelectableParameter value in Parameters) {
				value.Selected = true;
			}
			suppressSelectionChangedEvent = false;
			RaiseSelectionChanged();
		}

		public void UnselectAll()
		{
			suppressSelectionChangedEvent = true;
			foreach(SelectableParameter value in Parameters) {
				value.Selected = false;
			}
			suppressSelectionChangedEvent = false;
			RaiseSelectionChanged();
		}

		public Dictionary<string, object> GetSelectedValues()
		{
			Dictionary<string, object> result = new Dictionary<string, object>();

			var selectedValues = Parameters.SelectMany(x => x.GetAllSelected().Select(y => y.Value));

			result.Add($"{ParameterName}{includeSuffix}", FilterType == SelectableFilterType.Include ? GetValidSelectedValues(selectedValues) : EmptyValue);
			result.Add($"{ParameterName}{excludeSuffix}", FilterType == SelectableFilterType.Exclude ? GetValidSelectedValues(selectedValues) : EmptyValue);

			return result;
		}

		private object[] GetValidSelectedValues(IEnumerable<object> selectedValues)
		{
			if(!selectedValues.Any()) {
				return EmptyValue;
			}
			return selectedValues.ToArray();
		}
	}
}
