using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Bindings.Utilities;
using System.Linq.Expressions;
using System.Reflection;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using QS.Utilities.Text;
using Vodovoz.ViewModelBased;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Infrastructure.Services;
using QSValidation;
using QS.Permissions;
using QS.Project.Domain;

namespace Vodovoz.Infrastructure.ViewModels
{
	public abstract class EntityTabViewModelBase<TEntity> : UoWTabViewModelBase, IEntityDialog<TEntity>, IEntityDialog
		where TEntity : class, IDomainObject, INotifyPropertyChanged, IValidatableObject, new()
	{
		#region IEntityDialog implementation

		public override IUnitOfWork UoW => UoWGeneric;

		public IUnitOfWorkGeneric<TEntity> UoWGeneric { get; set; }

		public TEntity Entity => UoWGeneric.Root;

		public object EntityObject => UoWGeneric.Root;

		#endregion

		private string tabName = string.Empty;
		public override string TabName {
			get {
				if(!string.IsNullOrWhiteSpace(tabName))
					return tabName;
				if(UoW != null && UoW.RootObject != null) {
					AppellativeAttribute subAtt = GetType().GetCustomAttribute<AppellativeAttribute>(true);

					if(UoW.IsNew) {
						if(subAtt != null && !string.IsNullOrWhiteSpace(subAtt.Nominative)) {
							switch(subAtt.Gender) {
								case GrammaticalGender.Masculine:
									return "Новый " + subAtt.Nominative;
								case GrammaticalGender.Feminine:
									return "Новая " + subAtt.Nominative;
								case GrammaticalGender.Neuter:
									return "Новое " + subAtt.Nominative;
								default:
									return "Новый(ая) " + subAtt.Nominative;
							}
						}
					} else {
						var notifySubject = UoW.RootObject as INotifyPropertyChanged;

						var prop = UoW.RootObject.GetType().GetProperty("Title");
						if(prop != null) {
							if(notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_TitlePropertyChanged;
								notifySubject.PropertyChanged += Subject_TitlePropertyChanged;
							}
							return prop.GetValue(UoW.RootObject, null).ToString();
						}

						prop = UoW.RootObject.GetType().GetProperty("Name");
						if(prop != null) {
							if(notifySubject != null) {
								notifySubject.PropertyChanged -= Subject_NamePropertyChanged;
								notifySubject.PropertyChanged += Subject_NamePropertyChanged;
							}
							return prop.GetValue(UoW.RootObject, null).ToString();
						}

						if(subAtt != null && !string.IsNullOrWhiteSpace(subAtt.Nominative))
							return subAtt.Nominative.StringToTitleCase();
					}
					return UoW.RootObject.ToString();
				}
				return string.Empty;
			}
			set {
				if(tabName == value)
					return;
				tabName = value;
				OnTabNameChanged();
			}
		}

		protected ValidationContext ValidationContext { get; set; }
		protected IUserService UserService { get; private set; }
		protected IValidator Validator { get; private set; }
		protected IPermissionResult PermissionResult { get; private set; }

		public UserBase CurrentUser { get; set; }

		protected EntityTabViewModelBase(IEntityConstructorParam ctorParam, ICommonServices commonServices)
			: base((commonServices ?? throw new ArgumentNullException(nameof(commonServices))).InteractiveService)
		{
			if(ctorParam == null) {
				throw new ArgumentNullException(nameof(ctorParam));
			}

			UserService = commonServices.UserService;

			if(ctorParam.IsNewEntity) {
				if(ctorParam.RootUoW == null) {
					UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<TEntity>();
				} else {
					UoWGeneric = UnitOfWorkFactory.CreateWithNewChildRoot<TEntity>(ctorParam.RootUoW);
				}
			} else {
				if(ctorParam.RootUoW == null) {
					UoWGeneric = UnitOfWorkFactory.CreateForRoot<TEntity>(ctorParam.EntityOpenId);
				} else {
					UoWGeneric = UnitOfWorkFactory.CreateForChildRoot<TEntity>(ctorParam.RootUoW.GetById<TEntity>(ctorParam.EntityOpenId), ctorParam.RootUoW);
				}
			}
			ValidationContext = new ValidationContext(Entity);
			Entity.PropertyChanged += Entity_PropertyChanged;
			CurrentUser = UserService.GetCurrentUser(UoW);
			Validator = commonServices.ValidationService.GetValidator(Entity, ValidationContext);
			PermissionResult = commonServices.PermissionService.ValidateUserPermission(typeof(TEntity), UserService.CurrentUserId);

			if(!PermissionResult.CanRead) {
				AbortOpening(PermissionsSettings.GetEntityReadValidateResult(typeof(TEntity)));
			}
		}

		protected virtual void BeforeSave()
		{
		}

		public override bool Save()
		{
			if(!Validate()) {
				return false;
			}
			BeforeSave();
			bool result = base.Save();
			AfterSave();
			return result;
		}

		public override void SaveAndClose()
		{
			if(!Validate()) {
				return;
			}
			BeforeSave();
			base.SaveAndClose();
			AfterSave();
		}

		protected virtual void AfterSave()
		{
		}

		protected bool Validate()
		{
			return Validator.Validate(ValidationContext);
		}

		void Subject_NamePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "Name")
				OnTabNameChanged();
		}

		void Subject_TitlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == "Title")
				OnTabNameChanged();
		}

		public override bool CompareHashName(string hashName)
		{
			if(Entity == null || UoWGeneric == null || UoWGeneric.IsNew) {
				return false;
			}
			return GenerateHashName(Entity.Id) == hashName;
		}

		public static string GenerateHashName(int id)
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), id);
		}

		public static string GenerateHashName(TEntity entity)
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), entity.Id);
		}

		public static string GenerateHashName()
		{
			return DomainHelper.GenerateDialogHashName(typeof(TEntity), 0);
		}

		protected override void OnTabNameChanged()
		{
			if(UoW?.ActionTitle != null) {
				UoWGeneric.ActionTitle.UserActionTitle = $"Диалог '{TabName}'";
			}

			base.OnTabNameChanged();
		}

		#region Подписки на изменения свойств

		private Dictionary<string, IList<string>> propertyTriggerRelations = new Dictionary<string, IList<string>>();
		private Dictionary<string, List<Action>> propertyOnChangeActions = new Dictionary<string, List<Action>>();
		private List<Action> onAnyPropertyChangedActions = new List<Action>();

		private void Entity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(propertyTriggerRelations.ContainsKey(e.PropertyName)) {
				foreach(var relatedPropertyName in propertyTriggerRelations[e.PropertyName]) {
					OnPropertyChanged(relatedPropertyName);
				}
			}

			if(propertyOnChangeActions.ContainsKey(e.PropertyName)) {
				foreach(var action in propertyOnChangeActions[e.PropertyName]) {
					action.Invoke();
				}
			}

			foreach(var onChangeAction in onAnyPropertyChangedActions) {
				onChangeAction.Invoke();
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

		protected void OnEntityPropertyChanged(Action onChangeAction, params Expression<Func<TEntity, object>>[] entityTriggeredProperty)
		{
			foreach(var property in entityTriggeredProperty) {
				string entityPropertyName = Entity.GetPropertyName(property);
				if(!propertyOnChangeActions.ContainsKey(entityPropertyName)) {
					propertyOnChangeActions.Add(entityPropertyName, new List<Action>());
				}
				propertyOnChangeActions[entityPropertyName].Add(onChangeAction);
			}
		}

		protected void OnEntityAnyPropertyChanged(Action onChangeAction)
		{
			onAnyPropertyChangedActions.Add(onChangeAction);
		}

		#endregion
	}
}
