using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Utilities;
using System.Linq.Expressions;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace Vodovoz.ViewModelBased
{
	public abstract class ViewModel<TEntity> : ViewModelBase
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

		protected ViewModel(IEntityOpenOption entityOpenOption)
		{
			if(entityOpenOption == null) {
				throw new ArgumentNullException(nameof(entityOpenOption));
			}

			if(entityOpenOption.NeedCreateNew) {
				UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<TEntity>();
			} else {
				UoWGeneric = UnitOfWorkFactory.CreateForRoot<TEntity>(entityOpenOption.EntityId);
			}

			Entity.PropertyChanged += Entity_PropertyChanged;
		}

		protected abstract void ConfigurePropertyChangingRelations();

		private Dictionary<string, IList<string>> propertyTriggerRelation;

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(propertyTriggerRelation == null) {
				ConfigurePropertyChangingRelations();
			}

			if(propertyTriggerRelation.ContainsKey(e.PropertyName)) {
				foreach(var relatedPropertyName in propertyTriggerRelation[e.PropertyName]) {
					OnPropertyChanged(relatedPropertyName);
				}
			}
		}

		/// <summary>
		/// Устанавливает зависимость свойства сущности к свойствам модели представления.
		/// Если произойдет изменение указанного свойства сущности, то вызовутся изменения всех связанных свойств модели представления
		/// </summary>
		protected void SetPropertyChangeRelation<T>(Expression<Func<TEntity, object>> entityTriggeredProperty, params Expression<Func<T>>[] vmChangingPropertiesExpressions)
		{
			if(propertyTriggerRelation == null) {
				propertyTriggerRelation = new Dictionary<string, IList<string>>();
			}

			IList<string> vmChangingProperties = new List<string>();
			string entityPropertyName = Entity.GetPropertyName(entityTriggeredProperty);
			if(propertyTriggerRelation.ContainsKey(entityPropertyName)) {
				vmChangingProperties = propertyTriggerRelation[entityPropertyName];
			} else {
				propertyTriggerRelation.Add(entityPropertyName, vmChangingProperties);
			}

			foreach(var cpe in vmChangingPropertiesExpressions) {
				string vmChangingPropertyName = GetPropertyName(cpe);
				if(!vmChangingProperties.Contains(vmChangingPropertyName)) {
					vmChangingProperties.Add(vmChangingPropertyName);
				}
			}
		}
	}
}
