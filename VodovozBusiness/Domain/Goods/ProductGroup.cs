using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.Collections.Generic;
using QS.DomainModel.Entity.EntityPermissions;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы товаров",
		Nominative = "группа товаров")]
	[EntityPermission]
	public class ProductGroup : DomainTreeNodeBase<ProductGroup>
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
			if(OnlineStoreGuid == null && ExportToOnlineStore)
			{
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
		}

		public ProductGroup()
		{
		}
	}
}
