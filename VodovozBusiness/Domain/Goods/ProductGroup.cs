using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Basis;
using QSOrmProject;

namespace Vodovoz.Domain.Goods
{
	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "группы товаров",
		Nominative = "группа товаров")]
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

		#endregion

		/// <summary>
		/// Получает или создает новый Guid. Uow необходим для сохранения группы.
		/// </summary>
		public virtual Guid GetOrCreateGuid(IUnitOfWork uow)
		{
			if(OnlineStoreGuid == null)
			{
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
			return OnlineStoreGuid.Value;
		}

		public ProductGroup()
		{
		}
	}
}
