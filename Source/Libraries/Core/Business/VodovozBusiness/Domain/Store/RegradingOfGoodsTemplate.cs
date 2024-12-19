using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;

namespace Vodovoz.Domain.Store
{
	/// <summary>
	/// Шаблон пересортицы
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны пересортицы",
		Nominative = "шаблон пересортицы")]
	[EntityPermission]
	public class RegradingOfGoodsTemplate : PropertyChangedBase, IDomainObject
	{
		private string _name;
		private IList<RegradingOfGoodsTemplateItem> _items = new List<RegradingOfGoodsTemplateItem>();
		private GenericObservableList<RegradingOfGoodsTemplateItem> _observableItems;

		/// <summary>
		/// Конструктор
		/// </summary>
		public RegradingOfGoodsTemplate()
		{
		}

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

		/// <summary>
		/// Название
		/// </summary>
		[Required(ErrorMessage = "Название шаблона должно быть заполнено.")]
		[Display(Name = "Название")]
		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		/// <summary>
		/// Строки
		/// </summary>
		[Display(Name = "Строки")]
		public virtual IList<RegradingOfGoodsTemplateItem> Items
		{
			get => _items;
			set
			{
				SetField(ref _items, value);
				_observableItems = null;
			}
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		/// <summary>
		/// Строки
		/// </summary>
		public virtual GenericObservableList<RegradingOfGoodsTemplateItem> ObservableItems
		{
			get
			{
				if(_observableItems == null)
				{
					_observableItems = new GenericObservableList<RegradingOfGoodsTemplateItem>(Items);
				}

				return _observableItems;
			}
		}

		/// <summary>
		/// Добавление строки шаблона пересортицы
		/// </summary>
		/// <param name="item"></param>
		public virtual void AddItem(RegradingOfGoodsTemplateItem item)
		{
			item.Template = this;
			ObservableItems.Add(item);
		}
	}
}
