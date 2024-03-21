using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionViewModel<TEntity> : PropertyChangedBase, IEntitySelectionViewModel
		where TEntity : class, IDomainObject
	{
		private TEntity _entity;
		private IPropertyBinder<TEntity> _entityBinder;
		private ISelectionDialogEntitiesLoader<TEntity> _selectionDialogEntitiesLoader;
		private IEntitySelectionAutocompleteSelector<TEntity> _autocompleteSelector;
		private IEntityJournalSelector _entityJournalSelector;
		private IEntitySelectionAdapter<TEntity> _entityAdapter;

		private bool _isEditable = true;
		private bool _isUserHasAccessToOpenJournal;
		private readonly Func<SelectionDialogSettings> _selectionDialogSettingsFunc;

		public EntitySelectionViewModel(
			IPropertyBinder<TEntity> binder = null,
			ISelectionDialogEntitiesLoader<TEntity> selectionDialogEntitiesLoader = null,
			IEntitySelectionAutocompleteSelector<TEntity> autocompleteSelector = null,
			IEntityJournalSelector entityJournalSelector = null,
			IEntitySelectionAdapter<TEntity> entityAdapter = null,
			Func<SelectionDialogSettings> selectionDialogSettingsFunc = null
			)
		{
			if(binder != null)
			{
				EntityBinder = binder;
			}

			if(selectionDialogEntitiesLoader != null)
			{
				SelectionDialogEntitiesLoader = selectionDialogEntitiesLoader;
			}

			if(autocompleteSelector != null)
			{
				AutocompleteSelector = autocompleteSelector;
			}

			if(entityJournalSelector != null)
			{
				EntityJournalSelector = entityJournalSelector;
			}

			if(entityAdapter != null)
			{
				EntityAdapter = entityAdapter;
			}

			_selectionDialogSettingsFunc = selectionDialogSettingsFunc;

			OpenEntityJournalCommand = new DelegateCommand(OpenEntityJournal, () => CanSelectEntityFromJournal);
			ClearEntityCommand = new DelegateCommand(ClearEntity, () => CanClearEntity);
		}

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;
		public event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;

		public DelegateCommand OpenEntityJournalCommand { get; }
		public DelegateCommand ClearEntityCommand { get; }

		public virtual TEntity Entity
		{
			get { return _entity; }
			set
			{
				if(_entity == value)
				{
					return;
				}

				UnsubscribeEntity();

				_entity = value;

				if(_entity is INotifyPropertyChanged notifyPropertyNewEntity)
				{
					notifyPropertyNewEntity.PropertyChanged += Entity_PropertyChanged; ;
				}

				if(EntityBinder != null)
				{
					EntityBinder.PropertyValue = value;
				}

				OnPropertyChanged();
				OnPropertyChanged(nameof(EntityTitle));
				OnPropertyChanged(nameof(CanClearEntity));
				Changed?.Invoke(this, EventArgs.Empty);
			}
		}

		object IEntitySelectionViewModel.Entity { get => Entity; set => Entity = (TEntity)value; }
		public IEnumerable<object> AvailableEntities => SelectionDialogEntitiesLoader?.GetEntities()?.Cast<object>() ?? new List<object>();

		public IEntitySelectionAdapter<TEntity> EntityAdapter
		{
			get => _entityAdapter;
			set
			{
				_entityAdapter = value;
				_entityAdapter.EntitySelectionViewModel = this;
			}
		}

		public ISelectionDialogEntitiesLoader<TEntity> SelectionDialogEntitiesLoader
		{
			get => _selectionDialogEntitiesLoader;
			set
			{
				_selectionDialogEntitiesLoader = value;
			}
		}

		public IEntityJournalSelector EntityJournalSelector
		{
			get => _entityJournalSelector;
			set
			{
				_entityJournalSelector = value;
				EntityJournalSelector.EntitySelected += EntitySelector_EntitySelected;
				OnPropertyChanged(nameof(CanSelectEntity));
			}
		}

		public bool IsEditable
		{
			get { return _isEditable; }
			set
			{
				if(_isEditable == value)
				{
					return;
				}

				_isEditable = value;

				OnPropertyChanged(nameof(CanSelectEntity));
				OnPropertyChanged(nameof(CanClearEntity));
				OnPropertyChanged(nameof(CanAutoCompleteEntry));
				OnPropertyChanged(nameof(CanSelectEntityFromJournal));
			}
		}

		public bool IsUserHasAccessToOpenJournal
		{
			get => _isUserHasAccessToOpenJournal;
			set
			{
				if(_isUserHasAccessToOpenJournal == value)
				{
					return;
				}

				_isUserHasAccessToOpenJournal = value;

				OnPropertyChanged(nameof(CanSelectEntityFromJournal));
			}
		}

		public bool DisposeViewModel { get; set; } = true;
		public string EntityTitle => Entity?.GetTitle();
		public SelectionDialogSettings SelectionDialogSettings => _selectionDialogSettingsFunc?.Invoke() ?? new SelectionDialogSettings();

		public virtual bool CanSelectEntity => CanAutoCompleteEntry || CanSelectEntityFromJournal;
		public virtual bool CanClearEntity => IsEditable && Entity != null;
		public virtual bool CanAutoCompleteEntry => IsEditable && AutocompleteSelector != null;
		public virtual bool CanSelectEntityFromJournal => IsEditable && IsUserHasAccessToOpenJournal && EntityJournalSelector != null;

		#region AutoCompletion

		public int AutocompleteListSize { get; set; }

		public IEntitySelectionAutocompleteSelector<TEntity> AutocompleteSelector
		{
			get => _autocompleteSelector;
			set
			{
				_autocompleteSelector = value;
				OnPropertyChanged(nameof(CanAutoCompleteEntry));
				_autocompleteSelector.AutocompleteLoaded += AutocompleteSelector_AutocompleteLoaded;
			}
		}

		public void AutocompleteTextEdited(string searchText)
		{
			var words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			AutocompleteSelector?.LoadAutocompletion(words, AutocompleteListSize);
		}

		void AutocompleteSelector_AutocompleteLoaded(object sender, AutocompleteUpdatedEventArgs e)
		{
			AutoCompleteListUpdated?.Invoke(this, e);
		}

		public string GetAutocompleteTitle(object node)
		{
			return AutocompleteSelector.GetTitle(node);
		}

		public void AutocompleteSelectNode(object node)
		{
			Entity = EntityAdapter?.GetEntityByNode(node);
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		private void EntitySelector_EntitySelected(object sender, EntitySelectedEventArgs e)
		{
			Entity = EntityAdapter?.GetEntityByNode(e.Entity);
			ChangedByUser?.Invoke(this, e);
		}

		private void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EntityTitle));
		}

		#region Команды View

		private void ClearEntity()
		{
			Entity = null;
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		private void OpenEntityJournal()
		{
			_entityJournalSelector?.OpenSelector();
		}

		#endregion

		#region Entity binding

		public IPropertyBinder<TEntity> EntityBinder
		{
			get => _entityBinder;
			set
			{
				UnsubscribeBinder();
				_entityBinder = value;
				if(EntityBinder != null)
				{
					Entity = _entityBinder.PropertyValue;
					_entityBinder.Changed += EntityBinder_Changed;
				}
			}
		}

		private void EntityBinder_Changed(object sender, EventArgs e)
		{
			Entity = _entityBinder.PropertyValue;
		}

		#endregion

		public void Dispose()
		{
			UnsubscribeAll();

			if(EntityJournalSelector is IDisposable esd)
			{
				esd.Dispose();
			}

			if(EntityBinder is IDisposable ebd)
			{
				ebd.Dispose();
			}

			if(EntityAdapter is IDisposable ead)
			{
				ead.Dispose();
			}

			if(AutocompleteSelector is IDisposable asd)
			{
				asd.Dispose();
			}

			_entityJournalSelector = null;
			_entityBinder = null;
			_entityAdapter = null;
		}

		private void UnsubscribeAll()
		{
			UnsubscribeEntity();
			UnsubscribeBinder();
			UnsubscribeEntitySelector();
			UnsubscribeAutoCompleteSelector();
		}

		private void UnsubscribeBinder()
		{
			if(EntityBinder != null)
			{
				EntityBinder.Changed -= EntityBinder_Changed;
			}
		}

		private void UnsubscribeEntity()
		{
			if(_entity is INotifyPropertyChanged notifyPropertyOldEntity)
			{
				notifyPropertyOldEntity.PropertyChanged -= Entity_PropertyChanged;
			}
		}

		private void UnsubscribeEntitySelector()
		{
			if(_entityJournalSelector != null)
			{
				_entityJournalSelector.EntitySelected += EntitySelector_EntitySelected;
			}
		}

		private void UnsubscribeAutoCompleteSelector()
		{
			if(_autocompleteSelector != null)
			{
				_autocompleteSelector.AutocompleteLoaded -= AutocompleteSelector_AutocompleteLoaded;
			}
		}
	}

	public class BeforeChangeEventArgs : EventArgs
	{
		public bool CanChange { get; set; }
	}
}
