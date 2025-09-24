using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NHibernate;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "группы товаров",
		Nominative = "группа товаров",
		GenitivePlural = "групп товаров")]
	[EntityPermission]
	[HistoryTrace]
	public class ProductGroup : PropertyChangedBase, INamedDomainObject, IValidatableObject, IArchivable
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();

		private bool _isHighlightInCarLoadDocument;
		private bool _isNeedAdditionalControl;

		#region Свойства

		public virtual int Id { get; set; }

		private string name;
		[Display(Name = "Название")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value, () => Name);
		}
		
		private ProductGroup parent;
		[Display(Name = "Родительская группа")]
		public virtual ProductGroup Parent {
			get => parent;
			set => SetField(ref parent, value);
		}
		
		private IList<ProductGroup> childs;
		[Display(Name = "Дочерние группы")]
		public virtual IList<ProductGroup> Childs {
			get => childs;
			set => SetField(ref childs, value);
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
			get => exportToOnlineStore;
			set => SetField(ref exportToOnlineStore, value);
		}
		
		private Guid? onlineStoreGuid;
		[Display(Name = "Guid интернет магазина")]
		public virtual Guid? OnlineStoreGuid {
			get => onlineStoreGuid;
			set => SetField(ref onlineStoreGuid, value);
		}

		
		private OnlineStore onlineStore;
		[Display(Name = "Интернет-магазин")]
		public virtual OnlineStore OnlineStore {
			get => onlineStore;
			set => SetField(ref onlineStore, value);
		}

		private bool isArchive;
		[Display(Name = "Группа архивирована")]
		public virtual bool IsArchive {
			get => isArchive;
			set => SetField(ref isArchive, value);
		}

		[Display(Name = "Выделять в талонах погрузки авто")]
		public virtual bool IsHighlightInCarLoadDocument
		{
			get => _isHighlightInCarLoadDocument;
			set => SetField(ref _isHighlightInCarLoadDocument, value);
		}

		[Display(Name = "Требует доп. контроля водителя")]
		public virtual bool IsNeedAdditionalControl
		{
			get => _isNeedAdditionalControl;
			set => SetField(ref _isNeedAdditionalControl, value);
		}

		[Display(Name = "Характеристики товаров")]
		public virtual string CharacteristicsText {
			get => String.Join(",", characteristics);
			set {
				characteristics.Clear();
				if(string.IsNullOrWhiteSpace(value)) {
					return;
				}
				var parts = value.Split(',');
				foreach(var characteristic in parts) {
					if(Enum.TryParse<NomenclatureProperties>(characteristic, out var property))
						characteristics.Add(property);
					else
						logger.Error($"Характеристика товара {characteristic}, не была найдена в перечислении {nameof(NomenclatureProperties)}, поэтому была отброшена.");
				}
			}
		}

		private List<NomenclatureProperties> characteristics = new List<NomenclatureProperties>();
		[Display(Name = "Характеристики товаров")]
		public virtual List<NomenclatureProperties> Characteristics {
			get => characteristics;
			set => SetField(ref characteristics, value);
		}

		#endregion

		#region Функции

		public virtual void SetIsArchiveRecursively(bool value)
		{
			IsArchive = value;
			foreach(var child in Childs)
			{
				child.SetIsArchiveRecursively(value);
			}
		}

		public virtual void SetIsHighlightInCarLoadDocumenToAllChildGroups(bool value)
		{
			IsHighlightInCarLoadDocument = value;
			foreach(var child in Childs)
			{
				child.SetIsHighlightInCarLoadDocumenToAllChildGroups(value);
			}
		}

		public virtual void SetIsNeedAdditionalControlToAllChildGroups(bool value)
		{
			IsNeedAdditionalControl = value;
			foreach(var child in Childs)
			{
				child.SetIsNeedAdditionalControlToAllChildGroups(value);
			}
		}

		public virtual void CreateGuidIfNotExist(IUnitOfWork uow)
		{
			if(OnlineStoreGuid == null && ExportToOnlineStore) {
				OnlineStoreGuid = Guid.NewGuid();
				uow.Save(this);
			}
		}

		private bool isChildsFetched = false;
		public virtual void FetchChilds(IUnitOfWork uow)
		{
			if(isChildsFetched)
				return;
			
			uow.Session.QueryOver<ProductGroup>().Fetch(SelectMode.Fetch, x => x.Childs).List();
			isChildsFetched = true;
		}
		
		/// <summary>
		/// Входит ли переданная группа товаров в эту группу товаров
		/// </summary>
		/// <returns><c>true</c>, если входит, <c>false</c> если не входит.</returns>
		/// <param name="productGroup">Проверяемая группа товаров</param>
		public virtual bool IsBelongsOf(ProductGroup productGroup)
		{
			return Id == productGroup.Id || IsChildOfParentGroup(productGroup);
		}

		/// <summary>
		/// Является ли группа подгруппой другой группы товаров?
		/// </summary>
		/// <returns><c>true</c>, если является, <c>false</c> если не является.</returns>
		/// <param name="productGroup">Головная группа</param>
		public virtual bool IsChildOf(ProductGroup productGroup)
		{
			return this != productGroup && IsChildOfParentGroup(productGroup);
		}
		
		/// <summary>
		/// Является ли группа подгруппой другой группы товаров?
		/// </summary>
		/// <returns><c>true</c>, если является, <c>false</c> если не является.</returns>
		/// <param name="productGroupId">Id головной группы</param>
		public virtual bool IsChildOf(int productGroupId)
		{
			if(Id == productGroupId)
			{
				return false;
			}

			var parentGroup = Parent;

			while(parentGroup != null)
			{
				if(parentGroup.Id == productGroupId)
				{
					return true;
				}

				parentGroup = parentGroup.Parent;
			}

			return false;
		}
		
		private bool IsChildOfParentGroup(ProductGroup productGroup)
		{
			var parentGroup = Parent;

			while(parentGroup != null)
			{
				if(parentGroup == productGroup)
				{
					return true;
				}

				parentGroup = parentGroup.Parent;
			}

			return false;
		}

		#endregion

		#region IValidatableObject Implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(CheckCircle(this, parent)) {
				yield return new ValidationResult(
					"Родитель не назначен, так как возникает зацикливание", 
					new[] { nameof(Parent) }
				);
			}
			
			if(String.IsNullOrWhiteSpace(name)) {
				yield return new ValidationResult(
					"Название должно быть заполнено.", 
					new[] { nameof(Name) }
				);
			} else if(name.Length > 100) {
				yield return new ValidationResult(
					"Длина поля \"Название\" должна бать меньше 100 символов", 
					new[] { nameof(Name) }
				);
			}
		}

		#endregion

		#region Статические функции

		public static bool CheckCircle(ProductGroup group, ProductGroup parent)
		{
			if(parent == null)
				return false;
			return group == parent || CheckCircle(group, parent.Parent);
		}

		public static ProductGroup GetRootParent(ProductGroup group)
		{
			return group.Parent == null ? group : GetRootParent(group.Parent);
		}

		#endregion
	}
}
