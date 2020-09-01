using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Utilities;
using Gamma.ColumnConfig;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы товаров",
		Nominative = "группа товаров")]
	[EntityPermission]
	[HistoryTrace]
	public class ProductGroup : DomainTreeNodeBase<ProductGroup>, IValidatableObject
	{
		#region Свойства

		string name;
		[Display(Name = "Название")]
		[StringLength(100)]
		[Required(ErrorMessage = "Название должно быть заполнено.")]
		public virtual string Name {
			get { return name; }
			set { SetField(ref name, value, () => Name); }
		}

		/// <summary>
		/// Нужен для NHibernate.
		/// </summary>
		public virtual ProductGroup MappedParent {
			get => parent;
			set { SetField(ref parent, value, () => MappedParent); }
		}

		private ProductGroup parent;
		/// <summary>
		/// Для Nhibernate используйте <see cref="MappedParent"/>
		/// </summary>
		public override ProductGroup Parent {
			get => parent;
			set {
				if(parent != null)
					parent.Childs.Remove(this);

				MappedParent = value;

				if(parent != null)
					parent.Childs.Add(this);
			}
		}

		private Guid? onlineStoreGuid;
		[Display(Name = "Guid интернет магазина")]
		public virtual Guid? OnlineStoreGuid {
			get { return onlineStoreGuid; }
			set { SetField(ref onlineStoreGuid, value, () => OnlineStoreGuid); }
		}
		
		private string onlineStoreExternalId;
		[Display(Name = "Id в интернет магазине")]
		public virtual string OnlineStoreExternalId {
			get => onlineStoreExternalId;
			set => SetField(ref onlineStoreExternalId, value);
		}

		private bool exportToOnlineStore;
		[Display(Name = "Выгружать товары в онлайн магазин?")]
		public virtual bool ExportToOnlineStore {
			get { return exportToOnlineStore; }
			set { SetField(ref exportToOnlineStore, value, () => ExportToOnlineStore); }
		}

		private bool isOnlineStore;
		/// <summary>
		/// Для Nhibernate используйте <see cref="MappedIsOnlineStore"/>
		/// </summary>
		[Display(Name = "Группа товаров интернет магазина?")]
		public virtual bool IsOnlineStore {
			get => isOnlineStore;
			set {
				MappedIsOnlineStore = value;
				if(Childs != null) {
					foreach(var item in Childs) {
						item.IsOnlineStore = value;
					}
				}
			}
		}
		
		private bool isArchive;
		/// <summary>
		/// Для Nhibernate используйте <see cref="MappedIsArchive"/>
		/// </summary>
		[Display(Name = "Группа архивирована")]
		public virtual bool IsArchive {
			get => isArchive;
			set {
				MappedIsArchive = value;
				if(Childs != null) {
					SetChildren(this, value);
				}
			}
		}

		public virtual void SetArchive(bool _isArchive)
		{
			isArchive = _isArchive;
			OnPropertyChanged(nameof(IsArchive));
		}

		void SetChildren(ProductGroup productGroup, bool _isArchive)
		{
			productGroup.SetArchive(_isArchive);
			if (productGroup.Childs != null)
			{
				foreach(var n in productGroup.Childs) {
					SetChildren(n, _isArchive);
				}
			}
		}
		 
		
		/// <summary>
		/// Нужен для NHibernate.
		/// </summary>
		public virtual bool MappedIsArchive {
			get { return isArchive; }
			set { SetField(ref isArchive, value, () => MappedIsArchive); }
		}
		

		/// <summary>
		/// Нужен для NHibernate.
		/// </summary>
		public virtual bool MappedIsOnlineStore {
			get { return isOnlineStore; }
			set { SetField(ref isOnlineStore, value, () => MappedIsOnlineStore); }
		}
		
		private OnlineStore onlineStore;
		[Display(Name = "Интернет-магазин")]
		public virtual OnlineStore OnlineStore {
			get => onlineStore;
			set => SetField(ref onlineStore, value);
		}

		[Display(Name = "Характеристики товаров")]
		public virtual string CharacteristicsText {
			get { return String.Join(",", characteristics); }
			set {
				characteristics.Clear();
				if(string.IsNullOrWhiteSpace(value)) {
					return;
				}
				var parts = value.Split(',');
				foreach(var characteristic in parts) {
					NomenclatureProperties property;
					if(Enum.TryParse<NomenclatureProperties>(characteristic, out property))
						characteristics.Add(property);
					else
						logger.Error($"Характеристика товара {characteristic}, не была найдена в перечислении {typeof(NomenclatureProperties).Name}, поэтому была отброшена.");
				}
			}
		}

		private List<NomenclatureProperties> characteristics = new List<NomenclatureProperties>();
		[Display(Name = "Характеристики товаров")]
		public virtual List<NomenclatureProperties> Characteristics {
			get { return characteristics; }
			set { SetField(ref characteristics, value); }
		}

		#endregion

		/// <summary>
		/// Cоздает новый Guid. Uow необходим для сохранения созданного Guid в базу.
		/// </summary>
		public virtual void CreateGuidIfNotExist(IUnitOfWork uow)
		{
			if(OnlineStoreGuid == null && ExportToOnlineStore) {
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
		}

		private bool IsValidParent(ProductGroup parentGroup)
		{
			if(parentGroup == null)
				return true;

			if(this == parentGroup)
				return false;
				
			return IsValidParent(parentGroup.parent);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(!IsValidParent(parent))
				yield return new ValidationResult(
					"\"Родитель не назначен, так как возникает зацикливание\"",
					new[] { this.GetPropertyName(o => o.Parent) }
				);
		}

		public ProductGroup() { }
	}
}
