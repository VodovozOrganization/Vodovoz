using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.Extensions.Observable.Collections.List;
using System.ComponentModel.DataAnnotations;

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
		private IObservableList<RegradingOfGoodsTemplateItem> _items = new ObservableList<RegradingOfGoodsTemplateItem>();

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
		public virtual IObservableList<RegradingOfGoodsTemplateItem> Items
		{
			get => _items;
			set => SetField(ref _items, value);
		}

		/// <summary>
		/// Добавление строки шаблона пересортицы
		/// </summary>
		/// <param name="item"></param>
		public virtual void AddItem(RegradingOfGoodsTemplateItem item)
		{
			item.Template = this;
			Items.Add(item);
		}
	}
}
