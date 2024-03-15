using QS.DomainModel.Entity;
using QS.ViewModels.Control;
using QS.ViewModels.Control.EEVM;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class EntitySelectionViewModel<TEntity> : PropertyChangedBase
		where TEntity : class, IDomainObject
	{
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
		}

		#region События

		public event EventHandler Changed;
		public event EventHandler ChangedByUser;
		public event EventHandler<BeforeChangeEventArgs> BeforeChangeByUser;

		#endregion

		#region Работа с сущьностью

		private TEntity _entity;

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
				OnPropertyChanged(nameof(SensitiveCleanButton));
				Changed?.Invoke(this, EventArgs.Empty);
			}
		}

		//object IEntityEntryViewModel.Entity { get => Entity; set => Entity = (TEntity)value; }

		private EntitySelectionAdapter<TEntity> _entityAdapter;
		public EntitySelectionAdapter<TEntity> EntityAdapter
		{
			get => _entityAdapter;
			set
			{
				_entityAdapter = value;
				_entityAdapter.EntitySelectionViewModel = this;
			}
		}

		#endregion

		#region Публичные свойства

		bool _isEditable = true;

		public bool IsEditable
		{
			get { return _isEditable; }
			set
			{
				_isEditable = value;
				OnPropertyChanged(nameof(SensitiveSelectButton));
				OnPropertyChanged(nameof(SensitiveCleanButton));
			}
		}

		#endregion

		#region Свойства для использования во View

		public bool DisposeViewModel { get; set; } = true;
		public string EntityTitle => Entity?.GetTitle();

		public virtual bool SensitiveSelectButton => IsEditable && EntitySelector != null;
		public virtual bool SensitiveCleanButton => IsEditable && Entity != null;

		public bool CanViewEntity { get; set; } = true;
		#endregion

		#region Выбор сущьности основным способом

		private IEntitySelector _entitySelector;
		public IEntitySelector EntitySelector
		{
			get => _entitySelector;
			set
			{
				_entitySelector = value;
				EntitySelector.EntitySelected += EntitySelector_EntitySelected;
				OnPropertyChanged(nameof(SensitiveSelectButton));
			}
		}

		/// <summary>
		/// Открывает диалог выбора сущности
		/// </summary>
		public void OpenSelectDialog()
		{
			if(!OnBeforeUserChanged())
			{
				return;
			}
			_entitySelector?.OpenSelector();
		}

		void EntitySelector_EntitySelected(object sender, EntitySelectedEventArgs e)
		{
			Entity = EntityAdapter?.GetEntityByNode(e.Entity);
			ChangedByUser?.Invoke(this, e);
		}

		#endregion

		#region Обработка событий

		void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(EntityTitle));
		}

		bool OnBeforeUserChanged()
		{
			if(BeforeChangeByUser == null)
			{
				return true;
			}

			var args = new BeforeChangeEventArgs
			{
				CanChange = true
			};

			BeforeChangeByUser(this, args);

			return args.CanChange;
		}

		#endregion

		#region Команды View

		public void CleanEntity()
		{
			Entity = null;
			ChangedByUser?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Entity binding

		IPropertyBinder<TEntity> _entityBinder;

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

		void EntityBinder_Changed(object sender, EventArgs e)
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
