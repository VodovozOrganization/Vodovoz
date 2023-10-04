﻿using QS.Commands;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public abstract partial class IncludeExcludeFilter : PropertyChangedBase, IDisposable
	{
		public const string defaultIncludePrefix = "_include";
		public const string defaultExcludePrefix = "_exclude";

		private string _title;

		private Func<IncludeExcludeFilter, IDictionary<string, object>> _getReportParametersFunc;

		internal IncludeExcludeFilter()
		{
			IncludedElements.ListContentChanged += (s, e) => OnPropertyChanged(nameof(IncludedCount));
			ExcludedElements.ListContentChanged += (s, e) => OnPropertyChanged(nameof(ExcludedCount));

			GetReportParametersFunc = DefaultGetReportParameters;

			ClearExcludesCommand = new DelegateCommand(ClearExcludes);
			ClearIncludesCommand = new DelegateCommand(ClearIncludes);
			RefreshFilteredElementsCommand = new DelegateCommand(RefreshFilteredElements);
		}

		public virtual GenericObservableList<IncludeExcludeElement> FilteredElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public GenericObservableList<IncludeExcludeElement> IncludedElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public GenericObservableList<IncludeExcludeElement> ExcludedElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public DelegateCommand RefreshFilteredElementsCommand { get; }

		public DelegateCommand ClearExcludesCommand { get; }

		public DelegateCommand ClearIncludesCommand { get; }

		public int IncludedCount => IncludedElements.Count;

		public int ExcludedCount => ExcludedElements.Count;

		public Type Type { get; set; }

		public EventHandler SelectionChanged;

		public Func<IncludeExcludeFilter, IDictionary<string, object>> GetReportParametersFunc
		{
			get => _getReportParametersFunc;
			set => _getReportParametersFunc = value;
		}

		public string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		protected virtual void ClearExcludes()
		{
			while(ExcludedCount > 0)
			{
				var currentExcludedElement = ExcludedElements.FirstOrDefault();

				if(currentExcludedElement != null)
				{
					currentExcludedElement.Exclude = false;
				}
			}

			SelectionChanged?.Invoke(this, null);
		}

		protected virtual void ClearIncludes()
		{
			while(IncludedCount > 0)
			{
				var currentIncludedElement = IncludedElements.FirstOrDefault();

				if(currentIncludedElement != null)
				{
					currentIncludedElement.Include = false;
				}
			}

			SelectionChanged?.Invoke(this, null);
		}

		protected void BeforeRefreshFilteredElements()
		{
			UnsubscribeFromElements(FilteredElements);

			FilteredElements.Clear();
		}

		public virtual void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			AfterRefreshFilteredElements();
		}

		protected void AfterRefreshFilteredElements()
		{
			SubscribeToElements(FilteredElements);

			RefreshIncludedElements(FilteredElements);

			RefreshExcludedElements(FilteredElements);
		}

		private void RefreshIncludedElements(GenericObservableList<IncludeExcludeElement> filteredElements)
		{
			for(var i = 0; i < IncludedElements.Count; i++)
			{
				IncludeExcludeElement replacement = null;

				foreach(var element in filteredElements)
				{
					if(element.Number == IncludedElements[i].Number)
					{
						replacement = element;

						break;
					}

					if(element.Children.Count > 0)
					{
						RefreshIncludedElements(element.Children);
					}
				}

				if(replacement != null)
				{
					replacement.Include = true;
					IncludedElements[i] = replacement;
				}
			}
		}

		private void RefreshExcludedElements(GenericObservableList<IncludeExcludeElement> filteredElements)
		{
			for(var i = 0; i < ExcludedElements.Count; i++)
			{
				IncludeExcludeElement replacement = null;

				foreach(var element in filteredElements)
				{
					if(element.Number == ExcludedElements[i].Number)
					{
						replacement = element;

						break;
					}

					if(element.Children.Count > 0)
					{
						RefreshExcludedElements(element.Children);
					}
				}

				if(replacement != null)
				{
					replacement.Exclude = true;
					ExcludedElements[i] = replacement;
				}
			}
		}

		private void OnElementUnExcluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			ExcludedElements.Remove(sender);
			OnPropertyChanged(nameof(ExcludedCount));
		}

		private void OnElementUnIncluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			IncludedElements.Remove(sender);
			OnPropertyChanged(nameof(IncludedCount));
		}

		private void OnElementExcluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			if(!ExcludedElements.Any(x => x.Number == sender.Number))
			{
				ExcludedElements.Add(sender);
				OnPropertyChanged(nameof(ExcludedCount));
			}
		}

		private void OnElementIncluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			if(!IncludedElements.Any(x => x.Number == sender.Number))
			{
				IncludedElements.Add(sender);
				OnPropertyChanged(nameof(IncludedCount));
			}
		}

		private void UnsubscribeFromElements(IEnumerable<IncludeExcludeElement> elements)
		{
			foreach(var element in elements)
			{
				if(element.Children.Count > 0)
				{
					UnsubscribeFromElements(element.Children);
				}

				UnsubscribeFromElement(element);
			}
		}

		private void SubscribeToElements(IEnumerable<IncludeExcludeElement> elements)
		{
			foreach(var element in elements)
			{
				if(element.Children.Count > 0)
				{
					SubscribeToElements(element.Children);
				}

				SubscribeToElement(element);
			}
		}

		private void SubscribeToElement(IncludeExcludeElement element)
		{
			element.ElementIncluded += OnElementIncluded;
			element.ElementExcluded += OnElementExcluded;
			element.ElementUnIncluded += OnElementUnIncluded;
			element.ElementUnExcluded += OnElementUnExcluded;
		}

		private void UnsubscribeFromElement(IncludeExcludeElement element)
		{
			element.ElementIncluded -= OnElementIncluded;
			element.ElementExcluded -= OnElementExcluded;
			element.ElementUnIncluded -= OnElementUnIncluded;
			element.ElementUnExcluded -= OnElementUnExcluded;
		}

		public void Dispose()
		{
			BeforeRefreshFilteredElements();
			IncludedElements.Clear();
			ExcludedElements.Clear();
		}

		public virtual IDictionary<string, object> GetReportParameters()
			=> GetReportParametersFunc.Invoke(this);

		private IDictionary<string, object> DefaultGetReportParameters(IncludeExcludeFilter filter)
		{
			var result = new Dictionary<string, object>();

			var includeParameterName = filter.Type.Name + defaultIncludePrefix;

			if(filter.IncludedCount > 0)
			{
				result.Add(includeParameterName, filter.IncludedElements.Select(x => x.Number).ToArray());
			}
			else
			{
				result.Add(includeParameterName, new object[] { "0" });
			}

			var excludeParameterName = filter.Type.Name + defaultExcludePrefix;

			if(filter.ExcludedCount > 0)
			{
				result.Add(excludeParameterName, filter.ExcludedElements.Select(x => x.Number).ToArray());
			}
			else
			{
				result.Add(excludeParameterName, new object[] { "0" });
			}

			return result;
		}
	}
}
