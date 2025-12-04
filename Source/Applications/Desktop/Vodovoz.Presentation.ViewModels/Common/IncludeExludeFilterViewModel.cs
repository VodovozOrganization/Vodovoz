using NHibernate.Linq;
using NHibernate.Util;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Extensions;
using Vodovoz.Tools;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public class IncludeExludeFilterGroupViewModel : WidgetViewModelBase
	{
		private readonly GenericObservableList<IncludeExcludeElement> _emptyElements = new GenericObservableList<IncludeExcludeElement>();

		public const string defaultIncludePrefix = "_include";
		public const string defaultExcludePrefix = "_exclude";
		private const int _defaultLimit = 200;

		private string _title;
		private bool _showArchived;
		private string _searchString;

		private IncludeExcludeFilter _filter;

		private Func<IncludeExludeFilterGroupViewModel, IDictionary<string, object>> _getReportParametersFunc;

		public event Action<object, EventArgs> FilteredElementsChanged;
		public event Action<object, SearchStringChangedEventArgs> SearchStringChanged;

		public IncludeExludeFilterGroupViewModel()
		{
			IncludedElements.ListContentChanged += (s, e) => OnPropertyChanged(nameof(IncludedCount));
			ExcludedElements.ListContentChanged += (s, e) => OnPropertyChanged(nameof(ExcludedCount));
			SearchStringChanged += (s, e) => RefreshFilteredElements();

			GetReportParametersFunc = DefaultGetReportParameters;

			ClearExcludesCommand = new DelegateCommand(ClearExcludes);
			ClearIncludesCommand = new DelegateCommand(ClearIncludes);
			RefreshFilteredElementsCommand = new DelegateCommand(RefreshFilteredElements);
			ClearSearchStringCommand = new DelegateCommand(ClearSearchString);
			RaiseSelectionChangedCommand = new DelegateCommand(RaiseSelectionChanged);
		}

		public bool WithExcludes { get; set; } = true;

		public EventHandler SelectionChanged;

		public DelegateCommand RefreshFilteredElementsCommand { get; }

		public DelegateCommand ClearExcludesCommand { get; }

		public DelegateCommand ClearIncludesCommand { get; }

		public DelegateCommand ClearSearchStringCommand { get; }
		
		public DelegateCommand RaiseSelectionChangedCommand { get; }

		public virtual GenericObservableList<IncludeExcludeElement> FilteredElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public GenericObservableList<IncludeExcludeElement> IncludedElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public GenericObservableList<IncludeExcludeElement> ExcludedElements { get; } = new GenericObservableList<IncludeExcludeElement>();

		public GenericObservableList<IncludeExcludeElement> Elements => FilteredElements ?? _emptyElements;

		public Type Type { get; set; }

		public bool ShowArchived
		{
			get => _showArchived;
			set
			{
				if(SetField(ref _showArchived, value))
				{
					UpdateFilteredElements();
				}
			}
		}

		public string Title
		{
			get => _title;
			set => SetField(ref _title, value);
		}

		public string SearchString
		{
			get => _searchString;
			set
			{
				if(SetField(ref _searchString, value))
				{
					UpdateFilteredElements();
					SearchStringChanged?.Invoke(this, new SearchStringChangedEventArgs(value));
				}
			}
		}

		public int IncludedCount => IncludedElements.Count;

		public int ExcludedCount => ExcludedElements.Count;


		public Func<IncludeExludeFilterGroupViewModel, IDictionary<string, object>> GetReportParametersFunc
		{
			get => _getReportParametersFunc;
			set => _getReportParametersFunc = value;
		}

		public virtual IDictionary<string, object> GetReportParameters()
			=> GetReportParametersFunc.Invoke(this);

		public void InitializeFor<TEntity>(
			IUnitOfWork unitOfWork,
			IGenericRepository<TEntity> repository,
			Action<IncludeExcludeEntityFilter<TEntity>> includeExcludeFilter = null)
			where TEntity : class, IDomainObject
		{
			Title = typeof(TEntity).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			var newFilter = new IncludeExcludeEntityFilter<TEntity>
			{
				Title = Title,
				GenitivePluralTitle = typeof(TEntity).GetClassUserFriendlyName().GenitivePlural,
				Type = typeof(TEntity)
			};

			newFilter.RefreshFunc = (IncludeExcludeEntityFilter<TEntity> filter) =>
			{
				var isArchivable = typeof(IArchivable).IsAssignableFrom(typeof(TEntity));

				var isNamed = typeof(INamed).IsAssignableFrom(typeof(TEntity));

				var isTitled = typeof(ITitled).IsAssignableFrom(typeof(TEntity));

				Expression<Func<TEntity, bool>> specificationExpression = null;

				if(isArchivable)
				{
					Expression<Func<TEntity, bool>> isArchiveSpec = entity => ShowArchived || !((IArchivable)entity).IsArchive;

					specificationExpression = specificationExpression.CombineWith(isArchiveSpec);
				}

				if(isNamed && !string.IsNullOrWhiteSpace(SearchString))
				{
					Expression<Func<TEntity, bool>> isNamedSearchSpec = entity => ((INamed)entity).Name.ToLower().Like($"%{SearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isNamedSearchSpec);
				}

				if(isTitled && !string.IsNullOrWhiteSpace(SearchString))
				{
					Expression<Func<TEntity, bool>> isTitledSearchSpec = entity => ((ITitled)entity).Title.ToLower().Like($"%{SearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isTitledSearchSpec);
				}

				if(filter.Specification != null)
				{
					specificationExpression = specificationExpression.CombineWith(filter.Specification);
				}

				var elementsToAdd = repository.Get(
						unitOfWork,
						specificationExpression,
						limit: _defaultLimit)
					.Select(x => new IncludeExcludeElement<int, TEntity>
					{
						Id = x.Id,
						Title = isNamed ? (x as INamed).Name : x.GetTitle(),
					});

				filter.FilteredElements.Clear();

				foreach(var element in elementsToAdd)
				{
					filter.FilteredElements.Add(element);
				}
			};

			includeExcludeFilter?.Invoke(newFilter);

			newFilter.SelectionChanged = SelectionChanged;

			_filter = newFilter;
		}

		public virtual void RefreshFilteredElements()
		{
			BeforeRefreshFilteredElements();

			_filter.RefreshFilteredElementsCommand.Execute();

			foreach(var element in _filter.FilteredElements)
			{
				FilteredElements.Add(element);
			}

			AfterRefreshFilteredElements();
		}

		protected void BeforeRefreshFilteredElements()
		{
			UnsubscribeFromElements(FilteredElements);

			FilteredElements.Clear();
		}

		protected void AfterRefreshFilteredElements()
		{
			SubscribeToElements(FilteredElements);

			RefreshIncludedElements(FilteredElements);

			RefreshExcludedElements(FilteredElements);
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

		private void UpdateFilteredElements()
		{
			RefreshFilteredElements();

			FilteredElementsChanged?.Invoke(this, EventArgs.Empty);
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

		private IDictionary<string, object> DefaultGetReportParameters(IncludeExludeFilterGroupViewModel filter)
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

		private void RaiseSelectionChanged()
		{
			SelectionChanged?.Invoke(this, null);
		}

		private void ClearSearchString()
		{
			SearchString = string.Empty;
		}

		public void Dispose()
		{
			BeforeRefreshFilteredElements();
			IncludedElements.Clear();
			ExcludedElements.Clear();
		}
	}
}
