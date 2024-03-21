using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class SelectionDialogSelector<TEntity> : ISelectionDialogSelector<TEntity>
		where TEntity : class, IDomainObject
	{
		private readonly INavigationManager _navigationManager;
		private readonly IUnitOfWork _uow;
		private readonly SelectionDialogSettings _dialogSettings;
		private readonly Func<IList<int>> _entityIdRestrictionFunc;

		private EntityButtonsSelectionViewModel _selectionDialog;

		public SelectionDialogSelector(
			INavigationManager navigationManager,
			IUnitOfWork uow,
			Func<IList<int>> entityIdRestrictionFunc = null,
			SelectionDialogSettings dialogSettings = null)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_entityIdRestrictionFunc = entityIdRestrictionFunc;
			_dialogSettings = dialogSettings ?? new SelectionDialogSettings();
		}

		public event EventHandler<EntitySelectedEventArgs> EntitySelected;
		public event EventHandler SelectEntityFromJournalSelected;

		public void OpenSelector()
		{
			_selectionDialog = _navigationManager.OpenViewModel<EntityButtonsSelectionViewModel, IList<object>, SelectionDialogSettings>(
				null,
				GetEntities(),
				_dialogSettings)
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

		private IQueryable<TEntity> GetQuery()
		{
			var query = _uow.Session.Query<TEntity>();

			if(_entityIdRestrictionFunc != null)
			{
				query = query.Where(e => _entityIdRestrictionFunc.Invoke().Contains(e.Id));
			}

			return query;
		}
	}
}
