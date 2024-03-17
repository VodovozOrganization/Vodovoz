using QS.Commands;
using QS.DomainModel.Entity;
using QS.ViewModels.Control;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionViewModel<TEntity> : PropertyChangedBase, IEntitySelectionViewModel
		where TEntity : class, IDomainObject
	{
		private TEntity _entity;
		private EntitySelectionAdapter<TEntity> _entityAdapter;
		private IEntitySelector _entitySelector;
		private IPropertyBinder<TEntity> _entityBinder;

		private bool _isEditable = true;

		public EntitySelectionViewModel(
			IPropertyBinder<TEntity> binder = null,
			IEntitySelector entitySelector = null,
			EntitySelectionAdapter<TEntity> entityAdapter = null
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

			OpenSelectDialogCommand = new DelegateCommand(OpenSelectDialog, () => CanSelectEntity);
			ClearEntityCommand = new DelegateCommand(ClearEntity, () => CanClearEntity);
		}

		#region События

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;

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

		public EntitySelectionAdapter<TEntity> EntityAdapter
		{
			get => _entityAdapter;
			set
			{
				_entityAdapter = value;
				_entityAdapter.EntitySelectionViewModel = this;
			}
		}

		public bool IsEditable
		{
			get { return _isEditable; }
			set
			{
				_isEditable = value;
				OnPropertyChanged(nameof(CanSelectEntity));
				OnPropertyChanged(nameof(CanClearEntity));
			}
		}

		public bool DisposeViewModel { get; set; } = true;
		public string EntityTitle => Entity?.GetTitle();

		public virtual bool CanSelectEntity => IsEditable && EntitySelector != null;
		public virtual bool CanClearEntity => IsEditable && Entity != null;

		public bool CanViewEntity { get; set; } = true;

		public IEntitySelector EntitySelector
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

			_entitySelector = null;
			_entityBinder = null;
			_entityAdapter = null;
		}

		private void UnsubscribeAll()
		{
			UnsubscribeEntity();
			UnsubscribeBinder();
			UnsubscribeEntitySelector();
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
	}

	public class BeforeChangeEventArgs : EventArgs
	{
		public bool CanChange { get; set; }
	}
}
