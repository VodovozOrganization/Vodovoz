using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using QS.DomainModel.Entity;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSet : PropertyChangedBase
	{
		private readonly IParametersFactory parametersFactory;
		private readonly string includeSuffix;
		private readonly string excludeSuffix;
		private const string _emptyIncludeParameter = "Все";
		private const string _emptyExcludeParameter = "Нет";
		private bool _isVisible = true;

		protected List<Func<ICriterion>> FilterRelations { get; } = new List<Func<ICriterion>>();

		public string Name { get; set; }

		public virtual object[] EmptyValue { get; set; } = new object[] { "0" };

		private SelectableFilterType filterType;
		public virtual SelectableFilterType FilterType {
			get => filterType;
			set => SetField(ref filterType, value);
		}

		public bool IsVisible
		{
			get => _isVisible;
			set => SetField(ref _isVisible, value);
		}

		private GenericObservableList<SelectableParameter> outputParameters = new GenericObservableList<SelectableParameter>();
		public virtual GenericObservableList<SelectableParameter> OutputParameters {
			get => outputParameters;
			set => SetField(ref outputParameters, value, () => OutputParameters);
		}

		private GenericObservableList<SelectableParameter> parameters;
		public GenericObservableList<SelectableParameter> Parameters {
			get {
				if(parameters == null) {
					parameters = new GenericObservableList<SelectableParameter>(parametersFactory.GetParameters(FilterRelations));
					foreach(SelectableParameter parameter in parameters) {
						parameter.AnySelectedChanged += Parameter_AnySelectedChanged;
					}
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
					UpdateOutputParameters();
				}
			}
		}

		public string ParameterName { get; }

		public event EventHandler<SelectableParameterSetSelectionChanged> SelectionChanged;

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

		void Parameter_AnySelectedChanged(object sender, SelectableParameterSelectionChangedEventArgs e)
		{
			RaiseSelectionChanged(e);
		}

		private bool suppressSelectionChangedEvent;

		private void RaiseSelectionChanged(params SelectableParameterSelectionChangedEventArgs[] changes)
		{
			if(suppressSelectionChangedEvent) {
				return;
			}
			SelectionChanged?.Invoke(this, new SelectableParameterSetSelectionChanged(ParameterName, changes));
		}

		private string searchValue;

		public void FilterParameters(string searchValue)
		{
			this.searchValue = searchValue;
			UpdateOutputParameters();
		}

		public void UpdateOutputParameters()
		{
			foreach(SelectableParameter sp in Parameters)
			{
				sp.FilterChilds(searchValue);
			}

			if(Parameters.Any(x => x.Children.Any())) {
				OutputParameters = new GenericObservableList<SelectableParameter>(Parameters.Where(x => x.Children.Any() || x.Title.ToLower().Contains(searchValue == null ? "" : searchValue.ToLower())).ToList());
			} else {
				OutputParameters = new GenericObservableList<SelectableParameter>(Parameters.Where(x => x.Title.ToLower().Contains(searchValue == null ? "" : searchValue.ToLower())).ToList());
			}
		}

		public void SelectAll()
		{
			suppressSelectionChangedEvent = true;
			List<SelectableParameterSelectionChangedEventArgs> changes = new List<SelectableParameterSelectionChangedEventArgs>();
			foreach(SelectableParameter value in OutputParameters) {
				value.Selected = true;
				changes.Add(new SelectableParameterSelectionChangedEventArgs(value.Value, value.Title, true));
			}
			suppressSelectionChangedEvent = false;
			RaiseSelectionChanged(changes.ToArray());
		}

		public void UnselectAll()
		{
			suppressSelectionChangedEvent = true;
			List<SelectableParameterSelectionChangedEventArgs> changes = new List<SelectableParameterSelectionChangedEventArgs>();
			foreach(SelectableParameter value in OutputParameters) {
				value.Selected = false;
				changes.Add(new SelectableParameterSelectionChangedEventArgs(value.Value, value.Title, false));
			}
			suppressSelectionChangedEvent = false;
			RaiseSelectionChanged(changes.ToArray());
		}

		public IEnumerable<object> GetSelectedValues()
		{
			var selectedValues = OutputParameters.SelectMany(x => x.GetAllSelected().Select(y => y.Value));
			return selectedValues;
		}

		/// <summary>
		/// Возвращает выбранные параметры. Если фильтр в Exclude - вернет все, кроме выбранных параметров
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public IEnumerable<SelectableParameter> GetIncludedParameters()
		{
			var selectedValues = OutputParameters.SelectMany(x => x.GetAllSelected()).ToList();
			IEnumerable<SelectableParameter> includedValues;
			switch (FilterType)
			{
				case SelectableFilterType.Include:
					includedValues = selectedValues;
					break;
				case SelectableFilterType.Exclude:
					includedValues = OutputParameters.Where(x => !selectedValues.Select(sv => sv.Value).Contains(x.Value));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return includedValues;
		}

		public Dictionary<string, object> GetParameters()
		{
			var result = new Dictionary<string, object>();

			if(IsVisible)
			{
				var selectedValues = GetSelectedValues();

				result.Add($"{ParameterName}{includeSuffix}", FilterType == SelectableFilterType.Include ? GetValidSelectedValues(selectedValues) : EmptyValue);
				result.Add($"{ParameterName}{excludeSuffix}", FilterType == SelectableFilterType.Exclude ? GetValidSelectedValues(selectedValues) : EmptyValue);
			}

			return result;
		}

		private object[] GetValidSelectedValues(IEnumerable<object> selectedValues)
		{
			if(!selectedValues.Any()) {
				return EmptyValue;
			}
			return selectedValues.ToArray();
		}

		public void AddFilterOnSourceSelectionChanged(SelectableParameterSet sourceParameterSet, Func<ICriterion> filterCriterionFunc)
		{
			if(sourceParameterSet == null) {
				throw new ArgumentNullException(nameof(sourceParameterSet));
			}

			if(filterCriterionFunc == null) {
				throw new ArgumentNullException(nameof(filterCriterionFunc));
			}

			FilterRelations.Add(filterCriterionFunc);

			sourceParameterSet.SelectionChanged -= MasterParameterSet_SelectionChanged;
			sourceParameterSet.SelectionChanged += MasterParameterSet_SelectionChanged;
		}

		void MasterParameterSet_SelectionChanged(object sender, EventArgs e)
		{
			Parameters = new GenericObservableList<SelectableParameter>(parametersFactory.GetParameters(FilterRelations));
		}
		
		public Dictionary<string, string> GetSelectedParametersTitles()
		{
			var result = new Dictionary<string, string>();
			var sb = new StringBuilder();
			var selectedValuesTitles = GetSelectedValuesTitles();

			result.Add($"{Name} включая: ",
				FilterType == SelectableFilterType.Include
					? GetValidSelectedValuesTitles(sb, selectedValuesTitles, true)
					: _emptyIncludeParameter);
			result.Add($"{Name} исключая: ",
				FilterType == SelectableFilterType.Exclude
					? GetValidSelectedValuesTitles(sb, selectedValuesTitles, false)
					: _emptyExcludeParameter);

			return result;
		}
		
		private IEnumerable<string> GetSelectedValuesTitles()
		{
			var selectedValues =
				OutputParameters.SelectMany(x => x.GetAllSelected().Select(y => y.Title)).ToArray();
			return selectedValues;
		}

		private string GetValidSelectedValuesTitles(StringBuilder sb, IEnumerable<string> selectedValuesTitles, bool include)
		{
			sb.Clear();
			if(!selectedValuesTitles.Any())
			{
				if(include)
				{
					return _emptyIncludeParameter;
				}
				return _emptyExcludeParameter;
			}

			var count = selectedValuesTitles.Count();
			if(count < 5)
			{
				foreach(var item in selectedValuesTitles)
				{
					sb.Append($"{item}, ");
				}
			}
			else
			{
				sb.Append($"{count}");
			}

			return sb.ToString().Trim(' ', ',');
		}
	}
}
