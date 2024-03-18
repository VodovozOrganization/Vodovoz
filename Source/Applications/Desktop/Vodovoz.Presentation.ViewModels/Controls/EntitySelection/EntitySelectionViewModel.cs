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
		private TEntity _entity;
		private IEntitySelectionAdapter<TEntity> _entityAdapter;
		private IEntityJournalSelector _entitySelector;
		private IPropertyBinder<TEntity> _entityBinder;
		private IEntitySelectionAutocompleteSelector<TEntity> _autocompleteSelector;

		private bool _isEditable = true;

		public EntitySelectionViewModel(
			IPropertyBinder<TEntity> binder = null,
			IEntityJournalSelector entitySelector = null,
			IEntitySelectionAdapter<TEntity> entityAdapter = null,
			IEntitySelectionAutocompleteSelector<TEntity> autocompleteSelector = null
			)
		{
			if(binder != null)
			{
				EntityBinder = binder;
			}

			if(entitySelector != null)
			{
				EntitySelector = entitySelector;
			}

			if(entityAdapter != null)
			{
				EntityAdapter = entityAdapter;
			}
			if(autocompleteSelector != null)
			{
				AutocompleteSelector = autocompleteSelector;
			}

			OpenSelectDialogCommand = new DelegateCommand(OpenSelectDialog, () => CanSelectEntity);
			ClearEntityCommand = new DelegateCommand(ClearEntity, () => CanClearEntity);
		}

		#region События

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;
		public event EventHandler<AutocompleteUpdatedEventArgs> AutoCompleteListUpdated;

		#endregion

		#region Свойства

		public DelegateCommand OpenSelectDialogCommand { get; }
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

		public IEntitySelectionAdapter<TEntity> EntityAdapter
		{
			get => _entityAdapter;
			set
			{
				_entityAdapter = value;
				_entityAdapter.EntitySelectionViewModel = this;
			}
		}

		#region AutoCompletion

		public int AutocompleteListSize { get; set; }

		public EntitySelectionAutocompleteSelector<TEntity> AutocompleteSelector
		{
			get => _autocompleteSelector;
			set
			{
				_autocompleteSelector = value;
				OnPropertyChanged(nameof(SensitiveAutoCompleteEntry));
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
				OnPropertyChanged(nameof(SensitiveAutoCompleteEntry));
			}
		}

		public bool DisposeViewModel { get; set; } = true;
		public string EntityTitle => Entity?.GetTitle();

		public virtual bool CanSelectEntity => IsEditable && EntitySelector != null;
		public virtual bool CanClearEntity => IsEditable && Entity != null;
		public virtual bool SensitiveAutoCompleteEntry => IsEditable && AutocompleteSelector != null;

		public bool CanViewEntity { get; set; } = true;

		public IEntityJournalSelector EntitySelector
		{
			get => _entitySelector;
			set
			{
				_entitySelector = value;
				EntitySelector.EntitySelected += EntitySelector_EntitySelected;
				OnPropertyChanged(nameof(CanSelectEntity));
			}
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
		private void OpenSelectDialog()
		{
			OpenEntityJournal();
		}

		private void ClearEntity()
		{
			Entity = null;
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		private void OpenEntityJournal()
		{
			_entitySelector?.OpenSelector();
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

			if(EntitySelector is IDisposable esd)
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

			_entitySelector = null;
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
			if(_entitySelector != null)
			{
				_entitySelector.EntitySelected += EntitySelector_EntitySelected;
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
