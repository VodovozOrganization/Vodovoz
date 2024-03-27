using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntityDialogSelectionAutocompleteSelector<TEntity> : IEntityDialogSelectionAutocompleteSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		private readonly INavigationManager _navigationManager;
		private readonly IUnitOfWork _uow;
		private readonly Func<IList<int>> _entityIdRestrictionFunc;
		private readonly Func<string, Expression<Func<TEntity, bool>>> _entityTitleComparerFunc;
		private readonly Func<IEnumerable<TEntity>, IEnumerable<TEntity>> _resultCollectionProcessingFunc;
		private readonly Func<SelectionDialogSettings> _dialogSettingsFunc;
		private EntityButtonsSelectionViewModel _selectionDialog;

		public EntityDialogSelectionAutocompleteSelector(
			INavigationManager navigationManager,
			IUnitOfWork uow,
			Func<IList<int>> entityIdRestrictionFunc = null,
			Func<string, Expression<Func<TEntity, bool>>> entityTitleComparerFunc = null,
			Func<IEnumerable<TEntity>, IEnumerable<TEntity>> resultCollectionProcessingFunc = null,
			Func<SelectionDialogSettings> dialogSettingsFunc = null)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_entityIdRestrictionFunc = entityIdRestrictionFunc;
			_entityTitleComparerFunc = entityTitleComparerFunc;
			_resultCollectionProcessingFunc = resultCollectionProcessingFunc;
			_dialogSettingsFunc = dialogSettingsFunc;
		}

		public event EventHandler<AutocompleteUpdatedEventArgs> AutocompleteLoaded;

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public string GetTitle(object node) => node.GetTitle();

		public void LoadAutocompletion(string[] searchText, int takeCount)
		{
			var items = GetQuery(searchText, takeCount).ToList();

			AutocompleteLoaded?.Invoke(this, new AutocompleteUpdatedEventArgs(items));
		}

		public void OpenSelector()
		{
			_selectionDialog = _navigationManager.OpenViewModel<EntityButtonsSelectionViewModel, IList<object>, Func<SelectionDialogSettings>>(
				null,
				GetEntities(),
				_dialogSettingsFunc)
				.ViewModel;

			_selectionDialog.EntitySelected += OnEntitySelected;
			_selectionDialog.SelectEntityFromJournalSelected += OnSelectEntityFromJournalSelected;
		}

		private void OnEntitySelected(object sender, EntitySelectedEventArgs e)
		{
			if(_selectionDialog != null)
			{
				_selectionDialog.EntitySelected -= OnEntitySelected;
			}

			EntitySelected?.Invoke(this, e);
		}

		private void OnSelectEntityFromJournalSelected(object sender, EventArgs e)
		{
			if(_selectionDialog != null)
			{
				_selectionDialog.SelectEntityFromJournalSelected -= OnSelectEntityFromJournalSelected;
			}

			SelectEntityFromJournalSelected?.Invoke(this, e);
		}

		private IList<object> GetEntities()
		{
			return GetQuery().Cast<object>().ToList();
		}

		private IEnumerable<TEntity> GetQuery(string[] searchText = null, int? entitiesMaxCount = null)
		{
			var query = _uow.Session.Query<TEntity>();

			if(_entityIdRestrictionFunc != null)
			{
				query = query.Where(e => _entityIdRestrictionFunc.Invoke().Contains(e.Id));
			}

			if(searchText != null && _entityTitleComparerFunc != null)
			{
				foreach(var text in searchText)
				{
					query = query.Where(_entityTitleComparerFunc(text));
				}
			}

			if(entitiesMaxCount != null)
			{
				query = query.Take(entitiesMaxCount.Value);
			}

			if(_resultCollectionProcessingFunc != null)
			{
				return _resultCollectionProcessingFunc.Invoke(query.ToList());
			}

			return query.ToList();
		}
	}
}
