using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using System.Data.Bindings.Utilities;

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
		/// Нужен для NHibernate. Используйте <see cref="Parent"/>
		/// </summary>
		public virtual ProductGroup MappedParent {
			get => parent;
			set { SetField(ref parent, value, () => MappedParent); }
		}

		private ProductGroup parent;
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

		private bool exportToOnlineStore;

		[Display(Name = "Выгружать товары в онлайн магазин")]
		public virtual bool ExportToOnlineStore {
			get { return exportToOnlineStore; }
			set { SetField(ref exportToOnlineStore, value, () => ExportToOnlineStore); }
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
