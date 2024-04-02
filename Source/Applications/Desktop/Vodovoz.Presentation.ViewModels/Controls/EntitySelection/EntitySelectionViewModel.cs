using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels.Control;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionViewModel<TEntity> : PropertyChangedBase, IEntitySelectionViewModel
		where TEntity : class, IDomainObject
	{
		private IPropertyBinder<TEntity> _entityBinder;
		private IEntityDialogSelectionAutocompleteSelector<TEntity> _dialogSelectionAndAutocompleteSelector;
		private IEntityJournalSelector _entityJournalSelector;
		private IEntitySelectionAdapter<TEntity> _entityAdapter;

		private bool _isEditable = true;

		private TEntity _entity;

		public EntitySelectionViewModel(
			IPropertyBinder<TEntity> binder = null,
			IEntityDialogSelectionAutocompleteSelector<TEntity> dialogSelectionAndAutocompleteSelector = null,
			IEntityJournalSelector entityJournalSelector = null,
			IEntitySelectionAdapter<TEntity> entityAdapter = null
			)
		{
			if(binder != null)
			{
				EntityBinder = binder;
			}

			if(dialogSelectionAndAutocompleteSelector != null)
			{
				DialogSelectionAndAutocompleteSelector = dialogSelectionAndAutocompleteSelector;
			}

			if(entityJournalSelector != null)
			{
				EntityJournalSelector = entityJournalSelector;
			}

			if(entityAdapter != null)
			{
				EntityAdapter = entityAdapter;
			}

			SelectEntityCommand = new DelegateCommand(SelectEntityFromSelectionDialog, () => CanSelectEntityFromDialog);
			SelectEntityFromJournalCommand = new DelegateCommand(SelectEntityFromJournal, () => CanSelectEntityFromJournal);
			ClearEntityCommand = new DelegateCommand(ClearEntity, () => CanClearEntity);
		}

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;
		public event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;

		public DelegateCommand SelectEntityCommand { get; }
		public DelegateCommand SelectEntityFromJournalCommand { get; }
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
					notifyPropertyNewEntity.PropertyChanged += OnEntityPropertyChanged; ;
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

		public IPropertyBinder<TEntity> EntityBinder
		{
			get => _entityBinder;
			set
			{
				if(_entityBinder == value)
				{
					return;
				}

				UnsubscribeBinder();

				_entityBinder = value;

				if(EntityBinder != null)
				{
					Entity = _entityBinder.PropertyValue;

					_entityBinder.Changed += OnEntityBinderChanged;
				}
			}
		}

		public IEntityDialogSelectionAutocompleteSelector<TEntity> DialogSelectionAndAutocompleteSelector
		{
			get => _dialogSelectionAndAutocompleteSelector;
			set
			{
				if(_dialogSelectionAndAutocompleteSelector == value)
				{
					return;
				}

				UnsubscribeDialogSelectionAndAutocompleteSelector();

				_dialogSelectionAndAutocompleteSelector = value;

				if(_dialogSelectionAndAutocompleteSelector != null)
				{
					_dialogSelectionAndAutocompleteSelector.AutocompleteLoaded += AutocompleteSelector_AutocompleteLoaded;
					_dialogSelectionAndAutocompleteSelector.EntitySelected += OnSelectionDialogEntitySelected;
					_dialogSelectionAndAutocompleteSelector.SelectEntityFromJournalSelected += OnSelectionDialogSelectEntityFromJournalSelected;
				}

				OnPropertyChanged(nameof(CanSelectEntity));
				OnPropertyChanged(nameof(CanSelectEntityFromDialog));
			}
		}

		public IEntitySelectionAdapter<TEntity> EntityAdapter
		{
			get => _entityAdapter;
			set
			{
				if(_entityAdapter == value)
				{
					return;
				}

				_entityAdapter = value;

				if(_entityAdapter != null)
				{
					_entityAdapter.EntitySelectionViewModel = this;
				}

				OnPropertyChanged(nameof(CanSelectEntity));
				OnPropertyChanged(nameof(CanSelectEntityFromDialog));
				OnPropertyChanged(nameof(CanSelectEntityFromJournal));
				OnPropertyChanged(nameof(CanClearEntity));
			}
		}

		public IEntityJournalSelector EntityJournalSelector
		{
			get => _entityJournalSelector;
			set
			{
				if(_entityJournalSelector == value)
				{
					return;
				}

				_entityJournalSelector = value;

				UnsubscribeJournalEntitySelector();

				if(_entityJournalSelector != null)
				{
					EntityJournalSelector.EntitySelected += OnEntityJournalSelectorEntitySelected;
				}

				OnPropertyChanged(nameof(CanSelectEntity));
				OnPropertyChanged(nameof(CanSelectEntityFromJournal));
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
				OnPropertyChanged(nameof(CanSelectEntityFromDialog));
				OnPropertyChanged(nameof(CanSelectEntityFromJournal));
				OnPropertyChanged(nameof(CanClearEntity));
			}
		}

		public bool DisposeViewModel { get; set; } = true;
		public string EntityTitle => Entity?.GetTitle();

		public virtual bool CanSelectEntity => CanSelectEntityFromDialog || CanSelectEntityFromJournal;
		public virtual bool CanSelectEntityFromDialog => IsEditable && DialogSelectionAndAutocompleteSelector != null;
		public virtual bool CanSelectEntityFromJournal =>
			IsEditable && EntityJournalSelector != null;
		public virtual bool CanClearEntity => IsEditable && Entity != null;

		public void SelectEntity(object entity)
		{
			Entity = EntityAdapter?.GetEntityByNode(entity);
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		#region AutoCompletion

		public int AutocompleteListSize { get; set; }

		public void AutocompleteTextEdited(string searchText)
		{
			var words = searchText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			DialogSelectionAndAutocompleteSelector?.LoadAutocompletion(words, AutocompleteListSize);
		}

		void AutocompleteSelector_AutocompleteLoaded(object sender, AutocompleteUpdatedEventArgs e)
		{
			AutoCompleteListUpdated?.Invoke(this, e);
		}

		public string GetAutocompleteTitle(object node)
		{
			return DialogSelectionAndAutocompleteSelector.GetTitle(node);
		}

		public void AutocompleteSelectNode(object node)
		{
			SelectEntity(node);
		}

		#endregion

		private void OnSelectionDialogEntitySelected(object sender, EntitySelectedEventArgs e)
		{
			SelectEntity(e.SelectedObject);
		}

		private void OnSelectionDialogSelectEntityFromJournalSelected(object sender, EventArgs e)
		{
			SelectEntityFromJournalCommand.Execute();
		}

		private void OnEntityJournalSelectorEntitySelected(object sender, EntitySelectedEventArgs e)
		{
			SelectEntity(e.SelectedObject);
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EntityTitle));
		}

		#region Команды View

		private void ClearEntity()
		{
			Entity = null;
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		private void SelectEntityFromSelectionDialog()
		{
			DialogSelectionAndAutocompleteSelector?.OpenSelector();
		}

		private void SelectEntityFromJournal()
		{
			EntityJournalSelector?.OpenSelector();
		}

		#endregion

		#region Entity binding

		private void OnEntityBinderChanged(object sender, EventArgs e)
		{
			Entity = _entityBinder.PropertyValue;
		}

		#endregion

		public void Dispose()
		{
			UnsubscribeAll();

			if(EntityBinder is IDisposable entityBinder)
			{
				entityBinder.Dispose();
			}

			if(DialogSelectionAndAutocompleteSelector is IDisposable dialogSelectionAndAutocompleteSelector)
			{
				dialogSelectionAndAutocompleteSelector.Dispose();
			}

			if(EntityJournalSelector is IDisposable entityJournalSelector)
			{
				entityJournalSelector.Dispose();
			}

			if(EntityAdapter is IDisposable entityAdapter)
			{
				entityAdapter.Dispose();
			}

			_entityBinder = null;
			_dialogSelectionAndAutocompleteSelector = null;
			_entityJournalSelector = null;
			_entityAdapter = null;
		}

		private void UnsubscribeAll()
		{
			UnsubscribeEntity();
			UnsubscribeBinder();
			UnsubscribeJournalEntitySelector();
			UnsubscribeDialogSelectionAndAutocompleteSelector();
		}

		private void UnsubscribeBinder()
		{
			if(EntityBinder != null)
			{
				EntityBinder.Changed -= OnEntityBinderChanged;
			}
		}

		private void UnsubscribeEntity()
		{
			if(_entity is INotifyPropertyChanged notifyPropertyOldEntity)
			{
				notifyPropertyOldEntity.PropertyChanged -= OnEntityPropertyChanged;
			}
		}

		private void UnsubscribeDialogSelectionAndAutocompleteSelector()
		{
			if(_dialogSelectionAndAutocompleteSelector != null)
			{
				_dialogSelectionAndAutocompleteSelector.EntitySelected -= OnSelectionDialogEntitySelected;
				_dialogSelectionAndAutocompleteSelector.SelectEntityFromJournalSelected -= OnSelectionDialogSelectEntityFromJournalSelected;
				_dialogSelectionAndAutocompleteSelector.AutocompleteLoaded -= AutocompleteSelector_AutocompleteLoaded;
			}
		}

		private void UnsubscribeJournalEntitySelector()
		{
			if(_entityJournalSelector != null)
			{
				_entityJournalSelector.EntitySelected += OnEntityJournalSelectorEntitySelected;
			}
		}
	}
}
