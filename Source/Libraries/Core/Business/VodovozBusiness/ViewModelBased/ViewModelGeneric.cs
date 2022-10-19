using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Utilities;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Domain;

namespace Vodovoz.ViewModelBased
{
	public abstract class ViewModel<TEntity> : TabViewModelBase
		where TEntity : class, INotifyPropertyChanged, IDomainObject, new()
	{
		private IUnitOfWorkGeneric<TEntity> uoWGeneric;
		protected IUnitOfWorkGeneric<TEntity> UoWGeneric {
			get => uoWGeneric;
			private set {
				uoWGeneric = value;
				UoW = uoWGeneric;
			}
		}

		public IViewModelBasedDialog<ViewModel<TEntity>, TEntity> View { get; set; }

		public TEntity Entity => UoWGeneric?.Root;

		protected ViewModel(IEntityUoWBuilder entityUoWBuilder, IUnitOfWorkFactory unitOfWorkFactory)
		{
			if(entityUoWBuilder == null) {
				throw new ArgumentNullException(nameof(entityUoWBuilder));
			}
			UoWGeneric = entityUoWBuilder.CreateUoW<TEntity>(unitOfWorkFactory);
			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		private Dictionary<string, IList<string>> propertyTriggerRelations = new Dictionary<string, IList<string>>();
		private Dictionary<string, Action> propertyOnChangeActions = new Dictionary<string, Action>();

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(propertyTriggerRelations.ContainsKey(e.PropertyName)) {
				foreach(var relatedPropertyName in propertyTriggerRelations[e.PropertyName]) {
					OnPropertyChanged(relatedPropertyName);
				}
			}

			if(propertyOnChangeActions.ContainsKey(e.PropertyName)) {
				propertyOnChangeActions[e.PropertyName].Invoke();
			}
		}

		/// <summary>
		/// Устанавливает зависимость свойства сущности к свойствам модели представления.
		/// Если произойдет изменение указанного свойства сущности, то вызовутся изменения всех связанных свойств модели представления
		/// </summary>
		protected void SetPropertyChangeRelation<T>(Expression<Func<TEntity, object>> entityTriggeredProperty, params Expression<Func<T>>[] vmChangingPropertiesExpressions)
		{
			IList<string> vmChangingProperties = new List<string>();
			string entityPropertyName = Entity.GetPropertyName(entityTriggeredProperty);
			if(propertyTriggerRelations.ContainsKey(entityPropertyName)) {
				vmChangingProperties = propertyTriggerRelations[entityPropertyName];
			} else {
				propertyTriggerRelations.Add(entityPropertyName, vmChangingProperties);
			}

			foreach(var cpe in vmChangingPropertiesExpressions) {
				string vmChangingPropertyName = GetPropertyName(cpe);
				if(!vmChangingProperties.Contains(vmChangingPropertyName)) {
					vmChangingProperties.Add(vmChangingPropertyName);
				}
			}
		}

		protected void OnEntityPropertyChanged(Expression<Func<TEntity, object>> entityTriggeredProperty, Action onChangeAction)
		{
			string entityPropertyName = Entity.GetPropertyName(entityTriggeredProperty);
			if(!propertyOnChangeActions.ContainsKey(entityPropertyName)) {
				propertyOnChangeActions.Add(entityPropertyName, onChangeAction);
			}
		}


	}
}
