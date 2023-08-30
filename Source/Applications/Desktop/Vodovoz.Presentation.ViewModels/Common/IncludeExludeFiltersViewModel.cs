using Gamma.Utilities;
using NHibernate.Linq;
using NHibernate.Util;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.EntityRepositories;
using Vodovoz.Extensions;
using Vodovoz.Tools;

namespace Vodovoz.Presentation.ViewModels.Common
{
	public partial class IncludeExludeFiltersViewModel : WidgetViewModelBase
	{
		private const int _defaultLimit = 200;

		private readonly GenericObservableList<IncludeExcludeElement> _emptyElements = new GenericObservableList<IncludeExcludeElement>();
		private readonly IInteractiveService _interactiveService;
		private string _searchString = string.Empty;
		private string _currentSearchString = string.Empty;
		private IncludeExcludeFilter _activeFilter;
		private bool _showArchived;

		public event Action<object, EventArgs> FilteredElementsChanged;

		public IncludeExludeFiltersViewModel(IInteractiveService interactiveService)
		{
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));

			ClearSearchStringCommand = new DelegateCommand(ClearSearchString);
			ClearAllExcludesCommand = new DelegateCommand(ClearAllExcludes);
			ClearAllIncludesCommand = new DelegateCommand(ClearAllIncludes);
			ShowInfoCommand = new DelegateCommand(ShowInfo);
		}

		public string SearchString
		{
			get => _searchString;
			set => SetField(ref _searchString, value);
		}

		public string CurrentSearchString
		{
			get => _currentSearchString;
			set
			{
				if(SetField(ref _currentSearchString, value))
				{
					UpdateFilteredElements();
				}
			}
		}

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

		public GenericObservableList<IncludeExcludeFilter> Filters { get; } = new GenericObservableList<IncludeExcludeFilter>();

		[PropertyChangedAlso(nameof(Elements))]
		public IncludeExcludeFilter ActiveFilter
		{
			get => _activeFilter;
			set
			{
				if(SetField(ref _activeFilter, value))
				{
					UpdateFilteredElements();
				}
			}
		}

		public static int DefaultLimit => _defaultLimit;

		public GenericObservableList<IncludeExcludeElement> Elements => ActiveFilter?.FilteredElements ?? _emptyElements;

		public DelegateCommand ClearSearchStringCommand { get; }

		public DelegateCommand ClearAllExcludesCommand { get; }

		public DelegateCommand ClearAllIncludesCommand { get; }

		public DelegateCommand ShowInfoCommand { get; }

		public Dictionary<string, object> GetReportParametersSet()
		{
			var result = new Dictionary<string, object>();

			foreach(var filter in Filters)
			{
				foreach(var parameter in filter.GetReportParameters())
				{
					result.Add(parameter.Key, parameter.Value);
				}
			}

			return result;
		}

		public IEnumerable<IncludeExcludeElement> GetIncludedElements<T>()
		{
			return Filters.FirstOrDefault(x => x.Type == typeof(T))?.IncludedElements ?? Enumerable.Empty<IncludeExcludeElement>();
		}

		public IEnumerable<IncludeExcludeElement> GetExcludedElements<T>()
		{
			return Filters.FirstOrDefault(x => x.Type == typeof(T))?.ExcludedElements ?? Enumerable.Empty<IncludeExcludeElement>();
		}

		#region Добавление фильтров

		public void AddFilter<TEntity>(
			IUnitOfWork unitOfWork,
			IGenericRepository<TEntity> repository,
			Action<IncludeExcludeEntityFilter<TEntity>> includeExcludeFilter = null)
			where TEntity : class, IDomainObject
		{
			var title = typeof(TEntity).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			var newFilter = new IncludeExcludeEntityFilter<TEntity>
			{
				Title = title,
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

				if(isNamed)
				{
					Expression<Func<TEntity, bool>> isArchiveSpec = entity => string.IsNullOrWhiteSpace(CurrentSearchString) || ((INamed)entity).Name.ToLower().Like($"%{CurrentSearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isArchiveSpec);
				}

				if(isTitled)
				{
					Expression<Func<TEntity, bool>> isArchiveSpec = entity => string.IsNullOrWhiteSpace(CurrentSearchString) || ((ITitled)entity).Title.ToLower().Like($"%{CurrentSearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isArchiveSpec);
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

			Filters.Add(newFilter);
		}

		public void AddFilter<TEnum>(Action<IncludeExcludeEnumFilter<TEnum>> includeExcludeFilter = null)
			where TEnum : Enum
		{
			var title = typeof(TEnum).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			var newFilter = new IncludeExcludeEnumFilter<TEnum>
			{
				Title = title,
				Type = typeof(TEnum)
			};

			newFilter.RefreshFunc = (filter) =>
			{
				var values = Enum.GetValues(typeof(TEnum));

				filter.FilteredElements.Clear();

				foreach(var value in values)
				{
					if(value is TEnum enumElement
						&& !filter.HideElements.Contains(enumElement)
						&& (string.IsNullOrWhiteSpace(CurrentSearchString)
							|| enumElement.GetEnumTitle().Contains(CurrentSearchString)))
					{
						filter.FilteredElements.Add(new IncludeExcludeElement<TEnum, TEnum>()
						{
							Id = enumElement,
							Title = enumElement.GetEnumTitle(),
						});
					}
				}
			};

			includeExcludeFilter?.Invoke(newFilter);

			Filters.Add(newFilter);
		}

		public void AddFilter<TEntity, TId>(
			IUnitOfWork unitOfWork,
			IGenericRepository<TEntity> repository,
			Func<TEntity, TId> parentIdSelector,
			Func<TEntity, TId> idSelector,
			Action<IncludeExcludeEntityWithHierarchyFilter<TEntity>> includeExcludeFilter = null)
			where TEntity : class, IDomainObject
		{
			var title = typeof(TEntity).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			var newFilter = new IncludeExcludeEntityWithHierarchyFilter<TEntity>
			{
				Title = title,
				Type = typeof(TEntity)
			};

			newFilter.RefreshFunc = (IncludeExcludeEntityWithHierarchyFilter<TEntity> filter, TEntity parent) =>
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

				if(isNamed)
				{
					Expression<Func<TEntity, bool>> isArchiveSpec = entity => string.IsNullOrWhiteSpace(CurrentSearchString) || ((INamed)entity).Name.ToLower().Like($"%{CurrentSearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isArchiveSpec);
				}

				if(isTitled)
				{
					Expression<Func<TEntity, bool>> isArchiveSpec = entity => string.IsNullOrWhiteSpace(CurrentSearchString) || ((ITitled)entity).Title.ToLower().Like($"%{CurrentSearchString.ToLower()}%");

					specificationExpression = specificationExpression.CombineWith(isArchiveSpec);
				}

				if(filter.Specification != null)
				{
					specificationExpression = specificationExpression.CombineWith(filter.Specification);
				}

				var entitiesToAdd = repository
					.Get(unitOfWork, specificationExpression)
					.ToList();

				LoadParents(unitOfWork, repository, entitiesToAdd, parentIdSelector);

				var elementsInTree = RebuildTree(entitiesToAdd, new GenericObservableList<IncludeExcludeElement>(), default, parentIdSelector, idSelector);

				filter.FilteredElements.Clear();
				FilteredElementsChanged?.Invoke(this, EventArgs.Empty);

				foreach(var element in elementsInTree)
				{
					filter.FilteredElements.Add(element);
				}
			};

			includeExcludeFilter?.Invoke(newFilter);

			Filters.Add(newFilter);
		}

		#endregion Добавление фильтров

		public TFilter GetFilter<TFilter>()
			where TFilter : IncludeExcludeFilter
		{
			var filter = Filters.FirstOrDefault(x => x.GetType() == typeof(TFilter));
			return (TFilter)filter;
		}

		private void ClearSearchString()
		{
			SearchString = string.Empty;
			CurrentSearchString = string.Empty;
		}

		private void ClearAllExcludes()
		{
			foreach(var filter in Filters)
			{
				filter.ClearExcludesCommand.Execute();
			}
		}

		private void ClearAllIncludes()
		{
			foreach(var filter in Filters)
			{
				filter.ClearIncludesCommand.Execute();
			}
		}

		private void ShowInfo()
		{
			_interactiveService.ShowMessage(ImportanceLevel.Info,
				"Кнопки \"Снять ✔️\" и \"Снять X\" снимают все галочки в соответствующих колонках.\n" +
				"Кнопки в левой части действуют на все категории.\n" +
				"Кнопки в правой части действуют на элементы выбранной категории.\n" +
				"Слева от категорий отображается счетчик выбранных в ней элементов.\n" +
				"При выборе хотя бы одной ✔️ в текущем фильтре - в выборку попадут только указанные значения.\n" +
				"При выборе X - из выборки будут исключены выбранные элементы.\n" +
				"При выборе галочки \"Показать архивные\" будут доступны для выбора архивные элементы." +
				"!Фильтр по статусу заказов сейчас работает только в отчете по рентабельности!", //Todo: убрать при релизе 4445
				"Справка по фильтру");
		}

		private void LoadParents<TEntity, TId>(
			IUnitOfWork unitOfWork,
			IGenericRepository<TEntity> repository,
			List<TEntity> entitiesToAdd,
			Func<TEntity, TId> parentIdSelector,
			int startIndex = 0)
			where TEntity : class, IDomainObject
		{
			var parentsIds = entitiesToAdd
				.Skip(startIndex)
				.Where(x => parentIdSelector(x) != null && !entitiesToAdd.Any(y => y.Id.ToString() == parentIdSelector(x).ToString()))
				.Select(x => parentIdSelector(x))
				.Distinct()
				.Cast<int>()
				.ToList();

			var elementsAtStartCount = entitiesToAdd.Count;

			entitiesToAdd.AddRange(repository.Get(unitOfWork, x => parentsIds.Contains(x.Id)));

			var elementsAtEndCount = entitiesToAdd.Count;

			if(elementsAtStartCount == elementsAtEndCount)
			{
				return;
			}

			LoadParents(unitOfWork, repository, entitiesToAdd, parentIdSelector, elementsAtStartCount);
		}

		private GenericObservableList<IncludeExcludeElement> RebuildTree<TId, TEntity>(
			List<TEntity> entitiesToAdd,
			GenericObservableList<IncludeExcludeElement> readyElements,
			TId id,
			Func<TEntity, TId> parentSelector,
			Func<TEntity, TId> idSelector)
			where TEntity : class, IDomainObject
		{
			var result = new GenericObservableList<IncludeExcludeElement>();

			for(int i = 0; i < entitiesToAdd.Count; i++)
			{
				if(parentSelector.Invoke(entitiesToAdd[i]).Equals(id))
				{
					var parentId = parentSelector.Invoke(entitiesToAdd[i]);

					var parent = readyElements
						.FirstOrDefault(x => x.Number == parentId.ToString());

					var element = new IncludeExcludeElement<TId, TEntity>
					{
						Id = idSelector.Invoke(entitiesToAdd[i]),
						Parent = parent,
						Title = entitiesToAdd[i].GetTitle(),
					};

					result.Add(element);

					foreach(var child in RebuildTree(entitiesToAdd, result, idSelector.Invoke(entitiesToAdd[i]), parentSelector, idSelector))
					{
						element.Children.Add(child);
					}
				}
			}

			return result;
		}

		private void UpdateFilteredElements()
		{
			if(ActiveFilter != null
				&& ActiveFilter is IncludeExcludeFilter filter)
			{
				filter.RefreshFilteredElements();
			}

			FilteredElementsChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}
