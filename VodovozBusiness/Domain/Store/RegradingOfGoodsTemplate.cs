using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QSOrmProject;

namespace Vodovoz.Domain.Store
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны пересортицы",
		Nominative = "шаблон пересортицы")]
	[EntityPermission]
	public class RegradingOfGoodsTemplate: PropertyChangedBase, IDomainObject
	{

		#region Свойства

		public virtual int Id { get; set; }

		string name;

		[Required (ErrorMessage = "Название фуры должно быть заполнено.")]
		[Display (Name = "Название")]
		public virtual string Name {
			get { return name; }
			set { SetField (ref name, value, () => Name); }
		}

		#endregion

		IList<RegradingOfGoodsTemplateItem> items = new List<RegradingOfGoodsTemplateItem> ();

		[Display (Name = "Строки")]
		public virtual IList<RegradingOfGoodsTemplateItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<RegradingOfGoodsTemplateItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<RegradingOfGoodsTemplateItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<RegradingOfGoodsTemplateItem> (Items);
				return observableItems;
			}
		}

		public RegradingOfGoodsTemplate ()
		{
		}

		#region

		public virtual void AddItem(RegradingOfGoodsTemplateItem item)
		{
			item.Template = this;
			ObservableItems.Add(item);
		}

		#endregion
	}
}

