using QS.Commands;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
			IncludedElements.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IncludedCount));
			ExcludedElements.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ExcludedCount));

			GetReportParametersFunc = DefaultGetReportParameters;

			SelectAllCommand = new DelegateCommand(SelectAll);
			UnselectAllCommand = new DelegateCommand(UnselectAll);
			RefreshFilteredElementsCommand = new DelegateCommand(RefreshFilteredElements);
		}

		public virtual ObservableCollection<IncludeExcludeElement> FilteredElements { get; } = new ObservableCollection<IncludeExcludeElement>();

		public ObservableCollection<IncludeExcludeElement> IncludedElements { get; } = new ObservableCollection<IncludeExcludeElement>();

		public ObservableCollection<IncludeExcludeElement> ExcludedElements { get; } = new ObservableCollection<IncludeExcludeElement>();

		public DelegateCommand RefreshFilteredElementsCommand { get; }

		public DelegateCommand SelectAllCommand { get; }

		public DelegateCommand UnselectAllCommand { get; }

		public int IncludedCount => IncludedElements.Count;

		public int ExcludedCount => ExcludedElements.Count;

		public Type Type { get; set; }

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

		protected virtual void SelectAll()
		{
			while(ExcludedCount > 0)
			{
				ExcludedElements.First().Exclude = false;
			}
		}

		protected virtual void UnselectAll()
		{
			while(IncludedCount > 0)
			{
				IncludedElements.First().Include = false;
			}
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

		private void RefreshIncludedElements(ObservableCollection<IncludeExcludeElement> filteredElements)
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

		private void RefreshExcludedElements(ObservableCollection<IncludeExcludeElement> filteredElements)
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
		}

		private void OnElementUnIncluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			IncludedElements.Remove(sender);
		}

		private void OnElementExcluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			if(!ExcludedElements.Any(x => x.Number == sender.Number))
			{
				ExcludedElements.Add(sender);
			}
		}

		private void OnElementIncluded(IncludeExcludeElement sender, EventArgs eventArgs)
		{
			if(!IncludedElements.Any(x => x.Number == sender.Number))
			{
				IncludedElements.Add(sender);
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
			element.ElementIncluded += OnElementIncluded;
			element.ElementExcluded += OnElementExcluded;
			element.ElementUnIncluded += OnElementUnIncluded;
			element.ElementUnExcluded += OnElementUnExcluded;
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
