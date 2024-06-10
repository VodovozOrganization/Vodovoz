using NHibernate.Criterion;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vodovoz.Infrastructure.Report.SelectableParametersFilter
{
	public class SelectableParameterSet : PropertyChangedBase
	{
		private const string _emptyIncludeParameter = "Все";
		private const string _emptyExcludeParameter = "Нет";

		private GenericObservableList<SelectableParameter> _outputParameters = new GenericObservableList<SelectableParameter>();
		private GenericObservableList<SelectableParameter> _parameters;

		private readonly IParametersFactory _parametersFactory;

		private readonly string _includeSuffix;
		private readonly string _excludeSuffix;

		private SelectableFilterType _filterType;

		private string _searchValue;

		private bool _isVisible = true;
		private bool _suppressSelectionChangedEvent;

		public SelectableParameterSet(string name, IParametersFactory parametersFactory, string parameterName, string includeSuffix = "_include", string excludeSuffix = "_exclude")
		{
			if(string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			_parametersFactory = parametersFactory ?? throw new ArgumentNullException(nameof(parametersFactory));

			if(string.IsNullOrWhiteSpace(parameterName))
			{
				throw new ArgumentNullException(nameof(parameterName));
			}

			_includeSuffix = includeSuffix;
			_excludeSuffix = excludeSuffix;

			Name = name;
			ParameterName = parameterName;
		}

		public event EventHandler<SelectableParameterSetSelectionChanged> SelectionChanged;

		protected List<Func<ICriterion>> FilterRelations { get; } = new List<Func<ICriterion>>();

		public string Name { get; set; }

		public virtual object[] EmptyValue { get; set; } = new object[] { "0" };

		public virtual SelectableFilterType FilterType
		{
			get => _filterType;
			set => SetField(ref _filterType, value);
		}

		public bool IsVisible
		{
			get => _isVisible;
			set => SetField(ref _isVisible, value);
		}

		public virtual GenericObservableList<SelectableParameter> OutputParameters
		{
			get => _outputParameters;
			set => SetField(ref _outputParameters, value);
		}

		public string SearchValue => _searchValue;

		public GenericObservableList<SelectableParameter> Parameters
		{
			get
			{
				if(_parameters == null)
				{
					_parameters = new GenericObservableList<SelectableParameter>(_parametersFactory.GetParameters(FilterRelations));

					foreach(SelectableParameter parameter in _parameters)
					{
						parameter.AnySelectedChanged += Parameter_AnySelectedChanged;
					}
				}

				return _parameters;
			}
			set
			{
				var oldParameters = _parameters;

				if(oldParameters != null)
				{
					foreach(SelectableParameter oldParameter in oldParameters)
					{
						oldParameter.AnySelectedChanged -= Parameter_AnySelectedChanged;
					}
				}

				if(SetField(ref _parameters, value) && _parameters != null)
				{
					foreach(SelectableParameter parameter in Parameters)
					{
						parameter.AnySelectedChanged += Parameter_AnySelectedChanged;
					}

					UpdateOutputParameters();
				}
			}
		}

		public string ParameterName { get; }

		private void Parameter_AnySelectedChanged(object sender, SelectableParameterSelectionChangedEventArgs e)
		{
			RaiseSelectionChanged(e);
		}

		private void RaiseSelectionChanged(params SelectableParameterSelectionChangedEventArgs[] changes)
		{
			if(_suppressSelectionChangedEvent)
			{
				return;
			}

			SelectionChanged?.Invoke(this, new SelectableParameterSetSelectionChanged(ParameterName, changes));
		}

		public void FilterParameters(string searchValue)
		{
			_searchValue = searchValue;
			UpdateOutputParameters();
		}

		public void UpdateOutputParameters()
		{
			foreach(SelectableParameter sp in Parameters)
			{
				sp.FilterChilds(_searchValue);
			}

			if(Parameters.Any(x => x.Children.Any()))
			{
				OutputParameters = new GenericObservableList<SelectableParameter>(
					Parameters
						.Where(x => x.Children.Any()
							|| x.Title.ToLower().Contains(_searchValue == null ? "" : _searchValue.ToLower())
							|| x.Value.ToString() == _searchValue)
						.ToList());
			}
			else
			{
				OutputParameters = new GenericObservableList<SelectableParameter>(
					Parameters
						.Where(x => x.Title.ToLower().Contains(_searchValue == null ? "" : _searchValue.ToLower())
							|| x.Value.ToString() == _searchValue)
						.ToList());
			}
		}

		public void SelectAll()
		{
			_suppressSelectionChangedEvent = true;
			var changes = new List<SelectableParameterSelectionChangedEventArgs>();

			foreach(SelectableParameter value in OutputParameters)
			{
				value.Selected = true;
				changes.Add(new SelectableParameterSelectionChangedEventArgs(value.Value, value.Title, true));
			}

			_suppressSelectionChangedEvent = false;
			RaiseSelectionChanged(changes.ToArray());
		}

		public void UnselectAll()
		{
			_suppressSelectionChangedEvent = true;
			var changes = new List<SelectableParameterSelectionChangedEventArgs>();

			foreach(SelectableParameter value in OutputParameters)
			{
				value.Selected = false;
				changes.Add(new SelectableParameterSelectionChangedEventArgs(value.Value, value.Title, false));
			}

			_suppressSelectionChangedEvent = false;
			RaiseSelectionChanged(changes.ToArray());
		}

		public IEnumerable<object> GetSelectedValues()
		{
			return OutputParameters.SelectMany(x => x.GetAllSelected().Select(y => y.Value));
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

			switch(FilterType)
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

				result.Add($"{ParameterName}{_includeSuffix}", FilterType == SelectableFilterType.Include ? GetValidSelectedValues(selectedValues) : EmptyValue);
				result.Add($"{ParameterName}{_excludeSuffix}", FilterType == SelectableFilterType.Exclude ? GetValidSelectedValues(selectedValues) : EmptyValue);
			}

			return result;
		}

		private object[] GetValidSelectedValues(IEnumerable<object> selectedValues)
		{
			if(!selectedValues.Any())
			{
				return EmptyValue;
			}

			return selectedValues.ToArray();
		}

		public void AddFilterOnSourceSelectionChanged(SelectableParameterSet sourceParameterSet, Func<ICriterion> filterCriterionFunc)
		{
			if(sourceParameterSet == null)
			{
				throw new ArgumentNullException(nameof(sourceParameterSet));
			}

			if(filterCriterionFunc == null)
			{
				throw new ArgumentNullException(nameof(filterCriterionFunc));
			}

			FilterRelations.Add(filterCriterionFunc);

			sourceParameterSet.SelectionChanged -= OnMasterParameterSetSelectionChanged;
			sourceParameterSet.SelectionChanged += OnMasterParameterSetSelectionChanged;
		}

		private void OnMasterParameterSetSelectionChanged(object sender, EventArgs e)
		{
			UpdateParameters();
		}
		
		public void UpdateParameters()
		{
			Parameters = new GenericObservableList<SelectableParameter>(_parametersFactory.GetParameters(FilterRelations));
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
			return OutputParameters.SelectMany(x => x.GetAllSelected().Select(y => y.Title)).ToArray();
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
